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
        public static CustomOption WinningGuessedCount { get; set; }
        public static CustomOption SuicideCountdown { get; set; }
        public static CustomButton SelectTargetButton { get; set; }
        private static int TotalGuessed { get; set; } = 0;
        private static NetworkedPlayerInfo Target { get; set; }

        IdealistRole() : base("Idealist", Palette.Orange, 
           (nameKey, roleColor) => IdealistSpawnRate = new(2100, nameKey, roleColor, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, 1), 
           RoleType.Idealist, TeamTypeToT.Neutral, true)
        {
            Instance = this;
            // FIX: SOME FORTUNE TELLER'S OPTIONS ARE UNDER IDEALIST'S
            IdealistAbilityCooldown = CustomOption.CreateInsertable(2101, "IdealistAbilityCooldown", 30, 10, 60, 5, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
            WinningGuessedCount = CustomOption.CreateInsertable(2102, "IdealistWinningGuessedCount", 3, 3, 5, 1, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
            SuicideCountdown = CustomOption.CreateInsertable(2103, "IdealistSuicideCountdown", 30, 10, 60, 5, ref CustomOptionHolder.OptionInsertionIndexes.Neutral, IdealistSpawnRate);
        }

        public override void ClearData()
        {
            TotalGuessed = 0;
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
                                    TotalGuessed++;
                                Target = null;
                                timer.SetUnused();
                            }
                        },
                        () => Target.IsDead
                    );

                    timer.Start();
                });
            },
            CanLocalPlayerUse,
            () => Target == null,
            () => Target = null,
            ModTranslation.GetImage("SelectTargetButton", 230),
            null,
            HudManager.Instance,
            HudManager.Instance.KillButton,
            KeyCode.Z,
            buttonText: ModTranslation.GetString("IdealistAbilityLabel"));

            return new[]
            {
                (SelectTargetButton, IdealistAbilityCooldown.GetFloat())
            };
        }

        public override bool CanWin(ShipStatus ship)
        {
            if (TotalGuessed >= WinningGuessedCount.GetFloat())
            {
                RpcCustomEndGame(CustomGameOverReason.IdealistWin);
                return true;
            }
            return false;
        }
    }
}