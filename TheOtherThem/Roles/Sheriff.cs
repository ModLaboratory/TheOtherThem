using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using TheOtherThem.Objects;
using UnityEngine;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.Patches.PlayerControlFixedUpdatePatch;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class Sheriff : RoleBase<Sheriff>
    {
        private static CustomButton sheriffKillButton;
        public static TMPro.TMP_Text sheriffNumShotsText;

        public static Color color = new Color32(248, 205, 70, byte.MaxValue);

        public static float cooldown { get { return CustomOptionHolder.sheriffCooldown.GetFloat(); } }
        public static int maxShots { get { return Mathf.RoundToInt(CustomOptionHolder.sheriffNumShots.GetFloat()); } }
        public static bool canKillNeutrals { get { return CustomOptionHolder.sheriffCanKillNeutrals.GetBool(); } }
        public static bool misfireKillsTarget { get { return CustomOptionHolder.sheriffMisfireKillsTarget.GetBool(); } }
        public static bool spyCanDieToSheriff { get { return CustomOptionHolder.spyCanDieToSheriff.GetBool(); } }
        public static bool madmateCanDieToSheriff { get { return CustomOptionHolder.madmateCanDieToSheriff.GetBool(); } }

        public int numShots = 2;
        public PlayerControl currentTarget;

        public Sheriff()
        {
            RoleType = RoleId = RoleType.Sheriff;
            numShots = maxShots;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }

        public override void FixedUpdate() {
            if (Player == PlayerControl.LocalPlayer && numShots > 0)
            {
                currentTarget = setTarget();
                setPlayerOutline(currentTarget, Sheriff.color);
            }
        }

        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm) {

            // Sheriff Kill
            sheriffKillButton = new CustomButton(
                () =>
                {
                    if (Local.numShots <= 0)
                    {
                        return;
                    }

                    MurderAttemptResult murderAttemptResult = Helpers.CheckMuderAttempt(PlayerControl.LocalPlayer, Local.currentTarget);
                    if (murderAttemptResult == MurderAttemptResult.SuppressKill) return;

                    if (murderAttemptResult == MurderAttemptResult.PerformKill)
                    {
                        bool misfire = false;
                        byte targetId = Local.currentTarget.PlayerId; ;
                        if ((Local.currentTarget.Data.Role.IsImpostor && (Local.currentTarget != Mini.mini || Mini.isGrownUp())) ||
                            (Sheriff.spyCanDieToSheriff && Spy.spy == Local.currentTarget) ||
                            (Sheriff.madmateCanDieToSheriff && Local.currentTarget.HasModifier(ModifierType.Madmate)) ||
                            (Sheriff.canKillNeutrals && Local.currentTarget.IsNeutral()) ||
                            (Jackal.jackal == Local.currentTarget || Sidekick.sidekick == Local.currentTarget))
                        {
                            //targetId = Sheriff.currentTarget.PlayerId;
                            misfire = false;
                        }
                        else
                        {
                            //targetId = PlayerControl.LocalPlayer.PlayerId;
                            misfire = true;
                        }

                        // Mad sheriff always misfires.
                        if (Local.Player.HasModifier(ModifierType.Madmate))
                        {
                            misfire = true;
                        }
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SheriffKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(PlayerControl.LocalPlayer.Data.PlayerId);
                        killWriter.Write(targetId);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RpcProcedure.sheriffKill(PlayerControl.LocalPlayer.Data.PlayerId, targetId, misfire);
                    }

                    sheriffKillButton.Timer = sheriffKillButton.MaxTimer;
                    Local.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Sheriff) && Local.numShots > 0 && !PlayerControl.LocalPlayer.Data.IsDead; },
                () =>
                {
                    if (sheriffNumShotsText != null)
                    {
                        if (Local.numShots > 0)
                            sheriffNumShotsText.text = String.Format(ModTranslation.GetString("sheriffShots"), Local.numShots);
                        else
                            sheriffNumShotsText.text = "";
                    }
                    return Local.currentTarget && PlayerControl.LocalPlayer.CanMove;
                },
                () => { sheriffKillButton.Timer = sheriffKillButton.MaxTimer; },
                hm.KillButton.graphic.sprite,
                new Vector3(0f, 1f, 0),
                hm,
                hm.KillButton,
                KeyCode.Q
            );

            sheriffNumShotsText = GameObject.Instantiate(sheriffKillButton.actionButton.cooldownTimerText, sheriffKillButton.actionButton.cooldownTimerText.transform.parent);
            sheriffNumShotsText.text = "";
            sheriffNumShotsText.enableWordWrapping = false;
            sheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
            sheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }

        public static void SetButtonCooldowns()
        {
            sheriffKillButton.MaxTimer = Sheriff.cooldown;
        }

        public static void Clear()
        {
            Players = new List<Sheriff>();
        }
    }
}