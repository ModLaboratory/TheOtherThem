global using PlayerStatistics = TheOtherThem.Patches.OnGameEndPatch.EndGameNavigationPatch.PlayerStatistics;
using CheckEndCriteriaPatch = TheOtherThem.Patches.OnGameEndPatch.EndGameNavigationPatch.CheckEndCriteriaPatch;
using Hazel;
using System.Collections.Generic;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem.ToTRole;
public abstract class CustomRole
{
    public delegate CustomRoleOption BaseRoleOptionGetter(string name, Color color);

    public string Name => MyRoleInfo.Name;
    public List<NetworkedPlayerInfo> Players { get; } = new();
    public RoleInfo MyRoleInfo { get; }
    public RoleType MyRoleType { get; }
    public TeamTypeTOT MyTeamType { get; }

    /// <summary>
    /// Construct a role.
    /// </summary>
    /// <param name="translationName"></param>
    /// <param name="roleColor"></param>
    /// <param name="baseOptionGetter">The constructor of the role's base option.</param>
    /// <param name="roleType"></param>
    /// <param name="teamType"></param>
    /// <param name="winnable">Whether this role is winnable.</param>
    /// <param name="generalOrStatisticalWinnable"><see cref="true"/> for adding the <see cref="CanWin(ShipStatus)"/> to general handler list; Otherwise, add <see cref="CanWin(ShipStatus, PlayerStatistics)"/> statistical list.</param>
    /// <param name="winnableInsertionIndex">The index of the order checking end. <seealso cref="-1"/> for adding directly.</param>
    public CustomRole(string translationName, Color roleColor, BaseRoleOptionGetter baseOptionGetter, RoleType roleType, TeamTypeTOT teamType, bool winnable = false, bool generalOrStatisticalWinnable = true, int winnableInsertionIndex = -1)
    {
        MyRoleInfo = new(translationName, roleColor, baseOptionGetter(translationName, roleColor), roleType);
        MyRoleType = roleType;
        MyTeamType = teamType;
        AllRoles.Add(this);

        if (winnable)
        {
            if (generalOrStatisticalWinnable)
            {
                var general = CheckEndCriteriaPatch.GeneralEndCheckingHandler;
                if (winnableInsertionIndex == -1) 
                    winnableInsertionIndex = general.Count;
                general.Insert(winnableInsertionIndex, CanWin);
            }
            else
            {
                var statistical = CheckEndCriteriaPatch.StatisticalEndCheckingHandler;
                if (winnableInsertionIndex == -1)
                    winnableInsertionIndex = statistical.Count;
                statistical.Insert(winnableInsertionIndex, CanWin);
            }
        }
        
        Main.Logger.LogInfo($"{translationName} ({nameof(winnable)} = {winnable}, {nameof(generalOrStatisticalWinnable)} = {generalOrStatisticalWinnable}, {nameof(winnableInsertionIndex)} = {winnableInsertionIndex}) registered");
    }

    public bool CanLocalPlayerUse() => PlayerControl.LocalPlayer.IsRole(MyRoleType) && PlayerControl.LocalPlayer.IsAlive();
    public void RpcCustomEndGame(CustomGameOverReason reason) => GameManager.Instance.RpcEndGame((GameOverReason)reason, false);

    public virtual (CustomButton, float)[] CreateButtons() => new[] { ((CustomButton)null, float.PositiveInfinity) };
    public virtual void OnRpcReceived(byte callId, MessageReader reader) { }
    public virtual bool CanWin(ShipStatus ship) => false;
    public virtual bool CanWin(ShipStatus ship, PlayerStatistics statistics) => false;

    public abstract void ClearData();

    public static List<CustomRole> AllRoles => allRoles ??= new();
    private static List<CustomRole> allRoles;
    public static void ReloadAll()
    {
        AllRoles.ForEach(cr => cr.ClearData());
    }
}

public enum TeamTypeTOT
{
    Crewmate,
    Neutral,
    Impostor
}