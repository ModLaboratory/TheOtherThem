﻿global using HarmonyLib;
global using Object = UnityEngine.Object;
using AmongUs.Data.Legacy;
using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Hazel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using UnityEngine;

namespace TheOtherThem
{
    [BepInPlugin(ModBaseInfo.Id, "TheOtherThem", ModBaseInfo.VersionString)]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        public static Version Version => Version.Parse(ModBaseInfo.VersionString);
        internal static BepInEx.Logging.ManualLogSource Logger { get; private set; }

        public Harmony Harmony { get; } = new Harmony(ModBaseInfo.Id);
        public static Main Instance { get; private set; }

        public static ConfigEntry<bool> DebugMode { get; private set; }
        [Obsolete] public static ConfigEntry<bool> StreamerMode { get; set; }
        public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
        public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
        public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
        public static ConfigEntry<bool> ShowRoleSummary { get; set; }
        public static ConfigEntry<bool> HideNameplates { get; set; }
        public static ConfigEntry<bool> ShowLighterDarker { get; set; }
        public static ConfigEntry<bool> HideTaskArrows { get; set; }
        [Obsolete] public static ConfigEntry<string> StreamerModeReplacementText { get; set; }
        [Obsolete] public static ConfigEntry<string> StreamerModeReplacementColor { get; set; }
        public static ConfigEntry<string> Ip { get; set; }
        public static ConfigEntry<ushort> Port { get; set; }
        public static ConfigEntry<string> DebugRepo { get; private set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; set; }

        public static Sprite ModStamp;

        public static IRegionInfo[] defaultRegions;
        public static void UpdateRegions()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = defaultRegions;

            var CustomRegion = new DnsRegionInfo(Ip.Value, "Custom", StringNames.NoTranslation, Ip.Value, Port.Value, false);
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.Cast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        public override void Load()
        {
            ModTranslation.Load();
            Logger = Log;
            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            //StreamerMode = Config.Bind("Custom", "Enable Streamer Mode", false);
            GhostsSeeTasks = Config.Bind("Custom", "Ghosts See Remaining Tasks", true);
            GhostsSeeRoles = Config.Bind("Custom", "Ghosts See Roles", true);
            GhostsSeeVotes = Config.Bind("Custom", "Ghosts See Votes", true);
            ShowRoleSummary = Config.Bind("Custom", "Show Role Summary", true);
            HideNameplates = Config.Bind("Custom", "Hide Nameplates", false);
            ShowLighterDarker = Config.Bind("Custom", "Show Lighter / Darker", false);
            HideTaskArrows = Config.Bind("Custom", "Hide Task Arrows", false);
            ShowPopUpVersion = Config.Bind("Custom", "Show PopUp", "0");
            //StreamerModeReplacementText = Config.Bind("Custom", "Streamer Mode Replacement Text", "\n\nThe Other Roles GM");
            //StreamerModeReplacementColor = Config.Bind("Custom", "Streamer Mode Replacement Text Hex Color", "#87AAF5FF");
            DebugRepo = Config.Bind("Custom", "Debug Hat Repo", "");

            Ip = Config.Bind("Custom", "Custom Server IP", "127.0.0.1");
            Port = Config.Bind("Custom", "Custom Server Port", (ushort)22023);
            defaultRegions = ServerManager.DefaultRegions;

            UpdateRegions();

            GameOptionsData.RecommendedImpostors = Enumerable.Repeat(3, 16).ToArray();
            GameOptionsData.MaxImpostors = Enumerable.Repeat(15, 16).ToArray(); // Max Imp = Recommended Imp = 3
            GameOptionsData.MinPlayers = Enumerable.Repeat(4, 15).ToArray(); // Min Players = 4

            DebugMode = Config.Bind("Custom", "Enable Debug Mode", false);
            Instance = this;
            CustomOptionHolder.Load();
            RoleHelpers.InitTOTRoles();
            CustomColors.Load();

            Harmony.PatchAll();
            AddComponent<CoroutineUtils.CustomCoroutine>();
            AddComponent<Timer.TimerManager>();
            AddComponent<ErrorNotification.ErrorNotificationManager>();

            System.Console.OutputEncoding = Encoding.UTF8;

            Logger.LogMessage("");
            Logger.LogMessage($"======= TOT LOADED! =======");
            Logger.LogMessage("");

            string currentGameVersion = Application.version;
            Logger.LogInfo($"{nameof(ModBaseInfo)}.{nameof(ModBaseInfo.VersionString)} = {ModBaseInfo.VersionString}");
            Logger.LogInfo($"{nameof(Application)}.{nameof(Application.version)} = {currentGameVersion}");

            if (Application.version != ModBaseInfo.SupportedGameVersion)
                Logger.LogWarning($"Unsupported game version {currentGameVersion} detected ({ModBaseInfo.SupportedGameVersion})");
        }

        public static Sprite GetModStamp()
        {
            if (ModStamp) return ModStamp;
            return ModStamp = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.ModStamp.png", 150f);
        }
    }

    // Deactivate bans, since I always leave my local testing game and ban myself
    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    public static class AmBannedPatch
    {
        public static void Postfix(out bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Awake))]
    public static class ChatControllerAwakePatch
    {
        private static void Prefix()
        {
            if (!EOSManager.Instance.IsMinorOrWaiting())
            {
                LegacySaveManager.chatModeType = 1;
                LegacySaveManager.isGuest = false;
            }
        }
    }

    // Debugging tools
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class DebugManager
    {
        private static readonly System.Random random = new System.Random((int)DateTime.Now.Ticks);
        private static List<PlayerControl> bots = new List<PlayerControl>();

        public static void Postfix(KeyboardJoystick __instance)
        {
            if (!Main.DebugMode.Value) return;

            if (Input.GetKeyDown(KeyCode.F12)) GameStartManager.Instance?.ReallyBegin(false);

            // Spawn dummys
            /*if (Input.GetKeyDown(KeyCode.F)) {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                var i = playerControl.PlayerId = (byte) GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl);
                AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

                int hat = random.Next(HatManager.Instance.allHats.Count);
                int pet = random.Next(HatManager.Instance.allPets.Count);
                int skin = random.Next(HatManager.Instance.allSkins.Count);
                int visor = random.Next(HatManager.Instance.allVisors.Count);
                int color = random.Next(Palette.PlayerColors.Length);
                int nameplate = random.Next(HatManager.Instance.allNamePlates.Count);

                playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(10));
                playerControl.SetColor(color);
                playerControl.SetHat(HatManager.Instance.AllHats[hat].ProductId, color);
                playerControl.SetPet(HatManager.Instance.AllPets[pet].ProductId, color);
                playerControl.SetVisor(HatManager.Instance.AllVisors[visor].ProductId);
                playerControl.SetSkin(HatManager.Instance.AllSkins[skin].ProductId);
                playerControl.SetNamePlate(HatManager.Instance.AllNamePlates[nameplate].ProductId);
                GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
            }*/

            // Terminate round
            if (Input.GetKeyDown(KeyCode.L) && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ForceEnd, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.forceEnd();
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
