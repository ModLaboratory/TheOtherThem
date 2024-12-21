using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.GameHistory;
using Hazel;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class NekoKabocha : RoleBase<NekoKabocha>
    {
        public static Color color = Palette.ImpostorRed;

        public static bool revengeCrew { get { return CustomOptionHolder.nekoKabochaRevengeCrew.GetBool(); } }
        public static bool revengeNeutral { get { return CustomOptionHolder.nekoKabochaRevengeNeutral.GetBool(); } }
        public static bool revengeImpostor { get { return CustomOptionHolder.nekoKabochaRevengeImpostor.GetBool(); } }
        public static bool revengeExile { get { return CustomOptionHolder.nekoKabochaRevengeExile.GetBool(); } }

        public PlayerControl meetingKiller = null;

        public NekoKabocha()
        {
            RoleType = RoleId = RoleType.NekoKabocha;
        }

        public override void OnMeetingStart() {
            meetingKiller = null;
        }

        public override void OnMeetingEnd()
        {
            meetingKiller = null;
        }

        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        
        public override void OnDeath(PlayerControl killer = null)
        {
            killer = killer ?? meetingKiller;
            if (killer != null && killer != Player && killer.IsAlive() && !killer.isGM())
            {
                if ((revengeCrew && killer.isCrew()) ||
                    (revengeNeutral && killer.IsNeutral()) ||
                    (revengeImpostor && killer.isImpostor()))
                {
                    if (meetingKiller == null)
                    {
                        Player.MurderPlayerQuick(killer);
                    }
                    else
                    {
                        killer.Exiled();
                        if (PlayerControl.LocalPlayer == killer)
                            HudManager.Instance.KillOverlay.ShowKillAnimation(Player.Data, killer.Data);
                    }

                    finalStatuses[killer.PlayerId] = FinalStatus.Revenge;
                }
            }
            else if (killer == null && revengeExile && PlayerControl.LocalPlayer == Player)
            {
                var candidates = PlayerControl.AllPlayerControls.ToArray().Where(x => x != Player && x.IsAlive()).ToList();
                int targetID = rnd.Next(0, candidates.Count);
                var target = candidates[targetID];

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.NekoKabochaExile, Hazel.SendOption.Reliable, -1);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.nekoKabochaExile(target.PlayerId);
            }
            meetingKiller = null;
        }

        public static void meetingKill(PlayerControl player, PlayerControl killer)
        {
            if (IsRole(player))
            {
                NekoKabocha n = Players.First(x => x.Player == player);
                n.meetingKiller = killer;
            }
        }

        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm) { }
        public static void SetButtonCooldowns() { }

        public static void clearAndReload()
        {
            Players = new List<NekoKabocha>();
        }
    }
}