using Hazel;
using System;
using System.Collections.Generic;
using TheOtherThem.Objects;
using TheOtherThem.TOTRole.Impostor;
using UnityEngine;

namespace TheOtherThem.TOTRole;
public abstract class CustomRole
{
    public string Name => MyRoleInfo.Name;
    public List<NetworkedPlayerInfo> Players { get; } = new();
    public RoleInfo MyRoleInfo { get; }
    public RoleType MyRoleType { get; }
    public TeamTypeTOT MyTeamType { get; }
    public CustomRole(string translationName, Color roleColor, Func<string, Color, CustomRoleOption> onGetBaseOption, RoleType roleType, TeamTypeTOT teamType)
    {
        MyRoleInfo = new(translationName, roleColor, onGetBaseOption(translationName, roleColor), roleType);
        MyRoleType = roleType;
        MyTeamType = teamType;
        AllRoles.Add(this);

        Main.Logger.LogInfo($"{translationName} registered");
    }

    public virtual (CustomButton, float)[] CreateButtons() => new[] { ((CustomButton)null, float.PositiveInfinity) };
    public virtual void OnRpcReceived(byte callId, MessageReader reader) { }

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