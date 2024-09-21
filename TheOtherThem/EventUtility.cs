﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TheOtherThem;
using TheOtherThem.Patches;
using static TheOtherThem.TheOtherRoles;
using System.Linq;
using InnerNet;
using TheOtherThem.Modules;
using HarmonyLib;

namespace TheOtherThem;

[HarmonyPatch]
public static class EventUtility {

    public static void Load() {
        if (!isEnabled) return;
    }

    public static void clearAndReload() {
    }

    public static void Update() {
        if (!isEnabled || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || TheOtherRoles.rnd == null || IntroCutscene.Instance) return;
    }

    public static DateTime enabled = DateTime.FromBinary(638475264000000000);
    public static bool isEventDate => DateTime.Today.Date == enabled;

    public static bool canBeEnabled => DateTime.Today.Date >= enabled && DateTime.Today.Date <= enabled.AddDays(7); // One Week after the EVENT
    public static bool isEnabled => isEventDate || canBeEnabled;

    public static void meetingEndsUpdate() {
        if (!isEnabled) return;
        // TODO - Implement Horse hats
        // PlayerControl.LocalPlayer.RpcSetHat(CustomHatLoader.horseHatProductIds[rnd.Next(CustomHatLoader.horseHatProductIds.Count)]);
    }


    public static void meetingStartsUpdate() {
        if (!isEnabled) return;
    }

    public static void gameStartsUpdate() {
        if (!isEnabled) return;
    }

    public static void gameEndsUpdate() {
        if (!isEnabled) return;
    }


    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    public static class AddChatPatch {
        public static void Prefix(ChatController __instance, PlayerControl sourcePlayer, ref string chatText, bool censor) {
            if (!isEnabled) return;
            var charArray = chatText.ToCharArray();
            Array.Reverse(charArray);
            chatText = new string(charArray);
        }
    }
}