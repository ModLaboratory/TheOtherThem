using HarmonyLib;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using TheOtherThem.Objects;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherThem.Patches {
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        static void resetNameTagsAndColors() {
            Dictionary<byte, PlayerControl> playersById = Helpers.AllPlayersById();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                player.cosmetics.nameText.text = Helpers.ShouldHidePlayerName(PlayerControl.LocalPlayer, player) ? "" : player.CurrentOutfit.PlayerName;
                if (PlayerControl.LocalPlayer.IsImpostor() && player.IsImpostor()) {
                    player.cosmetics.nameText.color = Palette.ImpostorRed;
                } else {
                    player.cosmetics.nameText.color = Color.white;
                }
            }

            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                    PlayerControl playerControl = playersById.ContainsKey((byte)player.TargetPlayerId) ? playersById[(byte)player.TargetPlayerId] : null;
                    if (playerControl != null) {
                        player.NameText.text = playerControl.Data.PlayerName;
                        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && playerControl.Data.Role.IsImpostor) {
                            player.NameText.color = Palette.ImpostorRed;
                        } else {
                            player.NameText.color = Color.white;
                        }
                    }
                }
            }

            if (PlayerControl.LocalPlayer.IsImpostor()) {
                List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsImpostor()).ToList();
                foreach (PlayerControl player in impostors)
                    player.cosmetics.nameText.color = Palette.ImpostorRed;
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        PlayerControl playerControl = Helpers.PlayerById((byte)player.TargetPlayerId);
                        if (playerControl != null && playerControl.Data.Role.IsImpostor)
                            player.NameText.color =  Palette.ImpostorRed;
                    }
            }

        }

        static void setPlayerNameColor(PlayerControl p, Color color) {
            p.cosmetics.nameText.color = color;
            if (MeetingHud.Instance != null)
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && p.PlayerId == player.TargetPlayerId)
                        player.NameText.color = color;
        }

        static void setNameColors() {
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Jester))
                setPlayerNameColor(PlayerControl.LocalPlayer, Jester.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Mayor))
                setPlayerNameColor(PlayerControl.LocalPlayer, Mayor.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Engineer))
                setPlayerNameColor(PlayerControl.LocalPlayer, Engineer.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Sheriff))
                setPlayerNameColor(PlayerControl.LocalPlayer, Sheriff.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Lighter))
                setPlayerNameColor(PlayerControl.LocalPlayer, Lighter.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Detective))
                setPlayerNameColor(PlayerControl.LocalPlayer, Detective.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.TimeMaster))
                setPlayerNameColor(PlayerControl.LocalPlayer, TimeMaster.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Medic))
                setPlayerNameColor(PlayerControl.LocalPlayer, Medic.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Shifter))
                setPlayerNameColor(PlayerControl.LocalPlayer, Shifter.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Swapper))
                setPlayerNameColor(PlayerControl.LocalPlayer, Swapper.swapper.Data.Role.IsImpostor ? Palette.ImpostorRed : Swapper.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Seer))
                setPlayerNameColor(PlayerControl.LocalPlayer, Seer.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Hacker))
                setPlayerNameColor(PlayerControl.LocalPlayer, Hacker.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Tracker))
                setPlayerNameColor(PlayerControl.LocalPlayer, Tracker.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Snitch))
                setPlayerNameColor(PlayerControl.LocalPlayer, Snitch.color);
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Jackal))
            {
                // Jackal can see his sidekick
                setPlayerNameColor(PlayerControl.LocalPlayer, Jackal.color);
                if (Sidekick.sidekick != null)
                {
                    setPlayerNameColor(Sidekick.sidekick, Jackal.color);
                }
                if (Jackal.fakeSidekick != null)
                {
                    setPlayerNameColor(Jackal.fakeSidekick, Jackal.color);
                }
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Spy))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Spy.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.SecurityGuard))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, SecurityGuard.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Arsonist))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Arsonist.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.NiceGuesser))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Guesser.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.EvilGuesser))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Palette.ImpostorRed);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Bait))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Bait.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Opportunist))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Opportunist.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Vulture))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Vulture.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Medium))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Medium.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Lawyer))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Lawyer.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Pursuer))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Pursuer.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.PlagueDoctor))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, PlagueDoctor.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Fox))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Fox.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Immoralist))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Immoralist.color);
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.FortuneTeller) && (FortuneTeller.isCompletedNumTasks(PlayerControl.LocalPlayer) || PlayerControl.LocalPlayer.Data.IsDead))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, FortuneTeller.color);
            }

            if (PlayerControl.LocalPlayer.HasModifier(ModifierType.Madmate))
            {
                setPlayerNameColor(PlayerControl.LocalPlayer, Madmate.color);

                if (Madmate.knowsImpostors(PlayerControl.LocalPlayer))
                {
                    foreach (var p in PlayerControl.AllPlayerControls)
                    {
                        if (p.IsImpostor() || p.IsRole(RoleType.Spy))
                        {
                            setPlayerNameColor(p, Palette.ImpostorRed);
                        }
                    }
                }
            }

            if (GM.gm != null) {
                setPlayerNameColor(GM.gm, GM.color);
            }

            // No else if here, as a Lover of team Jackal needs the colors
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Sidekick)) {
                // Sidekick can see the jackal
                setPlayerNameColor(Sidekick.sidekick, Sidekick.color);
                if (Jackal.jackal != null) {
                    setPlayerNameColor(Jackal.jackal, Jackal.color);
                }
            }

            // No else if here, as the Impostors need the Spy name to be colored
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                setPlayerNameColor(Spy.spy, Spy.color);
            }

            if (Immoralist.Exists && PlayerControl.LocalPlayer.IsRole(RoleType.Fox))
            {
                foreach (var immoralist in Immoralist.AllPlayers)
                {
                    setPlayerNameColor(immoralist, Immoralist.color);
                }
            }

            if (PlayerControl.LocalPlayer.IsRole(RoleType.Immoralist))
            {
                foreach(var fox in Fox.AllPlayers)
                {
                    setPlayerNameColor(fox, Fox.color);
                }
            }

            // Crewmate roles with no changes: Mini
            // Impostor roles with no changes: Morphling, Camouflager, Vampire, Godfather, Eraser, Janitor, Cleaner, Warlock, BountyHunter,  Witch and Mafioso
        }

        static void setNameTags() {
            // Mafia
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.cosmetics.nameText.text == "") continue;
                    if (Godfather.godfather != null && Godfather.godfather == player)
                        player.cosmetics.nameText.text = player.Data.PlayerName + $" ({ModTranslation.GetString("mafiaG")})";
                    else if (Mafioso.mafioso != null && Mafioso.mafioso == player)
                        player.cosmetics.nameText.text = player.Data.PlayerName + $" ({ModTranslation.GetString("mafiaM")})";
                    else if (Janitor.janitor != null && Janitor.janitor == player)
                        player.cosmetics.nameText.text = player.Data.PlayerName + $" ({ModTranslation.GetString("mafiaJ")})";
                }
                if (MeetingHud.Instance != null)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (Godfather.godfather != null && Godfather.godfather.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Godfather.godfather.Data.PlayerName + $" ({ModTranslation.GetString("mafiaG")})";
                        else if (Mafioso.mafioso != null && Mafioso.mafioso.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Mafioso.mafioso.Data.PlayerName + $" ({ModTranslation.GetString("mafiaM")})";
                        else if (Janitor.janitor != null && Janitor.janitor.PlayerId == player.TargetPlayerId)
                            player.NameText.text = Janitor.janitor.Data.PlayerName + $" ({ModTranslation.GetString("mafiaJ")})";
            }

            bool meetingShow = MeetingHud.Instance != null && 
                (MeetingHud.Instance.state == MeetingHud.VoteStates.Voted ||
                 MeetingHud.Instance.state == MeetingHud.VoteStates.NotVoted ||
                 MeetingHud.Instance.state == MeetingHud.VoteStates.Discussion);
            
            // Lovers
            if (PlayerControl.LocalPlayer.IsInLove() && PlayerControl.LocalPlayer.IsAlive()) {
                string suffix = Lovers.GetIcon(PlayerControl.LocalPlayer);
                var lover1 = PlayerControl.LocalPlayer;
                var lover2 = PlayerControl.LocalPlayer.GetPartner();

                lover1.cosmetics.nameText.text += suffix;
                if (!Helpers.ShouldHidePlayerName(lover2))
                    lover2.cosmetics.nameText.text += suffix;

                if (meetingShow)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (lover1.PlayerId == player.TargetPlayerId || lover2.PlayerId == player.TargetPlayerId)
                            player.NameText.text += suffix;
            }
            else if (MapOptions.GhostsSeeRoles && PlayerControl.LocalPlayer.IsDead())
            {
                foreach (var couple in Lovers.Couples)
                {
                    string suffix = Lovers.GetIcon(couple.Lover1);
                    couple.Lover1.cosmetics.nameText.text += suffix;
                    couple.Lover2.cosmetics.nameText.text += suffix;

                    if (meetingShow)
                        foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                            if (couple.Lover1.PlayerId == player.TargetPlayerId || couple.Lover2.PlayerId == player.TargetPlayerId)
                                player.NameText.text += suffix;
                }
            }

            // Lawyer
            bool localIsLawyer = Lawyer.lawyer != null && Lawyer.target != null && Lawyer.lawyer == PlayerControl.LocalPlayer;
            bool localIsKnowingTarget = Lawyer.lawyer != null && Lawyer.target != null && Lawyer.targetKnows && Lawyer.target == PlayerControl.LocalPlayer;
            if (localIsLawyer || (localIsKnowingTarget && !Lawyer.lawyer.Data.IsDead)) {
                string suffix = Helpers.ColorString(Lawyer.color, " §");
                if (!Helpers.ShouldHidePlayerName(Lawyer.target))
                    Lawyer.target.cosmetics.nameText.text += suffix;

                if (meetingShow)
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                        if (player.TargetPlayerId == Lawyer.target.PlayerId)
                            player.NameText.text += suffix;
            }

            // Hacker and Detective
            if (PlayerControl.LocalPlayer != null && MapOptions.ShowLighterOrDarker) {
                if (meetingShow) {
                    foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates) {
                        var target = Helpers.PlayerById(player.TargetPlayerId);
                        if (target != null)  player.NameText.text += $" ({(Helpers.IsLighterColor(target.Data.DefaultOutfit.ColorId) ? ModTranslation.GetString("detectiveLightLabel") : ModTranslation.GetString("detectiveDarkLabel"))})";
                    }
                }
            }
        }

        static void updateShielded() {
            if (Medic.shielded == null) return;

            if (Medic.shielded.Data.IsDead || Medic.medic == null || Medic.medic.Data.IsDead) {
                Medic.shielded = null;
            }
        }

        static void timerUpdate() {
            Hacker.hackerTimer -= Time.deltaTime;
            Trickster.lightsOutTimer -= Time.deltaTime;
            Tracker.corpsesTrackingTimer -= Time.deltaTime;
        }

        public static void miniUpdate() {
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f) return;
                
            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            string suffix = "";
            if (growingProgress != 1f)
                suffix = " <color=#FAD934FF>(" + Mathf.FloorToInt(growingProgress * 18) + ")</color>"; 

            if (!Helpers.ShouldHidePlayerName(Mini.mini))
                Mini.mini.cosmetics.nameText.text += suffix;

            if (MeetingHud.Instance != null) {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                    if (player.NameText != null && Mini.mini.PlayerId == player.TargetPlayerId)
                        player.NameText.text += suffix;
            }

            if (Morphling.morphling != null && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f && !Helpers.ShouldHidePlayerName(Morphling.morphling))
                Morphling.morphling.cosmetics.nameText.text += suffix;
        }

        static void updateImpostorKillButton(HudManager __instance) {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || MeetingHud.Instance) return;
            bool enabled = Helpers.ShowButtons;
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Vampire))
                enabled &= false;
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Mafioso) && !Mafioso.canKill)
                enabled &= false;
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Janitor))
                enabled &= false;
            
            if (enabled) __instance.KillButton.Show();
            else __instance.KillButton.Hide();
        }

        static void camouflageAndMorphActions()
        {
            float oldCamouflageTimer = Camouflager.camouflageTimer;
            float oldMorphTimer = Morphling.morphTimer;

            Camouflager.camouflageTimer -= Time.deltaTime;
            Morphling.morphTimer -= Time.deltaTime;

            // Everyone but morphling reset
            if (oldCamouflageTimer > 0f && Camouflager.camouflageTimer <= 0f)
            {
                Camouflager.resetCamouflage();
            }

            // Morphling reset
            if (oldMorphTimer > 0f && Morphling.morphTimer <= 0f)
            {
                Morphling.resetMorph();
            }
        }

        static void Postfix(HudManager __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            CustomButton.HudUpdate();
            resetNameTagsAndColors();
            setNameColors();
            updateShielded();
            setNameTags();

            // Camouflager and Morphling
            camouflageAndMorphActions();

            // Impostors
            updateImpostorKillButton(__instance);
            // Timer updates
            timerUpdate();
            // Mini
            miniUpdate();
        }
    }
}
