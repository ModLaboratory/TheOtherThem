
using System.Linq;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using UnityEngine;
using TheOtherThem.Objects;
using TheOtherThem.Patches;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class SerialKiller : RoleBase<SerialKiller> {

        private static CustomButton serialKillerButton;

        public static Color color = Palette.ImpostorRed;

        public static float killCooldown { get { return CustomOptionHolder.serialKillerKillCooldown.GetFloat(); } }
        public static float suicideTimer { get { return Mathf.Max(CustomOptionHolder.serialKillerSuicideTimer.GetFloat(), killCooldown + 2.5f); } }
        public static bool resetTimer { get { return CustomOptionHolder.serialKillerResetTimer.GetBool(); } }

        public bool isCountDown = false;

        public SerialKiller()
        {
            RoleType = RoleId = RoleType.SerialKiller;
            isCountDown = false;
        }

        public override void OnMeetingStart() { }

        public override void OnMeetingEnd()
        {
            if (PlayerControl.LocalPlayer == Player)
            {
                Player.SetKillTimerUnchecked(killCooldown);

                if (resetTimer)
                    serialKillerButton.Timer = suicideTimer;
            }
        }

        public override void FixedUpdate() { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override void OnKill(PlayerControl target)
        {
            if (PlayerControl.LocalPlayer == Player)
                Player.SetKillTimerUnchecked(killCooldown);

            serialKillerButton.Timer = suicideTimer;
            isCountDown = true;
        }

        public override void OnDeath(PlayerControl killer) { }

        private static Sprite buttonSprite;
        public static Sprite getButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.SuicideButton.png", 115f);
            return buttonSprite;
        }

        public static void MakeButtons(HudManager hm)
        {
            // SerialKiller Suicide Countdown
            serialKillerButton = new CustomButton(
                () => { },
                () => { return PlayerControl.LocalPlayer.IsRole(RoleType.SerialKiller) && PlayerControl.LocalPlayer.IsAlive() && Local.isCountDown; },
                () => { return true; },
                () => { },
                SerialKiller.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                true,
                suicideTimer,
                () => { Local.suicide(); }
            );
            serialKillerButton.buttonText = ModTranslation.GetString("SerialKillerText");
            serialKillerButton.isEffectActive = true;
        }

        public void suicide() {
            byte targetId = PlayerControl.LocalPlayer.PlayerId;
            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SerialKillerSuicide, Hazel.SendOption.Reliable, -1); killWriter.Write(targetId);
            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RpcProcedure.serialKillerSuicide(targetId);
        }

        public static void SetButtonCooldowns()
        {
            serialKillerButton.MaxTimer = SerialKiller.suicideTimer;
        }

        public static void Clear()
        {
            Players = new List<SerialKiller>();
        }
    }
}