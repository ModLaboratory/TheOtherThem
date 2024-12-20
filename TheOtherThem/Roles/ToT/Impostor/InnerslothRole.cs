using Hazel;
using System;
using System.Collections;
using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace TheOtherThem.ToTRole.Impostor
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class InnerslothRole : CustomRole
    {
        public static InnerslothRole Instance { get; private set; }

        private static bool CustomSabotageStarted = false;
        public static CustomRoleOption InnerslothSpawnRate { get; set; }
        public static CustomOption InnerslothAbilltyCooldown { get; set; }
        public static CustomButton LagButton { get; set; }

        InnerslothRole() : base("Innersloth", Palette.ImpostorRed,
            (nameKey, roleColor) => InnerslothSpawnRate = new(2000, nameKey, roleColor, ref CustomOptionHolder.OptionInsertionIndexes.Impostor, 1), 
            RoleType.Innersloth, TeamTypeTOT.Impostor)
        {
            Instance = this;

            InnerslothAbilltyCooldown = CustomOption.CreateInsertable(2001, "InnerslothAbilityCd", 20, 10, 60, 5, ref CustomOptionHolder.OptionInsertionIndexes.Impostor, InnerslothSpawnRate);
        }

        public override void ClearData()
        {
            CustomSabotageStarted = false;
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            LagButton = new CustomButton(() =>
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.InnerslothSabotage, Hazel.SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                CustomSabotageComms();
            },
            CanLocalPlayerUse,
            () => !CustomSabotageStarted,
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

        public override void OnRpcReceived(byte callId, MessageReader reader)
        {
            if (callId == ((byte)CustomRpc.InnerslothSabotage))
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

            if ((MapNames)mapId == MapNames.Mira)
                sys.Cast<HqHudSystemType>().CompletedConsoles.Clear();
            else
                sys.Cast<HudOverrideSystemType>().IsActive = true;

            CoroutineUtils.StartCoroutine(CoCustomSabotage());
        }

        static IEnumerator CoCustomSabotage()
        {
            CustomSabotageStarted = true;
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
            CustomSabotageStarted = false;
        }

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(TaskTypes) })]
        [HarmonyPrefix]
        static bool OverrideTaskPanelInfoPatch(TaskTypes task, ref string __result)
        {
            if (task != TaskTypes.FixComms) return true;
            __result = ModTranslation.GetString("InnerslothFixCommsTaskInfoOverride");
            return false;
        }
    }
}