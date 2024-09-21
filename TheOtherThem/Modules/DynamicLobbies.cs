using System;
using HarmonyLib;
using UnityEngine;
using Hazel;
using InnerNet;
using AmongUs.GameOptions;
using AmongUs.Data.Settings;
using AmongUs.Data;

namespace TheOtherThem.Modules {
    [HarmonyPatch]
    public static class DynamicLobbies {
        public static int LobbyLimit = 15;
        [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
        private static class SendChatPatch {
            static bool Prefix(ChatController __instance) {
                string text = __instance.freeChatField.Text;
                bool handled = false;
                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    if (text.ToLower().StartsWith("/size ")) { // Unfortunately server holds this - need to do more trickery
                            if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.CanBan()) { // checking both just cause
                                handled = true;
                                if (!Int32.TryParse(text.Substring(6), out LobbyLimit)) {
                                    __instance.AddChat(PlayerControl.LocalPlayer, "Invalid Size\nUsage: /size {amount}");
                                } else {
                                    LobbyLimit = Math.Clamp(LobbyLimit, 4, 15);
                                    if (LobbyLimit != GameOptionsManager.Instance.CurrentGameOptions.MaxPlayers) {
                                        GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV08>().MaxPlayers = LobbyLimit;
                                        DestroyableSingleton<GameStartManager>.Instance.LastPlayerCount = LobbyLimit;
                                        GameManager.Instance.LogicOptions.SyncOptions();
                                        __instance.AddChat(PlayerControl.LocalPlayer, $"Lobby Size changed to {LobbyLimit} players");
                                    } else {
                                        __instance.AddChat(PlayerControl.LocalPlayer, $"Lobby Size is already {LobbyLimit}");
                                    }
                                }
                            }
                        }
                }
                if (handled) {
                    __instance.freeChatField.Clear();
                }
                return !handled;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
        public static class InnerNetClientHostPatch {
            public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings) {
                DynamicLobbies.LobbyLimit = settings.MaxPlayers;
                settings.MaxPlayers = 15; // Force 15 Player Lobby on Server
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
            public static void Postfix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] GameOptionsData settings) {
                settings.MaxPlayers = DynamicLobbies.LobbyLimit;
            }
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        public static class InnerNetClientJoinPatch {
            public static void Prefix(InnerNet.InnerNetClient __instance) {
                DataManager.Settings.Multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
            }
        }
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public static class AmongUsClientOnPlayerJoined {
            public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client) {
                if (LobbyLimit < __instance.allClients.Count) { // TODO: Fix this canceling start
                    DisconnectPlayer(__instance, client.Id);
                    return false;
                }
                return true;
            }

            private static void DisconnectPlayer(InnerNetClient _this, int clientId) {
			if (!_this.AmHost) {
				return;
			}
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(4);
			messageWriter.Write(_this.GameId);
			messageWriter.WritePacked(clientId);
			messageWriter.Write((byte)DisconnectReasons.GameFull);
			messageWriter.EndMessage();
			_this.SendOrDisconnect(messageWriter);
			messageWriter.Recycle();
            }
        }
    }
}