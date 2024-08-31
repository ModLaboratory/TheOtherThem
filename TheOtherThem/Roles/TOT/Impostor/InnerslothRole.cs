using Hazel;
using System.Collections;
using TheOtherThem.Modules;
using UnityEngine;

namespace TheOtherThem.TOTRole.Impostor;

[RoleAutoInitialize]
[HarmonyPatch]
public class InnerslothRole : CustomRole
{
    public static InnerslothRole Instance { get; private set; }

    private bool CustomSabotageStarted = false;

    InnerslothRole() : base("Innersloth", Palette.ImpostorRed, CustomOptionHolder.InnerslothSpawnRate, RoleType.Innersloth, TeamTypeTOT.Impostor)
    {
        Instance = this;
    }

    public override void ClearData()
    {
        CustomSabotageStarted = false;
    }

    public override void OnRpcReceived(byte callId, MessageReader reader)
    {
        if (callId == ((byte)CustomRpc.InnerslothSabotage))
            SabotageComms();
    }

    static bool IsCommsSabotaged()
    {
        foreach (var task in PlayerControl.LocalPlayer.myTasks)
        {
            if (task.GetComponent<HudOverrideTask>())
                return !task.GetComponent<HudOverrideTask>().IsComplete;
            if (task.GetComponent<HqHudOverrideTask>())
                return !task.GetComponent<HqHudOverrideTask>().IsComplete;
        }
        return false;
    }

    static void SabotageComms()
    {
        foreach (var sys in ShipStatus.Instance.Systems)
        {
            if (sys.TryCast<HqHudSystemType>() != null)
            {
                sys.TryCast<HqHudSystemType>().CompletedConsoles.Clear();
                break;
            }

            if (sys.TryCast<HudOverrideSystemType>() != null)
            {
                sys.TryCast<HudOverrideSystemType>().IsActive = true;
                break;
            }
        }
    }

    static IEnumerator CoCustomSabotage()
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.InnerslothSabotage, Hazel.SendOption.Reliable);
        AmongUsClient.Instance.FinishRpcImmediately(writer);

        while (IsCommsSabotaged())
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            if (!IsCommsSabotaged()) break;
            PlayerControl.LocalPlayer.moveable = false;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            if (!IsCommsSabotaged()) break;
            PlayerControl.LocalPlayer.moveable = true;
        }
        PlayerControl.LocalPlayer.moveable = true;
    }

    [HarmonyPatch(typeof(HudOverrideTask), nameof(HudOverrideTask.FixedUpdate))]
    [HarmonyPatch(typeof(HqHudOverrideTask), nameof(HqHudOverrideTask.FixedUpdate))]
    [HarmonyPostfix]
    static void OnCommsSabotaged()
    {
        CoroutineUtils.StartCoroutine(CoCustomSabotage());
    }
}