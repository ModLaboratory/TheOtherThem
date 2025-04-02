using HarmonyLib;
using System;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherThem.Patches {
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance) {
            // Generate and initialize player icons
            if (PlayerControl.LocalPlayer != null && HudManager.Instance != null)
            {
                Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    NetworkedPlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, HudManager.Instance.transform);
                    player.UpdateFromPlayerOutfit(p.Data.DefaultOutfit, PlayerMaterial.MaskType.None, p.Data.IsDead, true);
                    player.SetFlipX(true);
                    player.TogglePet(false);
                    player.cosmetics.nameText.text = p.Data.DefaultOutfit.PlayerName;
                    MapOptions.PlayerIcons[p.PlayerId] = player;

                    if (PlayerControl.LocalPlayer == BountyHunter.bountyHunter)
                    {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    }
                    else if (PlayerControl.LocalPlayer == GM.gm)
                    {
                        player.transform.localPosition = Vector3.zero;
                        player.transform.localScale = Vector3.one * 0.3f;
                        player.SetSemiTransparent(false);
                        player.gameObject.SetActive(false);
                    }
                    else
                    {
                        player.gameObject.SetActive(false);
                    }
                }
            }

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            if (BountyHunter.bounty != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter) {
                BountyHunter.bountyUpdateTimer = 0f;
                if (HudManager.Instance != null) {
                    Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    BountyHunter.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(HudManager.Instance.KillButton.cooldownTimerText, HudManager.Instance.transform);
                    BountyHunter.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    BountyHunter.cooldownText.gameObject.SetActive(true);
                }
            }

            Arsonist.UpdateIcons();
            Morphling.resetMorph();
            Camouflager.resetCamouflage();

            if (PlayerControl.LocalPlayer == GM.gm && !GM.hasTasks)
            {
                PlayerControl.LocalPlayer.ClearAllTasks();
            }

            if (PlayerControl.LocalPlayer.IsGM())
            {
                HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
                HudManager.Instance.ReportButton.gameObject.SetActiveRecursively(false);
                HudManager.Instance.ReportButton.SetActive(false);
                HudManager.Instance.ReportButton.graphic.enabled = false;
                HudManager.Instance.ReportButton.enabled = false;
                HudManager.Instance.ReportButton.graphic.sprite = null;
                HudManager.Instance.ReportButton.buttonLabelText.enabled = false;
                HudManager.Instance.ReportButton.buttonLabelText.SetText("");

                HudManager.Instance.roomTracker.gameObject.SetActiveRecursively(false);
                HudManager.Instance.roomTracker.text.enabled = false;
                HudManager.Instance.roomTracker.text.SetText("");
                HudManager.Instance.roomTracker.enabled = false;
            }
        }
    }

    [HarmonyPatch]
    class IntroPatch {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            // Intro solo teams
            if (PlayerControl.LocalPlayer.IsNeutral() || PlayerControl.LocalPlayer == GM.gm) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Don't show the GM
            if (!PlayerControl.LocalPlayer.IsGM())
            {
                var newTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                foreach (PlayerControl p in yourTeam)
                {
                    if (p != GM.gm)
                        newTeam.Add(p);
                }
                yourTeam = newTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in players) {
                    if (PlayerControl.LocalPlayer != p && (p == Spy.spy || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            List<RoleInfo> infos = RoleInfo.GetRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.MyRoleType != RoleType.Lovers).FirstOrDefault();
            if (roleInfo == null) return;
            Main.Logger.LogInfo($"{PlayerControl.LocalPlayer.IsNeutral()} | {roleInfo.MyRoleType}");
            if (PlayerControl.LocalPlayer.IsNeutral() || PlayerControl.LocalPlayer.IsGM())
            {
                __instance.BackgroundBar.material.color = roleInfo.RoleColor;
                __instance.TeamTitle.text = roleInfo.Name;
                __instance.TeamTitle.color = roleInfo.RoleColor;
                __instance.ImpostorText.text = "";
            }
        }

        [HarmonyPatch(typeof(IntroCutscene._ShowRole_d__41), nameof(IntroCutscene._ShowRole_d__41.MoveNext))]
        class SetUpRoleTextPatch {
            public static void Postfix(IntroCutscene._ShowRole_d__41 __instance) {
                if (!CustomOptionHolder.activateRoles.GetBool()) return; // Don't override the intro of the vanilla roles

                List<RoleInfo> infos = RoleInfo.GetRoleInfoForPlayer(PlayerControl.LocalPlayer, new RoleType[] { RoleType.Lovers });
                RoleInfo roleInfo = infos.FirstOrDefault();

                var intro = __instance.__4__this;

                if (roleInfo != null && roleInfo != RoleInfo.crewmate && roleInfo != RoleInfo.impostor && !(roleInfo == RoleInfo.fortuneTeller && FortuneTeller.numTasks > 0)) {
                    intro.YouAreText.color = roleInfo.RoleColor;
                    intro.RoleText.text = roleInfo.Name;
                    intro.RoleText.color = roleInfo.RoleColor;
                    intro.RoleBlurbText.text = roleInfo.IntroDescription;
                    intro.RoleBlurbText.color = roleInfo.RoleColor;
                }

                if (PlayerControl.LocalPlayer.hasModifier(ModifierType.Madmate))
                {
                    if (roleInfo == RoleInfo.crewmate)
                    {
                        intro.RoleText.text = ModTranslation.GetString("madmate");
                    }
                    else
                    {
                        intro.RoleText.text = ModTranslation.GetString("madmatePrefix") + intro.RoleText.text;
                    }
                    intro.YouAreText.color = Madmate.color;
                    intro.RoleText.color = Madmate.color;
                    intro.RoleBlurbText.text = ModTranslation.GetString("madmateIntroDesc");
                    intro.RoleBlurbText.color = Madmate.color;
                }

                if (infos.Any(info => info.MyRoleType == RoleType.Lovers)) {
                    PlayerControl otherLover = PlayerControl.LocalPlayer.GetPartner();
                	intro.RoleBlurbText.text += "\n" + Helpers.ColorString(Lovers.color, String.Format(ModTranslation.GetString("loversFlavor"), otherLover?.Data?.PlayerName ?? ""));
                } 
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }
}

