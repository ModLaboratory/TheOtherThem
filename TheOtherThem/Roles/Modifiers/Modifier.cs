using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using TheOtherThem.Objects;
using static TheOtherThem.GameHistory;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using TheOtherThem.Patches;
using System.Reflection;

namespace TheOtherThem
{
    public enum ModifierType
    {
        Madmate = 0,

        // don't put anything below this
        NoModifier = int.MaxValue
    }

    [HarmonyPatch]
    public static class ModifierData
    {
        public static Dictionary<ModifierType, Type> allModTypes = new Dictionary<ModifierType, Type>
        {
            { ModifierType.Madmate, typeof(ModifierBase<Madmate>) },
        };
    }

    public abstract class Modifier
    {
        public static List<Modifier> AllModifiers { get; } = new List<Modifier>();
        public PlayerControl Player { get; set; }
        public ModifierType ModId { get; init; }

        public virtual void OnMeetingStart() { }
        public virtual void OnMeetingEnd() { }
        public virtual void FixedUpdate() { }
        public virtual void OnKill(PlayerControl target) { }
        public virtual void OnDeath(PlayerControl killer = null) { }
        public virtual void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
        public virtual void ResetModifier() { }

        public static void ClearAll()
        {
            AllModifiers.Clear();
        }
    }

    [HarmonyPatch]
    public abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
    {
        public static List<T> players = new List<T>();
        public static ModifierType ModType;

        public void Init(PlayerControl player)
        {
            this.Player = player;
            players.Add((T)this);
            AllModifiers.Add(this);
        }

        public static T local
        {
            get
            {
                return players.FirstOrDefault(x => x.Player == PlayerControl.LocalPlayer);
            }
        }

        public static List<PlayerControl> allPlayers
        {
            get
            {
                return players.Select(x => x.Player).ToList();
            }
        }

        public static List<PlayerControl> livingPlayers
        {
            get
            {
                return players.Select(x => x.Player).Where(x => x.IsAlive()).ToList();
            }
        }

        public static List<PlayerControl> deadPlayers
        {
            get
            {
                return players.Select(x => x.Player).Where(x => !x.IsAlive()).ToList();
            }
        }

        public static bool exists
        {
            get { return Helpers.RolesEnabled && players.Count > 0; }
        }

        public static T getModifier(PlayerControl player = null)
        {
            player = player ?? PlayerControl.LocalPlayer;
            return players.FirstOrDefault(x => x.Player == player);
        }

        public static bool hasModifier(PlayerControl player)
        {
            return players.Any(x => x.Player == player);
        }

        public static void addModifier(PlayerControl player)
        {
            T mod = new T();
            mod.Init(player);
        }

        public static void eraseModifier(PlayerControl player)
        {
            players.DoIf(x => x.Player == player, x => x.ResetModifier());
            players.RemoveAll(x => x.Player == player && x.ModId == ModType);
            AllModifiers.RemoveAll(x => x.Player == player && x.ModId == ModType);
        }

        public static void swapModifier(PlayerControl p1, PlayerControl p2)
        {
            var index = players.FindIndex(x => x.Player == p1);
            if (index >= 0)
            {
                players[index].Player = p2;
            }
        }
    }


    public static class ModifierHelpers
    {
        public static bool HasModifier(this PlayerControl player, ModifierType mod)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    return (bool)t.Value.GetMethod("hasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                }
            }
            return false;
        }

        public static void addModifier(this PlayerControl player, ModifierType mod)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    t.Value.GetMethod("addModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                    return;
                }
            }
        }

        public static void eraseModifier(this PlayerControl player, ModifierType mod)
        {
            if (HasModifier(player, mod))
            {
                foreach (var t in ModifierData.allModTypes)
                {
                    if (mod == t.Key)
                    {
                        t.Value.GetMethod("eraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                        return;
                    }
                }
                Main.Logger.LogError($"eraseRole: no method found for role type {mod}");
            }
        }

        public static void eraseAllModifiers(this PlayerControl player)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                t.Value.GetMethod("eraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }
        }

        public static void swapModifiers(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (player.HasModifier(t.Key))
                {
                    t.Value.GetMethod("swapModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
                }
            }
        }
    }
}