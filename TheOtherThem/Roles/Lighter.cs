using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheOtherThem.Objects;
using TheOtherThem.Patches;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class Lighter : RoleBase<Lighter>
    {
        private static CustomButton lighterButton;

        public static Color color = new Color32(238, 229, 190, byte.MaxValue);

        public static float lighterModeLightsOnVision { get { return CustomOptionHolder.lighterModeLightsOnVision.GetFloat(); } }
        public static float lighterModeLightsOffVision { get { return CustomOptionHolder.lighterModeLightsOffVision.GetFloat(); } }
        public static bool canSeeNinja { get { return CustomOptionHolder.lighterCanSeeNinja.GetBool(); } }

        public static float cooldown { get { return CustomOptionHolder.lighterCooldown.GetFloat(); } }
        public static float duration { get { return CustomOptionHolder.lighterDuration.GetFloat(); } }

        public bool lightActive = false;

        public Lighter()
        {
            RoleType = RoleId = RoleType.Lighter;
            lightActive = false;
        }

        public static bool isLightActive(PlayerControl player)
        {
            if (IsRole(player) && player.IsAlive())
            {
                Lighter r = Players.First(x => x.Player == player);
                return r.lightActive;
            }
            return false;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer = null) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static void MakeButtons(HudManager hm)
        {
            // Lighter light
            lighterButton = new CustomButton(
                () =>
                {
                    Local.lightActive = true;
                },
                () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Lighter) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    Local.lightActive = false;

                    lighterButton.Timer = lighterButton.MaxTimer;
                    lighterButton.isEffectActive = false;
                    lighterButton.actionButton.graphic.color = Palette.EnabledColor;
                },
                Lighter.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.UseButton,
                KeyCode.F,
                true,
                duration,
                () => {
                    Local.lightActive = false;
                    lighterButton.Timer = lighterButton.MaxTimer;
                }
            );
            lighterButton.buttonText = ModTranslation.GetString("LighterText");
        }

        public static void SetButtonCooldowns()
        {
            lighterButton.MaxTimer = cooldown;
            lighterButton.EffectDuration = duration;
        }

        public static void Clear()
        {
            Players = new List<Lighter>();
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = ModTranslation.GetImage("LighterButton", 115f);
            return buttonSprite;
        }
    }
}