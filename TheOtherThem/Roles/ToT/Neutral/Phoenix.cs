//using System;
//using System.Linq;
//using TheOtherThem.Modules;
//using TheOtherThem.Objects;
//using TheOtherThem.Patches;
//using UnityEngine;
//using static Il2CppSystem.Xml.Schema.FacetsChecker.FacetsCompiler;

//namespace TheOtherThem.ToTRole.MyTeam
//{
//    //[RoleAutoInitialize]
//    [HarmonyPatch]
//    public class PhoenixRole : CustomRole
//    {
//        public static PhoenixRole Instance { get; private set; }

//        public static CustomRoleOption PhoenixRoleSpawnRate { get; set; }
//        public static CustomOption PhoenixRoleExampleOption { get; set; }

//        PhoenixRole() : base("PhoenixRole", new(),
//           (nameKey, roleColor) => PhoenixRoleSpawnRate = new(2300, nameKey, roleColor, TeamTypeToT.MY_TEAM, max__, enabled__),
//           RoleType.Phoenix, TeamTypeToT.Neutral, true)
//        {
//            Instance = this;

//            PhoenixRoleExampleOption = CustomOption.CreateInsertable(2301, "PhoenixRoleExampleOption", DEFAULT, MIN, MAX, STEP, TeamTypeToT.MY_TEAM, PhoenixRoleSpawnRate);
//        }

//        public override (CustomButton, float)[] CreateButtons()
//        {
//            return new[]
//            {
//                ((CustomButton)null, float.PositiveInfinity)
//            };
//        }

//        public override void OnRpcReceived(byte callId, MessageReader reader)
//        {

//        }

//        public override bool CanWin(ShipStatus ship)
//        {
//            return false;
//        }

//        public override void ClearData()
//        {

//        }
//    }
//}
