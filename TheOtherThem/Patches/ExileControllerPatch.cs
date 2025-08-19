using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Objects;
using TheOtherThem.Roles;
using UnityEngine;
using static TheOtherThem.Roles.TheOtherRolesGM;
using static TheOtherThem.TheOtherRoles;

namespace TheOtherThem.Patches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    class ExileControllerBeginPatch
    {
        public static void Prefix(ExileController __instance, [HarmonyArgument(0)] ref ExileController.InitProperties init)
        {
            var exiled = init.networkedPlayer;
            var tie = init.voteTie;

            // Medic shield
            if (Medic.medic != null && AmongUsClient.Instance.AmHost && Medic.futureShielded != null && !Medic.medic.Data.IsDead)
            { // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                writer.Write(Medic.futureShielded.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.medicSetShielded(Medic.futureShielded.PlayerId);
            }

            // Shifter shift
            if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null)
            { // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ShifterShift, Hazel.SendOption.Reliable, -1);
                writer.Write(Shifter.futureShift.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.shifterShift(Shifter.futureShift.PlayerId);
            }
            Shifter.futureShift = null;

            // Eraser erase
            if (Eraser.eraser != null && AmongUsClient.Instance.AmHost && Eraser.futureErased != null)
            {  // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
                foreach (PlayerControl target in Eraser.futureErased)
                {
                    if (target != null && target.CanBeErased())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ErasePlayerRoles, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.erasePlayerRoles(target.PlayerId);
                    }
                }
            }
            Eraser.futureErased = new List<PlayerControl>();

            // Trickster boxes
            if (Trickster.trickster != null && JackInTheBox.HasJackInTheBoxLimitReached())
            {
                JackInTheBox.ConvertToVents();
            }

            // Witch execute casted spells
            if (Witch.witch != null && Witch.futureSpelled != null && AmongUsClient.Instance.AmHost)
            {
                bool exiledIsWitch = exiled != null && exiled.PlayerId == Witch.witch.PlayerId;
                bool witchDiesWithExiledLover = exiled != null && Lovers.BothDie && exiled.Object.IsInLove() && exiled.Object.GetPartner() == Witch.witch;

                if ((witchDiesWithExiledLover || exiledIsWitch) && Witch.witchVoteSavesTargets) Witch.futureSpelled = new List<PlayerControl>();
                foreach (PlayerControl target in Witch.futureSpelled)
                {
                    if (target != null && !target.Data.IsDead && Helpers.CheckMuderAttempt(Witch.witch, target, true) == MurderAttemptResult.PerformKill)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.WitchSpellCast, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.witchSpellCast(target.PlayerId);
                    }
                }
            }
            Witch.futureSpelled = new List<PlayerControl>();

            // SecurityGuard vents and cameras
            var allCameras = ShipStatus.Instance.AllCameras.ToList();
            MapOptions.CamerasToAdd.ForEach(camera =>
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                allCameras.Add(camera);
            });
            ShipStatus.Instance.AllCameras = allCameras.ToArray();
            MapOptions.CamerasToAdd = new List<SurvCamera>();

            foreach (Vent vent in MapOptions.VentsToSeal)
            {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>();
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                vent.myRend.color = Color.white;
                vent.name = "SealedVent_" + vent.name;
            }
            MapOptions.VentsToSeal = new List<Vent>();

            // 1 = reset per turn
            if (MapOptions.RestrictDevices == 1)
                MapOptions.resetDeviceTimes();
        }
    }

    [HarmonyPatch]
    class ExileControllerWrapUpPatch
    {

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                WrapUpPostfix(__instance.initData.networkedPlayer);
            }
        }

        static void WrapUpPostfix(NetworkedPlayerInfo exiled)
        {
            // Mini exile lose condition
            if (exiled != null && Mini.mini != null && Mini.mini.PlayerId == exiled.PlayerId && !Mini.isGrownUp() && !Mini.mini.Data.Role.IsImpostor)
            {
                Mini.triggerMiniLose = true;
            }

            // Jester win condition
            else if (exiled != null && Jester.jester != null && Jester.jester.PlayerId == exiled.PlayerId)
            {
                Jester.triggerJesterWin = true;
            }
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    class ExileControllerReEnableGameplayPatch
    {
        public static void Postfix(ExileController __instance)
        {
            // Reset custom button timers where necessary
            CustomButton.MeetingEndedUpdate();

            // Custom role post-meeting functions
            TheOtherRolesGM.OnMeetingEnd();

            // Mini set adapted cooldown
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini && Mini.mini.Data.Role.IsImpostor)
            {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().KillCooldown * multiplier);
            }

            // Seer spawn souls
            if (Seer.DeadBodyPositions != null && Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && (Seer.mode == 0 || Seer.mode == 2))
            {
                foreach (Vector3 pos in Seer.DeadBodyPositions)
                {
                    GameObject soul = new();
                    soul.transform.position = pos;
                    soul.layer = 5;
                    var rend = soul.AddComponent<SpriteRenderer>();
                    rend.sprite = Seer.GetSoulSprite();

                    if (Seer.limitSoulDuration)
                    {
                        HudManager.Instance.StartCoroutine(Effects.Lerp(Seer.SoulDuration, new Action<float>((p) =>
                        {
                            if (rend != null)
                            {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }
                            if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                        })));
                    }
                }
                Seer.DeadBodyPositions = new();
            }

            // Tracker reset deadBodyPositions
            Tracker.DeadBodyPositions = new();

            // Arsonist deactivate dead poolable players
            Arsonist.UpdateIcons();

            // Force Bounty Hunter Bounty Update
            if (BountyHunter.bountyHunter != null && BountyHunter.bountyHunter == PlayerControl.LocalPlayer)
                BountyHunter.bountyUpdateTimer = 0f;

            // Medium spawn souls
            if (Medium.medium != null && PlayerControl.LocalPlayer == Medium.medium)
            {
                if (Medium.souls != null)
                {
                    foreach (SpriteRenderer sr in Medium.souls) UnityEngine.Object.Destroy(sr.gameObject);
                    Medium.souls = new List<SpriteRenderer>();
                }

                if (Medium.featureDeadBodies != null)
                {
                    foreach ((DeadPlayer db, Vector3 ps) in Medium.featureDeadBodies)
                    {
                        GameObject s = new();
                        s.transform.position = ps;
                        s.layer = 5;
                        var rend = s.AddComponent<SpriteRenderer>();
                        rend.sprite = Medium.getSoulSprite();
                        Medium.souls.Add(rend);
                    }
                    Medium.deadBodies = Medium.featureDeadBodies;
                    Medium.featureDeadBodies = new List<Tuple<DeadPlayer, Vector3>>();
                }
            }

            if (Lawyer.lawyer != null && PlayerControl.LocalPlayer == Lawyer.lawyer && !Lawyer.lawyer.Data.IsDead)
                Lawyer.meetings++;
        }
    }

    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) })]
    class ExileControllerMessagePatch
    {
        static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
        {
            try
            {
                if (ExileController.Instance != null && ExileController.Instance.initData.networkedPlayer != null)
                {
                    PlayerControl player = ExileController.Instance.initData.networkedPlayer.Object;
                    if (player == null) return;
                    // Exile role text
                    if (id == StringNames.ExileTextPN || id == StringNames.ExileTextSN || id == StringNames.ExileTextPP || id == StringNames.ExileTextSP)
                    {
                        __result = string.Format(ModTranslation.GetString("EjectionText"), player.Data.PlayerName, string.Join(" ", RoleInfo.GetRoleInfoForPlayer(player).Select(x => x.Name).ToArray()));
                    }
                    // Hide number of remaining impostors on Jester win
                    if (id == StringNames.ImpostorsRemainP || id == StringNames.ImpostorsRemainS)
                    {
                        if (Jester.jester != null && player.PlayerId == Jester.jester.PlayerId) __result = "";
                    }
                }
            }
            catch
            {
                // pass - Hopefully prevent leaving while exiling to softlock game
            }
        }
    }
}