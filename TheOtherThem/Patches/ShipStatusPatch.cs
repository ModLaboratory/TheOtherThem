using HarmonyLib;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;

namespace TheOtherThem.Patches {

    [HarmonyPatch(typeof(ShipStatus))]
    public class ShipStatusPatch {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
        public static bool Prefix(ref float __result, ShipStatus __instance, [HarmonyArgument(0)] NetworkedPlayerInfo player) {
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.Electrical) ? __instance.Systems[SystemTypes.Electrical] : null;
            if (systemType == null) return true;
            SwitchSystem switchSystem = systemType.TryCast<SwitchSystem>();
            if (switchSystem == null) return true;

            float num = (float)switchSystem.Value / 255f;
            
            if (player == null || player.IsDead || player.PlayerId == GM.gm?.PlayerId) // IsDead
                __result = __instance.MaxLightRadius;
            else if (player.Role.IsImpostor
                || (Jackal.jackal != null && Jackal.jackal.PlayerId == player.PlayerId && Jackal.hasImpostorVision)
                || (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == player.PlayerId && Sidekick.hasImpostorVision)
                || (Spy.spy != null && Spy.spy.PlayerId == player.PlayerId && Spy.hasImpostorVision)
                || (player.Object.HasModifier(ModifierType.Madmate) && Madmate.hasImpostorVision) // Impostor, Jackal/Sidekick, Spy, or Madmate with Impostor vision
                || (Jester.jester != null && Jester.jester.PlayerId == player.PlayerId && Jester.hasImpostorVision) // Jester with Impostor vision
                || (player.Object.IsRole(RoleType.Fox))
                )
                __result = __instance.MaxLightRadius * GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().ImpostorLightMod;
            else if (PlayerControl.LocalPlayer.IsRole(RoleType.Lighter) && Lighter.isLightActive(PlayerControl.LocalPlayer)) // if player is Lighter and Lighter has his ability active
                __result = Mathf.Lerp(__instance.MaxLightRadius * Lighter.lighterModeLightsOffVision, __instance.MaxLightRadius * Lighter.lighterModeLightsOnVision, num);
            else if (Trickster.trickster != null && Trickster.lightsOutTimer > 0f) {
                float lerpValue = 1f;
                if (Trickster.lightsOutDuration - Trickster.lightsOutTimer < 0.5f) lerpValue = Mathf.Clamp01((Trickster.lightsOutDuration - Trickster.lightsOutTimer) * 2);
                else if (Trickster.lightsOutTimer < 0.5) lerpValue = Mathf.Clamp01(Trickster.lightsOutTimer * 2);
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, 1 - lerpValue) * GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().CrewLightMod; // Instant lights out? Maybe add a smooth transition?
            }
            else if (Lawyer.lawyer != null && Lawyer.lawyer.PlayerId == player.PlayerId) // if player is Lighter and Lighter has his ability active
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius * Lawyer.vision, num);
            else
                __result = Mathf.Lerp(__instance.MinLightRadius, __instance.MaxLightRadius, num) * GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().CrewLightMod;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.IsGameOverDueToDeath))]
        public static void Postfix2(ref bool __result)
        {
            __result = false;
        }

        private static int originalNumCommonTasksOption = 0;
        private static int originalNumShortTasksOption = 0;
        private static int originalNumLongTasksOption = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static bool Prefix(ShipStatus __instance)
        {
            if (CustomOptionHolder.UselessOptions.GetBool() && CustomOptionHolder.playerColorRandom.GetBool() && AmongUsClient.Instance.AmHost)
            {
                List<int> colors = Enumerable.Range(0, Palette.PlayerColors.Count).ToList();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    int i = rnd.Next(0, colors.Count);
                    p.SetColor(colors[i]);
                    p.RpcSetColor((byte)colors[i]);
                    colors.RemoveAt(i);
                }
            }

            var commonTaskCount = __instance.CommonTasks.Count;
            var normalTaskCount = __instance.ShortTasks.Count;
            var longTaskCount = __instance.LongTasks.Count;
            var option = GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>();
            
            originalNumCommonTasksOption = option.NumCommonTasks;
            originalNumShortTasksOption = option.NumShortTasks;
            originalNumLongTasksOption = option.NumLongTasks;

            if (option.NumCommonTasks > commonTaskCount) option.NumCommonTasks = commonTaskCount;
            if (option.NumShortTasks > normalTaskCount) option.NumShortTasks = normalTaskCount;
            if (option.NumLongTasks > longTaskCount) option.NumLongTasks = longTaskCount;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static void Postfix3(ShipStatus __instance)
        {
            // Restore original settings after the tasks have been selected
            var option = GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>();
            option.NumCommonTasks = originalNumCommonTasksOption;
            option.NumShortTasks = originalNumShortTasksOption;
            option.NumLongTasks = originalNumLongTasksOption;
        }
            
    }

}
