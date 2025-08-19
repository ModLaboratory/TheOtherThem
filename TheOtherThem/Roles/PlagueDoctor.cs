using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Objects;
using UnityEngine;
using static TheOtherThem.Patches.PlayerControlFixedUpdatePatch;

namespace TheOtherThem.Roles
{
    [HarmonyPatch]
    public class PlagueDoctor : RoleBase<PlagueDoctor>
    {
        private static CustomButton plagueDoctorButton;
        public static Color color = new Color32(255, 192, 0, byte.MaxValue);

        public static Dictionary<int, PlayerControl> infected;
        public static Dictionary<int, float> progress;
        public static Dictionary<int, bool> dead;
        public static TMPro.TMP_Text statusText = null;
        public static TMPro.TMP_Text numInfectionsText = null;
        public static bool triggerPlagueDoctorWin = false;

        public PlayerControl currentTarget;
        public int numInfections = 0;
        public bool meetingFlag = false;

        public static Sprite plagueDoctorIcon;

        public static float infectCooldown { get { return CustomOptionHolder.plagueDoctorInfectCooldown.GetFloat(); } }
        public static int maxInfectable { get { return Mathf.RoundToInt(CustomOptionHolder.plagueDoctorNumInfections.GetFloat()); } }
        public static float infectDistance { get { return CustomOptionHolder.plagueDoctorDistance.GetFloat(); } }
        public static float infectDuration { get { return CustomOptionHolder.plagueDoctorDuration.GetFloat(); } }
        public static float immunityTime { get { return CustomOptionHolder.plagueDoctorImmunityTime.GetFloat(); } }

        public static bool infectKiller { get { return CustomOptionHolder.plagueDoctorInfectKiller.GetBool(); } }
        public static bool resetAfterMeeting
        {
            get
            {
                //return CustomOptionHolder.plagueDoctorResetMeeting.getBool();
                return false;
            }
        }
        public static bool canWinDead { get { return CustomOptionHolder.plagueDoctorWinDead.GetBool(); } }

        public PlagueDoctor()
        {
            RoleType = RoleId = RoleType.PlagueDoctor;

            numInfections = maxInfectable;
            meetingFlag = false;

            updateDead();
        }

        public override void OnMeetingStart()
        {
            meetingFlag = true;
        }

        public override void OnMeetingEnd()
        {
            if (resetAfterMeeting)
            {
                progress.Clear();
            }

            updateDead();

            HudManager.Instance.StartCoroutine(Effects.Lerp(immunityTime, new Action<float>((p) =>
            { // 5秒後から感染開始
                if (p == 1f)
                {
                    meetingFlag = false;
                }
            })));
        }

        public override void OnKill(PlayerControl target) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void OnDeath(PlayerControl killer = null)
        {
            if (killer != null && infectKiller)
            {
                byte targetId = killer.PlayerId;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlagueDoctorSetInfected, SendOption.Reliable, -1);
                writer.Write(targetId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.plagueDoctorInfected(targetId);
            }
        }

        public override void FixedUpdate()
        {
            if (Player == PlayerControl.LocalPlayer)
            {
                if (numInfections > 0 && Player.IsAlive())
                {
                    currentTarget = setTarget(untargetablePlayers: infected.Values.ToList());
                    setPlayerOutline(currentTarget, color);
                }

                if (!meetingFlag && (canWinDead || Player.IsAlive()))
                {
                    List<PlayerControl> newInfected = new List<PlayerControl>();
                    foreach (PlayerControl target in PlayerControl.AllPlayerControls)
                    { // 非感染プレイヤーのループ
                        if (target == Player || target.IsDead() || infected.ContainsKey(target.PlayerId) || target.inVent) continue;

                        // データが無い場合は作成する
                        if (!progress.ContainsKey(target.PlayerId))
                        {
                            progress[target.PlayerId] = 0f;
                        }

                        foreach (var source in infected.Values.ToList())
                        { // 感染プレイヤーのループ
                            if (source.IsDead()) continue;
                            float distance = Vector3.Distance(source.transform.position, target.transform.position);
                            // 障害物判定
                            bool anythingBetween = PhysicsHelpers.AnythingBetween(source.GetTruePosition(), target.GetTruePosition(), Constants.ShipAndObjectsMask, false);

                            if (distance <= infectDistance && !anythingBetween)
                            {
                                progress[target.PlayerId] += Time.fixedDeltaTime;

                                // 他のクライアントに進行状況を通知する
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlagueDoctorUpdateProgress, SendOption.Reliable, -1);
                                writer.Write(target.PlayerId);
                                writer.Write(progress[target.PlayerId]);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);

                                // Only update a player's infection once per FixedUpdate
                                break;
                            }
                        }

                        // 既定値を超えたら感染扱いにする
                        if (progress[target.PlayerId] >= infectDuration)
                        {
                            newInfected.Add(target);
                        }
                    }

                    // 感染者に追加する
                    foreach (PlayerControl p in newInfected)
                    {
                        byte targetId = p.PlayerId;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlagueDoctorSetInfected, SendOption.Reliable, -1);
                        writer.Write(targetId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.plagueDoctorInfected(targetId);
                    }

                    // 勝利条件を満たしたか確認する
                    bool winFlag = true;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (p.IsDead()) continue;
                        if (p == Player) continue;
                        if (!infected.ContainsKey(p.PlayerId))
                        {
                            winFlag = false;
                            break;
                        }
                    }

                    if (winFlag)
                    {
                        MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlagueDoctorWin, SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                        RpcProcedure.plagueDoctorWin();
                    }
                }
            }
            UpdateStatusText();
        }

        public void UpdateStatusText()
        {
            if (MeetingHud.Instance != null)
            {
                if (statusText != null)
                {
                    statusText.gameObject.SetActive(false);
                }
                return;
            }

            if (Player != null && PlayerControl.LocalPlayer == Player || PlayerControl.LocalPlayer.IsDead())
            {
                if (statusText == null)
                {
                    var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                    var obj = Object.Instantiate(HudManager.Instance.AbilityButton.buttonLabelText);
                    statusText = obj.GetComponent<TMPro.TMP_Text>();
                    statusText.transform.position = new Vector3(-0.5f, position.y - 0.1f, -14f);
                    statusText.transform.localScale = new Vector3(1f, 1f, 1f);
                    statusText.fontSize = 1.5f;
                    statusText.fontSizeMin = 1.5f;
                    statusText.fontSizeMax = 1.5f;
                    statusText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                    statusText.transform.parent = HudManager.Instance.transform.parent;
                }

                statusText.gameObject.SetActive(true);
                string text = $"[{ModTranslation.GetString("plagueDoctorProgress")}]\n";
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (p == Player) continue;
                    if (dead.ContainsKey(p.PlayerId) && dead[p.PlayerId]) continue;
                    text += $"{p.name}: ";
                    if (infected.ContainsKey(p.PlayerId))
                    {
                        text += Helpers.ColorString(Color.red, ModTranslation.GetString("plagueDoctorInfectedText"));
                    }
                    else
                    {
                        // データが無い場合は作成する
                        if (!progress.ContainsKey(p.PlayerId))
                        {
                            progress[p.PlayerId] = 0f;
                        }
                        text += getProgressString(progress[p.PlayerId]);
                    }
                    text += "\n";
                }

                statusText.text = text;
            }
        }

        public static void MakeButtons(HudManager hm)
        {
            plagueDoctorButton = new CustomButton(
                () =>
                {/*ボタンが押されたとき*/
                    byte targetId = Local.currentTarget.PlayerId;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlagueDoctorSetInfected, SendOption.Reliable, -1);
                    writer.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.plagueDoctorInfected(targetId);
                    Local.numInfections--;

                    plagueDoctorButton.Timer = plagueDoctorButton.MaxTimer;
                    Local.currentTarget = null;
                },
                () => {/*ボタンが有効になる条件*/ return PlayerControl.LocalPlayer.IsRole(RoleType.PlagueDoctor) && Local.numInfections > 0 && !PlayerControl.LocalPlayer.IsDead(); },
                () =>
                {/*ボタンが使える条件*/
                    if (numInfectionsText != null)
                    {
                        if (Local.numInfections > 0)
                            numInfectionsText.text = string.Format(ModTranslation.GetString("plagueDoctorInfectionsLeft"), Local.numInfections);
                        else
                            numInfectionsText.text = "";
                    }

                    return Local.currentTarget != null && Local.numInfections > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => {/*ミーティング終了時*/ plagueDoctorButton.Timer = plagueDoctorButton.MaxTimer; },
                getSyringeIcon(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.UseButton,
                KeyCode.F
            );
            plagueDoctorButton.buttonText = ModTranslation.GetString("plagueDoctorInfectButton");

            numInfectionsText = Object.Instantiate(plagueDoctorButton.actionButton.cooldownTimerText, plagueDoctorButton.actionButton.cooldownTimerText.transform.parent);
            numInfectionsText.text = "";
            numInfectionsText.enableWordWrapping = false;
            numInfectionsText.transform.localScale = Vector3.one * 0.5f;
            numInfectionsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static Sprite getSyringeIcon()
        {
            if (plagueDoctorIcon) return plagueDoctorIcon;
            plagueDoctorIcon = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.InfectButton.png", 115f);
            return plagueDoctorIcon;
        }

        public static void SetButtonCooldowns()
        {
            plagueDoctorButton.MaxTimer = infectCooldown;
        }

        public static void updateDead()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                dead[pc.PlayerId] = pc.IsDead();
            }
        }

        public static string getProgressString(float progress)
        {
            // Go from green -> yellow -> red based on infection progress
            Color color;
            var prog = progress / infectDuration;
            if (prog < 0.5f)
                color = Color.Lerp(Color.green, Color.yellow, prog * 2);
            else
                color = Color.Lerp(Color.yellow, Color.red, prog * 2 - 1);

            float progPercent = prog * 100;
            return Helpers.ColorString(color, $"{progPercent.ToString("F1")}%");
        }

        public static void Clear()
        {
            Players = new List<PlagueDoctor>();
            triggerPlagueDoctorWin = false;
            infected = new Dictionary<int, PlayerControl>();
            progress = new Dictionary<int, float>();
            dead = new Dictionary<int, bool>();
        }
    }
}