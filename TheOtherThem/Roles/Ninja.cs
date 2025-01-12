using System.Linq;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using UnityEngine;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using AmongUs.GameOptions;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class Ninja : RoleBase<Ninja> {

        private static CustomButton ninjaButton;

        public static Color color = Palette.ImpostorRed;

        public static float stealthCooldown { get { return CustomOptionHolder.ninjaStealthCooldown.GetFloat(); } }
        public static float stealthDuration { get { return CustomOptionHolder.ninjaStealthDuration.GetFloat(); } }
        public static float killPenalty { get { return CustomOptionHolder.ninjaKillPenalty.GetFloat(); } }
        public static float speedBonus { get { return CustomOptionHolder.ninjaSpeedBonus.GetFloat() / 100f; } }
        public static float fadeTime { get { return CustomOptionHolder.ninjaFadeTime.GetFloat(); } }
        public static bool canUseVents { get { return CustomOptionHolder.ninjaCanVent.GetBool(); } }
        public static bool canBeTargeted { get { return CustomOptionHolder.ninjaCanBeTargeted.GetBool(); } }

        public bool penalized = false;
        public bool stealthed = false;
        public DateTime stealthedAt = DateTime.UtcNow;

        public Ninja()
        {
            RoleType = RoleId = RoleType.Ninja;
            penalized = false;
            stealthed = false;
            stealthedAt = DateTime.UtcNow;
        }

        public override void OnMeetingStart()
        {
            stealthed = false;
            ninjaButton.isEffectActive = false;
            ninjaButton.Timer = ninjaButton.MaxTimer = Ninja.stealthCooldown;
        }

        public override void OnMeetingEnd()
        {
            if (Player == PlayerControl.LocalPlayer)
            {
                if (penalized)
                {
                    Player.SetKillTimerUnchecked(GameManager.Instance.LogicOptions.GetKillCooldown() + killPenalty);
                }
                else
                {
                    Player.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV08>().KillCooldown);
                }
            }
        }

        public override void ResetRole()
        {
            penalized = false;
            stealthed = false;
            setOpacity(Player, 1.0f);
            ninjaButton.isEffectActive = false;
            ninjaButton.Timer = ninjaButton.MaxTimer = Ninja.stealthCooldown;
        }

        public override void FixedUpdate() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public static bool isStealthed(PlayerControl player)
        {
            if (IsRole(player) && player.IsAlive())
            {
                Ninja n = Players.First(x => x.Player == player);
                return n.stealthed;
            }
            return false;
        }

        public static float stealthFade(PlayerControl player)
        {
            if (IsRole(player) && fadeTime > 0f && player.IsAlive())
            {
                Ninja n = Players.First(x => x.Player == player);
                return Mathf.Min(1.0f, (float)(DateTime.UtcNow - n.stealthedAt).TotalSeconds / fadeTime);
            }
            return 1.0f;
        }

        public static bool isPenalized(PlayerControl player)
        {
            if (IsRole(player) && player.IsAlive())
            {
                Ninja n = Players.First(x => x.Player == player);
                return n.penalized;
            }
            return false;
        }

        public static void setStealthed(PlayerControl player, bool stealthed = true)
        {
            if (IsRole(player))
            {
                Ninja n = Players.First(x => x.Player == player);
                n.stealthed = stealthed;
                n.stealthedAt = DateTime.UtcNow;
            }
        }

        public override void OnKill(PlayerControl target)
        {
            penalized = stealthed;
            float penalty = penalized ? killPenalty : 0f;
            if (PlayerControl.LocalPlayer == Player)
                Player.SetKillTimerUnchecked(GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV08>().KillCooldown + penalty);
        }

        public override void OnDeath(PlayerControl killer)
        {
            stealthed = false;
            ninjaButton.isEffectActive = false;
        }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.NinjaButton.png", 115f);
            return buttonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            // Ninja stealth
            ninjaButton = new CustomButton(
                () => {
                    if (ninjaButton.isEffectActive)
                    {
                        ninjaButton.Timer = 0;
                        return;
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.NinjaStealth, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(true);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ninjaStealth(PlayerControl.LocalPlayer.PlayerId, true);
                },
                () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Ninja) && !PlayerControl.LocalPlayer.Data.IsDead; },
                () => {
                    if (ninjaButton.isEffectActive)
                    {
                        ninjaButton.buttonText = ModTranslation.GetString("NinjaUnstealthText");
                    }
                    else
                    {
                        ninjaButton.buttonText = ModTranslation.GetString("NinjaText");
                    }
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => {
                    ninjaButton.Timer = ninjaButton.MaxTimer = Ninja.stealthCooldown;
                },
                Ninja.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.KillButton,
                KeyCode.F,
                true,
                Ninja.stealthDuration,
                () => {
                    ninjaButton.Timer = ninjaButton.MaxTimer = Ninja.stealthCooldown;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.NinjaStealth, Hazel.SendOption.Reliable, -1);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ninjaStealth(PlayerControl.LocalPlayer.PlayerId, false);

                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(Math.Max(PlayerControl.LocalPlayer.killTimer, Ninja.killPenalty));
                }
            );
            ninjaButton.buttonText = ModTranslation.GetString("NinjaText");
            ninjaButton.effectCancellable = true;
        }

        public static void SetButtonCooldowns()
        {
            ninjaButton.MaxTimer = Ninja.stealthCooldown;
        }

        public static void Clear()
        {
            Players = new List<Ninja>();
        }

        public static void setOpacity(PlayerControl player, float opacity)
        {
            // Sometimes it just doesn't work?
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                player.cosmetics.currentBodySprite.BodySprite.color = color;
                if (player.cosmetics.HasHat()) player.cosmetics.hat.SpriteColor = color;
                player.GetPet()?.ForEachRenderer(true, new Action<SpriteRenderer>(r => r.color = color));
                player.cosmetics.CurrentVisor?.SetVisorColor(color);
            }
            catch { }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static class PlayerPhysicsNinjaPatch
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (__instance.AmOwner && __instance.myPlayer.CanMove && GameData.Instance && isStealthed(__instance.myPlayer))
                {
                    __instance.body.velocity *= speedBonus;
                }

                if (IsRole(__instance.myPlayer))
                {
                    var ninja = __instance.myPlayer;
                    if (ninja == null || ninja.IsDead()) return;

                    bool canSee = 
                        PlayerControl.LocalPlayer.IsImpostor() ||
                        PlayerControl.LocalPlayer.IsDead() ||
                        (Lighter.canSeeNinja && PlayerControl.LocalPlayer.IsRole(RoleType.Lighter) && Lighter.isLightActive(PlayerControl.LocalPlayer));

                    var opacity = canSee ? 0.1f : 0.0f;

                    if (isStealthed(ninja))
                    {
                        opacity = Math.Max(opacity, 1.0f - stealthFade(ninja));
                        ninja.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = Math.Max(opacity, stealthFade(ninja));
                    }

                    setOpacity(ninja, opacity);
                }
            }
        }
    }
}