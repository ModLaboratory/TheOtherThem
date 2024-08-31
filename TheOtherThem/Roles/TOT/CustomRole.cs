using Hazel;
using System;
using System.Collections.Generic;
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
    public CustomRole(string translationName, Color roleColor, CustomOption baseOption, RoleType roleType, TeamTypeTOT teamType)
    {
        MyRoleInfo = new(translationName, roleColor, baseOption, roleType);
        MyRoleType = roleType;
        MyTeamType = teamType;
        AllRoles.Add(this);
    }

    public virtual void CreateButtons() { }
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