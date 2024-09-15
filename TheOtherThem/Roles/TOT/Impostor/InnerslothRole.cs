using Hazel;
using System.Collections;
using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem.TOTRole.Impostor;

[RoleAutoInitialize]
[HarmonyPatch]
public class InnerslothRole : CustomRole
{
    public static InnerslothRole Instance { get; private set; }

    private static bool CustomSabotageStarted = false;
    public static CustomRoleOption InnerslothSpawnRate { get; set; }
    public static CustomOption InnerslothAbilltyCooldown { get; set; }

    InnerslothRole() : base("Innersloth", Palette.ImpostorRed, 
        (nameKey, roleColor) => InnerslothSpawnRate = new(2000, nameKey, roleColor, ref CustomOptionHolder.OptionInsertionIndexes.impostor, 1), RoleType.Innersloth, TeamTypeTOT.Impostor)
    {
        Instance = this;

        InnerslothAbilltyCooldown = CustomOption.Create(2001, "InnerslothAbilityCd", 20, 10, 60, 5, ref CustomOptionHolder.OptionInsertionIndexes.impostor, InnerslothSpawnRate);
    }

    public override void ClearData()
    {
        CustomSabotageStarted = false;
    }

    public override void CreateButtons()
    {
        _ = new CustomButton(() =>
        {
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.InnerslothSabotage, Hazel.SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomSabotageComms();
        },
        () => /*PlayerControl.LocalPlayer.IsRole(MyRoleType) && */PlayerControl.LocalPlayer.IsAlive(),
        () => !CustomSabotageStarted,
        () => { },
        null,
        new(3, 0, 0),
        HudManager.Instance,
        HudManager.Instance.UseButton,
        KeyCode.X,
        buttonText: ModTranslation.GetString("InnerslothAbilityLabel"));
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

        while (IsCommsSabotaged())
        {
            yield return new WaitForSeconds(Random.Range(1f, 5f));
            if (!IsCommsSabotaged()) break;
            PlayerControl.LocalPlayer.moveable = false;
            PlayerControl.LocalPlayer.NetTransform.Halt();
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            if (!IsCommsSabotaged()) break;
            PlayerControl.LocalPlayer.moveable = true;
        }
        PlayerControl.LocalPlayer.moveable = true;

        CustomSabotageStarted = false;
    }
}