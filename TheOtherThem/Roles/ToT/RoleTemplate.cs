#if false

using System;
using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem.ToTRole.MyTeam
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class MyRole : CustomRole
    {
        public static MyRole Instance { get; private set; }

        public static CustomRoleOption MyRoleSpawnRate { get; set; }
        public static CustomOption MyRoleExampleOption { get; set; }

        MyRole() : base("MyRole", new(),
           (nameKey, roleColor) => MyRoleSpawnRate = new(ROLE_ID, nameKey, roleColor, TeamTypeToT.MY_TEAM, max__, enabled__),
           RoleType.MY_TYPE, TeamTypeToT.MY_TEAM, winnable__, needsStatisticalWinningInfo__, winnableInsertionIndex__)
        {
            Instance = this;

            MyRoleExampleOption = CustomOption.CreateInsertable(2101, "MyRoleExampleOption", DEFAULT, MIN, MAX, STEP, TeamTypeToT.MY_TEAM, MyRoleSpawnRate);
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            return new[]
            {
                ((CustomButton)null, float.PositiveInfinity)
            };
        }

        public override void OnRpcReceived(byte callId, MessageReader reader)
        {

        }

        public override bool CanWin(ShipStatus ship)
        {
            return false;
        }

        public override bool CanWin(ShipStatus ship, PlayerStatistics statistics)
        {
            return false;
        }

        public override void ClearData()
        {
            
        }
    }
}

#endif