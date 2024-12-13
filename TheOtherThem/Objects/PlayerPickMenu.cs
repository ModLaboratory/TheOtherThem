using Il2CppSystem.Collections.Generic;
using System;
using UnityEngine;

// Code base from MalumMenu
namespace TheOtherThem.Objects
{
    public static class PlayerPickMenu
    {
        public static bool IsActive;
        public static Action<NetworkedPlayerInfo> Callback;
        public static List<NetworkedPlayerInfo> customPlayerList;
        public static ShapeshifterMinigame MenuPrefab => RoleManager.Instance.GetRole(AmongUs.GameOptions.RoleTypes.Shapeshifter).Cast<ShapeshifterRole>().ShapeshifterMenu;

        public static void OpenPlayerPickMenu(List<NetworkedPlayerInfo> playerList, Action<NetworkedPlayerInfo> callback)
        {
            IsActive = true;
            customPlayerList = playerList;
            Callback = callback;
            var menu = Object.Instantiate(MenuPrefab);

            menu.transform.SetParent(Camera.main.transform, false);
            menu.transform.localPosition = new Vector3(0f, 0f, -50f);
            menu.Begin(null);
        }

        public static void OpenPlayerPickMenu(Action<NetworkedPlayerInfo> callback) => OpenPlayerPickMenu(GameData.Instance.AllPlayers, callback);
    }

    [HarmonyPatch(typeof(ShapeshifterMinigame))]
    public static class ShapeshifterMinigamePatch
    {
        // Prefix patch of ShapeshifterMinigame.Begin to implement player pick menu logic
        [HarmonyPatch(nameof(ShapeshifterMinigame.Begin))]
        [HarmonyPrefix]
        public static bool MenuBeginPatch(ShapeshifterMinigame __instance)
        {
            if (PlayerPickMenu.IsActive)
            { // Player Pick Menu logic

                PlayerControl.LocalPlayer.moveable = false;

                // Custom player list set by OpenPlayerPickMenu
                List<NetworkedPlayerInfo> list = PlayerPickMenu.customPlayerList;

                __instance.potentialVictims = new();
                List<UiElement> elements = new();

                for (int i = 0; i < list.Count; i++)
                {
                    NetworkedPlayerInfo playerData = list[i];
                    int num = i % 3;
                    int num2 = i / 3;
                    var panel = Object.Instantiate(__instance.PanelPrefab, __instance.transform);
                    panel.transform.localPosition = new(__instance.XStart + num * __instance.XOffset, __instance.YStart + num2 * __instance.YOffset, -1f);

                    panel.SetPlayer(i, playerData, (Il2CppSystem.Action)(() =>
                    {
                        PlayerPickMenu.Callback(playerData); // Custom action set by OpenPlayerPickMenu
                        __instance.Close();
                    }));

                    if (playerData.Object != null)
                        panel.NameText.text = playerData.PlayerName;
                    panel.transform.FindChild("Nameplate").FindChild("Highlight").FindChild("ShapeshifterIcon").gameObject.SetActive(false);
                    
                    __instance.potentialVictims.Add(panel);
                    elements.Add(panel.Button);
                }

                ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.BackButton, __instance.DefaultButtonSelected, elements, false);
                PlayerPickMenu.IsActive = false;

                return false; // Skip original method when active
            }

            return true; // Open normal shapeshifter menu if not active
        }

        [HarmonyPatch(nameof(Minigame.Close))]
        [HarmonyPostfix]
        public static void MenuClosePatch(Minigame __instance)
        {
            if (__instance.GetComponent<ShapeshifterMinigame>())
                PlayerControl.LocalPlayer.moveable = true;
        }
    }
}