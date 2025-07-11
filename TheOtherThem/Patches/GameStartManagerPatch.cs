using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Reflection;
using TheOtherThem.Objects;
using TMPro;
using UnityEngine;

namespace TheOtherThem.Patches
{
    public class GameStartManagerPatch
    {
        public static Dictionary<int, PlayerVersion> PlayerVersions { get; } = new();
        //private static float _timer = 600f;
        private static float _kickingTimer = 0f;
        private static bool _versionSent = false;
        //private static string lobbyCodeText = "";

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer)
                    Helpers.ShareGameVersion();
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix(GameStartManager __instance)
            {
                // Trigger version refresh
                _versionSent = false;
                // Reset lobby countdown timer
                //_timer = 600f;
                // Reset kicking timer
                _kickingTimer = 0f;
                // Copy lobby code
                //string code = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                //GUIUtility.systemCopyBuffer = code;
                //lobbyCodeText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, new Il2CppReferenceArray<Il2CppSystem.Object>(0)) + "\r\n" + code;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            //private static bool _update = false;
            static int _textId;

            public static void Postfix(GameStartManager __instance)
            {
                // Send version as soon as PlayerControl.LocalPlayer exists
                if (PlayerControl.LocalPlayer != null && !_versionSent)
                {
                    _versionSent = true;
                    Helpers.ShareGameVersion();
                }

                // Host update with version handshake infos
                if (AmongUsClient.Instance.AmHost)
                {
                    bool versionMismatch = false;
                    string message = "";
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                    {
                        if (client.Character == null) continue;
                        if (client.Character.PlayerId == 0) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        else if (!PlayerVersions.ContainsKey(client.Id))
                        {
                            versionMismatch = true;
                            message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {ModTranslation.GetString("errorNotInstalled")}\n</color>";
                        }
                        else
                        {
                            PlayerVersion version = PlayerVersions[client.Id];
                            int diff = Main.Version.CompareTo(version.Version);
                            if (diff > 0)
                            {
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {ModTranslation.GetString("errorOlderVersion")} (v{PlayerVersions[client.Id].Version})\n</color>";
                                versionMismatch = true;
                            }
                            else if (diff < 0)
                            {
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {ModTranslation.GetString("errorNewerVersion")} (v{PlayerVersions[client.Id].Version})\n</color>";
                                versionMismatch = true;
                            }
                            else if (!version.GuidMatches())
                            { // version presumably matches, check if Guid matches
                                message += $"<color=#FF0000FF>{client.Character.Data.PlayerName}:  {ModTranslation.GetString("errorWrongVersion")} v{PlayerVersions[client.Id].Version} <size=30%>({version.Guid.ToString()})</size>\n</color>";
                                versionMismatch = true;
                            }
                        }
                    }
                    if (versionMismatch)
                    {
                        SetStartButtonEnabled(false);
                        if (ErrorNotification.HasError(_textId))
                            ErrorNotification.SetErrorString(_textId, message, float.PositiveInfinity);
                        else
                            _textId = ErrorNotification.AddErrorString(message, float.PositiveInfinity);
                    }
                    else
                    {
                        SetStartButtonEnabled(true);
                        ErrorNotification.RemoveErrorString(_textId);
                    }
                }

                void SetStartButtonEnabled(bool enabled)
                {
                    __instance.MinPlayers = enabled ? 0 : int.MaxValue;
                    __instance.StartButton.SetButtonEnableState(enabled);
                    __instance.StartButton.buttonText.text = TranslationController.Instance.GetString(enabled ? StringNames.StartLabel : StringNames.WaitingForPlayers);
                }

                // Client update with handshake infos
                if (!AmongUsClient.Instance.AmHost)
                {
                    if (!PlayerVersions.ContainsKey(AmongUsClient.Instance.HostId) || Main.Version.CompareTo(PlayerVersions[AmongUsClient.Instance.HostId].Version) != 0)
                    {
                        _kickingTimer += Time.deltaTime;
                        if (_kickingTimer > 10)
                        {
                            _kickingTimer = 0;
                            AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                            SceneChanger.ChangeScene(Constants.MAIN_MENU_SCENE);
                        }

                        ErrorNotification.AddErrorString(string.Format(ModTranslation.GetString("errorHostNoVersion"), Math.Round(10 - _kickingTimer)), float.PositiveInfinity);
                    }
                    else
                    {
                        if (__instance.startState != GameStartManager.StartingStates.Countdown)
                            ErrorNotification.RemoveErrorString(_textId);
                    }
                }

                // Lobby code replacement
                //if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) __instance.GameRoomNameCode.text = Main.StreamerMode.Value ? $"<color={Main.StreamerModeReplacementColor.Value}>{Main.StreamerModeReplacementText.Value}</color>" : lobbyCodeText;

                // Lobby timer
                //if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.) return; // Not host or no instance

                //if (update) currentText = __instance.PlayerCounter.text;

                //_timer = Mathf.Max(0f, _timer -= Time.deltaTime);
                //int minutes = (int)_timer / 60;
                //int seconds = (int)_timer % 60;
                //string suffix = $" ({minutes:00}:{seconds:00})";

                //__instance.PlayerCounter.text = currentText + suffix;
                //__instance.PlayerCounter.autoSizeTextContainer = true;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
        public class GameStartManagerBeginGame
        {
            public static bool Prefix(GameStartManager __instance)
            {
                // Block game start if not everyone has the same mod version
                bool continueStart = true;

                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients)
                    {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;

                        if (!PlayerVersions.ContainsKey(client.Id))
                        {
                            continueStart = false;
                            break;
                        }

                        PlayerVersion version = PlayerVersions[client.Id];
                        int diff = Main.Version.CompareTo(version.Version);
                        if (diff != 0 || !version.GuidMatches())
                        {
                            continueStart = false;
                            break;
                        }
                    }

                    if (CustomOptionHolder.UselessOptions.GetBool() && CustomOptionHolder.DynamicMap.GetBool() && continueStart)
                    {
                        // 0 = Skeld
                        // 1 = Mira HQ
                        // 2 = Polus
                        // 3 = Dleks - deactivated
                        // 4 = Airship
                        List<byte> possibleMaps = new List<byte>();
                        if (CustomOptionHolder.dynamicMapEnableSkeld.GetBool())
                            possibleMaps.Add(0);
                        if (CustomOptionHolder.dynamicMapEnableMira.GetBool())
                            possibleMaps.Add(1);
                        if (CustomOptionHolder.dynamicMapEnablePolus.GetBool())
                            possibleMaps.Add(2);
                        if (CustomOptionHolder.dynamicMapEnableDleks.GetBool())
                            possibleMaps.Add(3);
                        if (CustomOptionHolder.dynamicMapEnableAirShip.GetBool())
                            possibleMaps.Add(4);
                        byte chosenMapId = possibleMaps[TheOtherRoles.rnd.Next(possibleMaps.Count)];

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.DynamicMapOption, Hazel.SendOption.Reliable, -1);
                        writer.Write(chosenMapId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.DynamicMapOption(chosenMapId);
                    }
                }
                return continueStart;
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.SetStartCounter))]
        public static class SetStartCounterPatch
        {
            public static void Postfix(GameStartManager __instance, sbyte sec)
            {
                if (sec > 0)
                {
                    __instance.startState = GameStartManager.StartingStates.Countdown;
                }

                if (sec <= 0)
                {
                    __instance.startState = GameStartManager.StartingStates.NotStarting;
                }
            }
        }

        public class PlayerVersion
        {
            public Version Version { get; }
            public Guid Guid { get; }

            public PlayerVersion(Version version, Guid guid)
            {
                Version = version;
                Guid = guid;
            }

            public bool GuidMatches()
            {
                return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(Guid);
            }
        }
    }
}
