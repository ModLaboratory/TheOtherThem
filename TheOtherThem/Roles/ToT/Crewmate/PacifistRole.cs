using Hazel;
using TheOtherThem.Objects;
using UnityEngine;

namespace TheOtherThem.ToTRole.Crewmate
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class PacifistRole : CustomRole
    {
        public static PacifistRole Instance { get; private set; }

        public static CustomRoleOption PacifistSpawnRate { get; set; }
        public static CustomOption PacifistAbilityCooldown { get; set; }
        public static CustomOption CooldownResetUsesLimit { get; set; }
        public static CustomButton ResetAllKillCooldownButton { get; set; }
        private int RemainingUses { get; set; }

        PacifistRole() : base("Pacifist", Palette.White,
           (nameKey, roleColor) => PacifistSpawnRate = new(2200, nameKey, roleColor, TeamTypeToT.Crewmate, 1),
           RoleType.Pacifist, TeamTypeToT.Crewmate, false)
        {
            Instance = this;

            PacifistAbilityCooldown = CustomOption.CreateInsertable(2201, "PacifistAbilityCooldown", 30, 10, 60, 5, TeamTypeToT.Crewmate, PacifistSpawnRate);
            CooldownResetUsesLimit = CustomOption.CreateInsertable(2202, "PacifistAbilityUsesLimit", 3, 1, 5, 1, TeamTypeToT.Crewmate, PacifistSpawnRate);
        }

        public override void ClearData()
        {
            RemainingUses = (int)CooldownResetUsesLimit.GetFloat();
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            ResetAllKillCooldownButton = new(() =>
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ResetAllKillCooldown, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                RemainingUses--;
                _ = new CustomMessage("PacifistAbilityRemainingUsesMessage", 3f, RemainingUses.ToString(), false);
                
                ResetAllKillCooldownButton.ResetTimer();
            },
            CanLocalPlayerUse,
            () => RemainingUses > 0,
            () => { },
            ModTranslation.GetImage("ResetCooldownButton", 230),
            null,
            HudManager.Instance,
            HudManager.Instance.KillButton,
            KeyCode.R,
            buttonText: ModTranslation.GetString("PacifistAbilityLabel"));

            return new[]
            {
                (ResetAllKillCooldownButton, PacifistAbilityCooldown.GetFloat())
            };
        }

        public override void OnRpcReceived(byte callId, MessageReader reader)
        {
            if (callId == (byte)CustomRpc.ResetAllKillCooldown)
            {
                HudManagerStartPatch.vampireKillButton.ResetTimer();
                PlayerControl.LocalPlayer.SetKillTimer(float.PositiveInfinity); // Set kill timer to cooldown set by host
            }
        }
    }
}