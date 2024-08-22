using AmongUs.GameOptions;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class CredentialsPatch {

        public static string baseCredentials = $@"<size=130%><color=#ff351f>TheOtherRoles GM</color></size> v{TheOtherRolesPlugin.Version}";

        public static string contributorsCredentials = "<size=80%>GitHub Contributors: Alex2911, amsyarasyiq, gendelo3</size>";


        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance){
                __instance.text.alignment = TextAlignmentOptions.Top;
                var position = __instance.GetComponent<AspectPosition>();
                position.Alignment = AspectPosition.EdgeAlignments.Top;
                __instance.text.text = baseCredentials + $"\nPING: <b>{AmongUsClient.Instance.Ping}</b> ms";
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    position.DistanceFromEdge = new Vector3(2.25f, 0.11f, 0);
                }
                else
                {
                    position.DistanceFromEdge = new Vector3(0f, 0.1f, 0);
                }
                position.AdjustPosition();
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            static void Postfix(MainMenuManager __instance) {
                DestroyableSingleton<ModManager>.Instance.ShowModStamp();

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
                torLogo.transform.localPosition = new Vector3(-0.4f, 1f, 5f);
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);
            }
        }
    }
}
