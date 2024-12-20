using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem.ToTRole.Impostor
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class IdealistRole : CustomRole
    {
        public static IdealistRole Instance { get; private set; }
        public static CustomRoleOption IdealistSpawnRate { get; set; }
        public static CustomOption IdealistAbilityCooldown { get; set; }
        public static CustomOption WinningKilledCount { get; set; }
        public static CustomOption SuicideCountdown { get; set; }
        public static CustomButton SelectTargetButton { get; set; }
        private static int TotalKilled { get; set; } = 0;
        private static NetworkedPlayerInfo Target { get; set; }

        IdealistRole() : base("Idealist", Palette.Orange, 
           (nameKey, roleColor) => IdealistSpawnRate = new(2100, nameKey, roleColor, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, 1), 
           RoleType.Idealist, TeamTypeTOT.Neutral, true)
        {
            Instance = this;

            IdealistAbilityCooldown = CustomOption.CreateInsertable(2101, "IdealistAbilityCooldown", 30, 10, 60, 5, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
            WinningKilledCount = CustomOption.CreateInsertable(2102, "IdealistWinningKilledCount", 3, 3, 5, 1, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
            SuicideCountdown = CustomOption.CreateInsertable(2103, "IdealistSuicideCountdown", 30, 10, 60, 5, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
        }

        public override void ClearData()
        {
            TotalKilled = 0;
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            SelectTargetButton = new(() =>
            {
                PlayerPickMenu.OpenPlayerPickMenu(GameData.Instance.AllPlayers.ToArray().Where(n => n.PlayerId != PlayerControl.LocalPlayer.PlayerId && !n.IsDead && !n.Disconnected).ToList(), t =>
                {
                    Target = t;

                    var timer = new Timer();

                    timer = new("IdealistTargetCountdown",
                        SuicideCountdown.GetFloat(),
                        stoppedNormally =>
                        {
                            if (stoppedNormally)
                                PlayerControl.LocalPlayer.CmdCheckMurder(PlayerControl.LocalPlayer);
                            else
                            {
                                if (!Target.Disconnected)
                                    TotalKilled++;
                                Target = null;
                                timer.SetUnused();
                            }
                        },
                        () => Target.IsDead && GameHistory.DeadPlayers.Any(dp => dp.DeadInfo == Target && dp.KillerInfo != PlayerControl.LocalPlayer.Data)
                    );

                    timer.Start();
                });
            },
            CanLocalPlayerUse,
            () => Target == null,
            () => Target = null,
            null,
            null,
            HudManager.Instance,
            HudManager.Instance.KillButton,
            KeyCode.Z);

            return new[]
            {
                (SelectTargetButton, IdealistAbilityCooldown.GetFloat())
            };
        }

        public override bool CanWin(ShipStatus ship)
        {
            if (TotalKilled >= WinningKilledCount.GetFloat())
            {
                RpcCustomEndGame(CustomGameOverReason.IdealistWin);
                return true;
            }
            return false;
        }
    }
}