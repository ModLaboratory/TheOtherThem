using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Objects;
using UnityEngine;

namespace TheOtherThem.ToTRole.Impostor
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class IdealistRole : CustomRole
    {
        public static IdealistRole Instance { get; private set; }
        public static CustomRoleOption IdealistSpawnRate { get; set; }
        public static CustomOption WinningKilledCount { get; set; }
        private static int TotalKilled { get; set; } = 0;

        IdealistRole() : base("Idealist", Palette.Orange, 
           (nameKey, roleColor) => IdealistSpawnRate = new(2100, nameKey, roleColor, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, 1), 
           RoleType.Idealist, TeamTypeTOT.Neutral)
        {
            Instance = this;

            WinningKilledCount = CustomOption.CreateInsertable(2101, "IdealistWinningKilledCount", 3, 3, 5, 1, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
        }

        public override void ClearData()
        {
            TotalKilled = 0;
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        [HarmonyPostfix]
        public static void UpdatePatch()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                PlayerPickMenu.OpenPlayerPickMenu(GameData.Instance.AllPlayers.ToArray().ToList(), (p) => { System.Console.WriteLine(p.PlayerName); });
        }
    }
}