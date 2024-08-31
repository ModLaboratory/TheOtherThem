using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.GameHistory;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class Template : RoleBase<Template>
    {
        public static Color color = Palette.CrewmateBlue;

        public Template()
        {
            RoleType = RoleId = RoleType.NoRole;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm) { }
        public static void SetButtonCooldowns() { }

        public static void Clear()
        {
            Players = new List<Template>();
        }
    }
}