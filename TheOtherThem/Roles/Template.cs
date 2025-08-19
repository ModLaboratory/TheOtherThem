using System.Collections.Generic;
using UnityEngine;

namespace TheOtherThem.Roles
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