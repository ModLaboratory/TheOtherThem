using System;
using System.Collections;
using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace TheOtherThem.Roles.ToT.Impostor
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class InnerslothRole : CustomRole
    {
        public static InnerslothRole Instance { get; private set; }

        private static bool _customSabotageStarted = false;

        public static CustomRoleOption InnerslothSpawnRate { get; set; }
        public static CustomOption InnerslothAbilltyCooldown { get; set; }
        public static CustomButton LagButton { get; set; }

        InnerslothRole() : base("Innersloth", Palette.ImpostorRed,
            (nameKey, roleColor) => InnerslothSpawnRate = new(2000, nameKey, roleColor, TeamTypeToT.Impostor, 1),
            RoleType.Innersloth, TeamTypeToT.Impostor)
        {
            Instance = this;

            InnerslothAbilltyCooldown = CustomOption.CreateInsertable(2001, "InnerslothAbilityCd", 20, 10, 60, 5, TeamTypeToT.Impostor, InnerslothSpawnRate);
        }

        public override void ClearData()
        {
            _customSabotageStarted = false;
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            LagButton = new CustomButton(() =>
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.InnerslothSabotage, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                CustomSabotageComms();
            },
            CanLocalPlayerUse,
            () => !_customSabotageStarted,
            () => { },
            ModTranslation.GetImage("LagButton", 1000),
            new(-2, 1, 0),
            HudManager.Instance,
            HudManager.Instance.UseButton,
            KeyCode.X,
            buttonText: ModTranslation.GetString("InnerslothAbilityLabel"));

            return new[]
            {
                (LagButton, InnerslothAbilltyCooldown.GetFloat())
            };
        }

        public override void OnRpcReceive(byte callId, MessageReader reader)
        {
            if (callId == (byte)CustomRpc.InnerslothSabotage)
                CustomSabotageComms();
        }

        static bool IsCommsSabotaged()
        {
            return PlayerControl.LocalPlayer.myTasks.ToArray().Any(t => t.TaskType == TaskTypes.FixComms);
        }

        public static void CustomSabotageComms()
        {
            var sys = ShipStatus.Instance.Systems[SystemTypes.Comms];
            var mapId = TutorialManager.InstanceExists ? AmongUsClient.Instance.TutorialMapId : GameOptionsManager.Instance.currentGameOptions.MapId;

            if ((MapNames)mapId == MapNames.MiraHQ)
                sys.Cast<HqHudSystemType>().CompletedConsoles.Clear();
            else
                sys.Cast<HudOverrideSystemType>().IsActive = true;

            CoroutineUtils.StartCoroutine(CoCustomSabotage());
        }

        static IEnumerator CoCustomSabotage()
        {
            _customSabotageStarted = true;
            yield return null;

            while (IsCommsSabotaged())
            {
                yield return new WaitForSeconds(UnityRandom.Range(1f, 3f));
                if (!IsCommsSabotaged()) break;
                PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.Halt();
                yield return new WaitForSeconds(UnityRandom.Range(0.5f, 1f));
                if (!IsCommsSabotaged()) break;
                PlayerControl.LocalPlayer.moveable = true;
            }

            PlayerControl.LocalPlayer.moveable = true;
            LagButton.ResetTimer();
            _customSabotageStarted = false;
        }

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(TaskTypes) })]
        [HarmonyPrefix]
        static bool OverrideTaskPanelInfoPatch(TaskTypes task, ref string __result)
        {
            if (task == TaskTypes.FixComms && _customSabotageStarted)
            {
                __result = ModTranslation.GetString("InnerslothFixCommsTaskInfoOverride");
                return false;
            }
            return true;
        }
    }
}