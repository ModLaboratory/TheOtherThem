global using PlayerStatistics = TheOtherThem.Patches.OnGameEndPatch.EndGameNavigationPatch.PlayerStatistics;
using CheckEndCriteriaPatch = TheOtherThem.Patches.OnGameEndPatch.EndGameNavigationPatch.CheckEndCriteriaPatch;
using Hazel;
using System.Collections.Generic;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using UnityEngine;
using System;
using System.Linq;
using AmongUs.GameOptions;
using static Il2CppSystem.Globalization.CultureInfo;
using static UnityEngine.GraphicsBuffer;

namespace TheOtherThem.ToTRole;
public abstract class CustomRole
{
    public delegate CustomRoleOption BaseRoleOptionGetter(string name, Color color);

    public string Name => MyRoleInfo.Name;
    public List<NetworkedPlayerInfo> Players { get; } = new();
    public RoleInfo MyRoleInfo { get; }
    public RoleType MyRoleType { get; }
    public TeamTypeToT MyTeamType { get; }
    public Color RoleColor { get; }
    public bool IsKillableNonImpostor { get; init; } = false;

    /// <summary>
    /// Constructs a role.
    /// </summary>
    /// <param name="translationName"></param>
    /// <param name="roleColor"></param>
    /// <param name="baseOptionGetter">The constructor of the role's base option.</param>
    /// <param name="roleType"></param>
    /// <param name="teamType"></param>
    /// <param name="winnable">Whether this role is winnable.</param>
    /// <param name="needsStatisticalWinningInfo"><see cref="true"/> for adding the <see cref="CanWin(ShipStatus)"/> to general handler list; Otherwise, add <see cref="CanWin(ShipStatus, PlayerStatistics)"/> statistical list.</param>
    /// <param name="winnableInsertionIndex">The index of the order checking end. <seealso cref="-1"/> for adding directly.</param>
    public CustomRole(string translationName, Color roleColor, BaseRoleOptionGetter baseOptionGetter, RoleType roleType, TeamTypeToT teamType, bool winnable = false, bool needsStatisticalWinningInfo = false, int winnableInsertionIndex = -1)
    {
        MyRoleInfo = new(translationName, RoleColor = roleColor, baseOptionGetter(translationName, roleColor), roleType);
        MyRoleType = roleType;
        MyTeamType = teamType;
        AllRoles.Add(this);

        if (winnable)
        {
            if (needsStatisticalWinningInfo)
            {
                var statistical = CheckEndCriteriaPatch.StatisticalEndCheckingHandler;
                if (winnableInsertionIndex == -1)
                    winnableInsertionIndex = statistical.Count;
                statistical.Insert(winnableInsertionIndex, CanWin);
            }
            else
            {
                var general = CheckEndCriteriaPatch.GeneralEndCheckingHandler;
                if (winnableInsertionIndex == -1)
                    winnableInsertionIndex = general.Count;
                general.Insert(winnableInsertionIndex, CanWin);
            }
        }
        
        Main.Logger.LogInfo($"{translationName} ({nameof(winnable)} = {winnable}, {nameof(needsStatisticalWinningInfo)} = {needsStatisticalWinningInfo}, {nameof(winnableInsertionIndex)} = {winnableInsertionIndex}) registered");
    }

    public bool CanLocalPlayerUse() => PlayerControl.LocalPlayer.IsRole(MyRoleType) && PlayerControl.LocalPlayer.IsAlive();
    public void RpcCustomEndGame(CustomGameOverReason reason) => GameManager.Instance.RpcEndGame((GameOverReason)reason, false);

    public virtual (CustomButton, float)[] CreateButtons() => new[] { ((CustomButton)null, float.PositiveInfinity) };
    public virtual void OnRpcReceived(byte callId, MessageReader reader) { }
    public virtual void OnRoleDataBeingSynchronized(MessageReader reader) { }
    public virtual void OnRoleDataSynchronizing(MessageWriter writer) { }
    public virtual void OnLocalPlayerBecomingThisRole() { }
    public virtual bool ShouldShowKillButton() => true;
    public virtual bool CanWin(ShipStatus ship) => false;
    public virtual bool CanWin(ShipStatus ship, PlayerStatistics statistics) => false;
    public virtual string GetRoleTaskHintText() => MyRoleInfo.ShortDescription;

    public abstract void ClearData();

    public bool IsLocalPlayerRole() => PlayerControl.LocalPlayer.IsRole(MyRoleType);
    public void SyncRoleData()
    {
        var writer = new RpcWriter(CustomRpc.RoleDataSync);
        writer.WritePacked((int)MyRoleType);
        OnRoleDataSynchronizing(writer);
        writer.Finish();
    }

    public static List<CustomRole> AllRoles => _allRoles ??= new();
    private static List<CustomRole> _allRoles;

    public static void ReloadAll()
    {
        AllRoles.ForEach(cr => cr.ClearData());
    }
}

[HarmonyPatch(typeof(PlayerControl))]
static class PlayerKillButtonPatch
{
    [HarmonyPatch(nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    static void ButtonUpdatePatch(PlayerControl __instance)
    {
        if (!__instance.AmOwner) return;
        if (!CustomRole.AllRoles.Where(r => r.IsKillableNonImpostor).Any(r => r.Players.Contains(__instance.Data))
            && !__instance.IsImpostor())
        {
            HudManager.Instance.KillButton.Hide();
            return;
        }
        if (__instance.IsImpostor()) return;

        var role = CustomRole.AllRoles.First(r => r.Players.Contains(__instance.Data));
        var data = __instance.Data;
        data.Role.CanUseKillButton = true;
        PlayerControl playerControl = FindClosestTarget();

        if ((__instance.IsKillTimerEnabled || __instance.ForceKillTimerContinue) && !data.IsDead)
        {
            HudManager.Instance.KillButton.Show();
            __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
            DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(playerControl);
            __instance.cosmetics.SetOutline(true, new(Palette.ImpostorRed));
        }
        else
        {
            DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
            if (data.IsDead)
                HudManager.Instance.KillButton.Hide();
            __instance.cosmetics.SetOutline(false, new(Palette.ImpostorRed));
            __instance.Data.Role.SetPlayerTarget(playerControl);
        }

        HudManager.Instance.KillButton.ToggleVisible(role.ShouldShowKillButton());
    }

    private static PlayerControl FindClosestTarget()
    {
        List<PlayerControl> playersInAbilityRangeSorted = GetPlayersInAbilityRangeSorted(false);
        if (playersInAbilityRangeSorted.Count <= 0)
            return null;
        return playersInAbilityRangeSorted[0];
    }

    private static List<PlayerControl> GetPlayersInAbilityRangeSorted(bool ignoreColliders)
    {
        var outputList = new List<PlayerControl>();

        float abilityDistance = GameManager.Instance.LogicOptions.GetKillDistance();
        Vector2 myPos = PlayerControl.LocalPlayer.GetTruePosition();
        var allPlayers = GameData.Instance.AllPlayers;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo networkedPlayerInfo = allPlayers[i];
            if (IsValidTarget(networkedPlayerInfo))
            {
                PlayerControl @object = networkedPlayerInfo.Object;
                if (@object && @object.Collider.enabled)
                {
                    Vector2 vector = @object.GetTruePosition() - myPos;
                    float magnitude = vector.magnitude;
                    if (magnitude <= abilityDistance && (ignoreColliders || !PhysicsHelpers.AnyNonTriggersBetween(myPos, vector.normalized, magnitude, Constants.ShipAndObjectsMask)))
                    {
                        outputList.Add(@object);
                    }
                }
            }
        }

        outputList.Sort((a, b) => 
        {
            float magnitude2 = (a.GetTruePosition() - myPos).magnitude;
            float magnitude3 = (b.GetTruePosition() - myPos).magnitude;
            if (magnitude2 > magnitude3)
            {
                return 1;
            }
            if (magnitude2 < magnitude3)
            {
                return -1;
            }
            return 0;
        });

        return outputList;
    }

    private static bool IsValidTarget(NetworkedPlayerInfo target)
    {
        return !(target == null) && !target.Disconnected && !target.IsDead && target.PlayerId != PlayerControl.LocalPlayer.PlayerId && !(target.Role == null) && !(target.Object == null) && !target.Object.inVent && !target.Object.inMovingPlat && target.Object.Visible && target.Role.CanBeKilled;
    }
}



public enum TeamTypeToT
{
    Crewmate,
    Neutral,
    Impostor
}