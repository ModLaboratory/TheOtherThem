using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Patches;
using UnityEngine;
using static TheOtherThem.GameHistory;
using static TheOtherThem.TheOtherRoles;

namespace TheOtherThem
{
    public class Couple
    {
        public PlayerControl Lover1 { get; set; }
        public PlayerControl Lover2 { get; set; }
        public Color Color { get; set; }

        public Couple(PlayerControl lover1, PlayerControl lover2, Color color)
        {
            Lover1 = lover1;
            Lover2 = lover2;
            Color = color;
        }

        public string Icon => Helpers.ColorString(Color, " ♥");
        
        public bool Existing => Lover1 != null && Lover2 != null && !Lover1.Data.Disconnected && !Lover2.Data.Disconnected;

        public bool Alive => Lover1 != null && Lover2 != null && Lover1.IsAlive() && Lover2.IsAlive();

        public bool ExistingAndAlive => Existing && Alive;

        public bool ExistingWithKiller => Existing && (Lover1 == Jackal.jackal || Lover2 == Jackal.jackal
                           || Lover1 == Sidekick.sidekick || Lover2 == Sidekick.sidekick
                           || Lover1.Data.Role.IsImpostor || Lover2.Data.Role.IsImpostor);

        public bool HasAliveKillingLover => ExistingAndAlive && ExistingWithKiller;
    }

    [HarmonyPatch]
    public static class Lovers
    {
        public static List<Couple> Couples { get; } = new();
        public static Color Color => new Color32(232, 57, 185, byte.MaxValue);

        public static List<Color> LoverIconColors { get; } = new List<Color>
        {
            Color,                  // pink
            new Color32(255, 165, 0, 255), // orange
            new Color32(255, 255, 0, 255), // yellow
            new Color32(0, 255, 0, 255),   // green
            new Color32(0, 0, 255, 255),   // blue
            new Color32(0, 255, 255, 255), // light blue
            new Color32(255, 0, 0, 255),   // red
        };

        public static bool BothDie => CustomOptionHolder.loversBothDie.GetBool();

        // Making this closer to the au.libhalt.net version of Lovers
        public static bool SeparateTeam => CustomOptionHolder.loversSeparateTeam.GetBool();
        public static bool TasksCount => CustomOptionHolder.loversTasksCount.GetBool();
        public static bool EnableChat => CustomOptionHolder.loversEnableChat.GetBool();

        public static bool HasTasks => TasksCount;

        public static string GetIcon(PlayerControl player)
        {
            if (IsInLove(player))
            {
                var couple = Couples.Find(x => x.Lover1 == player || x.Lover2 == player);
                return couple.Icon;
            }
            return "";
        }

        public static void AddCouple(PlayerControl player1, PlayerControl player2)
        {
            var availableColors = new List<Color>(LoverIconColors);
            Couples.ForEach(couple => availableColors.RemoveAll(x => x == couple.Color));
            Couples.Add(new Couple(player1, player2, availableColors[0]));
        }

        public static void EraseCouple(PlayerControl player)
        {
            Couples.RemoveAll(x => x.Lover1 == player || x.Lover2 == player);
        }

        public static void SwapLovers(PlayerControl player1, PlayerControl player2)
        {
            var couple1 = Couples.FindIndex(x => x.Lover1 == player1 || x.Lover2 == player1);
            var couple2 = Couples.FindIndex(x => x.Lover1 == player2 || x.Lover2 == player2);

            // trying to swap within the same couple, just ignore
            if (couple1 == couple2) return;

            if (couple1 >= 0)
            {
                if (Couples[couple1].Lover1 == player1) Couples[couple1].Lover1 = player2;
                if (Couples[couple1].Lover2 == player1) Couples[couple1].Lover2 = player2;
            }

            if (couple2 >= 0)
            {
                if (Couples[couple2].Lover1 == player2) Couples[couple2].Lover1 = player1;
                if (Couples[couple2].Lover2 == player2) Couples[couple2].Lover2 = player1;
            }
        }

        public static void KillLovers(PlayerControl player, PlayerControl killer = null)
        {
            if (!player.IsInLove()) return;

            if (SeparateTeam && TasksCount)
                player.ClearAllTasks();

            if (!BothDie) return;

            var partner = GetPartner(player);
            if (partner)
            {
                if (!partner.Data.IsDead)
                {
                    if (killer != null)
                        partner.MurderPlayerQuick(partner);
                    else
                        partner.Exiled();

                    FinalStatuses[partner.PlayerId] = FinalStatus.Suicide;
                }

                if (SeparateTeam && TasksCount)
                    partner.ClearAllTasks();
            }
        }

        public static bool IsInLove(PlayerControl player) => GetCouple(player) != null;

        public static PlayerControl GetPartner(PlayerControl player)
        {
            var couple = GetCouple(player);
            if (couple != null)
            {
                return player?.PlayerId == couple.Lover1?.PlayerId ? couple.Lover2 : couple.Lover1;
            }
            return null;
        }

        public static Couple GetCouple(PlayerControl player)
        {
            foreach (var pair in Couples)
            {
                if (pair.Lover1?.PlayerId == player?.PlayerId || pair.Lover2?.PlayerId == player?.PlayerId) return pair;
            }
            return null;
        }

        public static bool AnyAlive => Couples.Any(c => c.Alive);
        public static bool AnyNonKillingCouples => Couples.Any(c => !c.HasAliveKillingLover);
        public static bool Existing(PlayerControl player) => GetCouple(player)?.Existing == true;
        public static bool ExistingAndAlive(PlayerControl player) => GetCouple(player)?.ExistingAndAlive == true;
        public static bool ExistingWithKiller(PlayerControl player) => GetCouple(player)?.ExistingWithKiller == true;
        
        public static void HandleDisconnect(PlayerControl player, DisconnectReasons reason) => EraseCouple(player);

        public static void Clear() => Couples.Clear();
    }
}