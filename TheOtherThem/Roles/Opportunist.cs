using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class Opportunist : RoleBase<Opportunist>
    {
        public static Color color = new Color32(0, 255, 00, byte.MaxValue);

        public Opportunist()
        {
            RoleType = RoleId = RoleType.Opportunist;
        }

        public static void Clear()
        {
            Players = new List<Opportunist>();
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    }
}