using AmongUs.GameOptions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.ToTRole;
using TMPro;
using UnityEngine;
using static TheOtherThem.CustomOption;

namespace TheOtherThem
{
    public enum CustomOptionType
    {
        General,
        Impostor,
        Neutral,
        Crewmate,
        Modifier,
    }

    public class CustomOption
    {
        public static List<CustomOption> Options { get; } = new List<CustomOption>();
        public static int Preset { get; set; } = 0;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public object[] Selections { get; set; }

        public int DefaultSelection { get; set; }
        public ConfigEntry<int> Entry { get; set; }
        public int Selection { get; set; }
        public OptionBehaviour OptionBehaviour { get; set; }
        public CustomOption Parent { get; set; }
        public List<CustomOption> Children { get; set; }
        public bool IsHeader { get; set; }
        public bool IsHidden { get; set; }
        public CustomOptionType Type { get; set; }

        public virtual bool Enabled => Helpers.RolesEnabled && GetBool();

        // Option creation
        public CustomOption() { }

        public CustomOption(int id, string name, object[] selections, object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            Init(id, name, selections, defaultValue, parent, isHeader, isHidden, format);
        }

        public void Init(int id, string name, object[] selections, object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            InitWithoutRegistration(id, name, selections, defaultValue, parent, isHeader, isHidden, format);

            Options.Add(this);
        }

        // =========== INSERTABLE INITIALIZERS =========== \\

        public CustomOption(int id, string name, object[] selections, object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format, int index)
        {
            Init(id, name, selections, defaultValue, parent, isHeader, isHidden, format, index);
        }

        private void InitWithoutRegistration(int id, string name, object[] selections, object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            Id = id;
            Name = name;
            Format = format;
            Selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            DefaultSelection = index >= 0 ? index : 0;
            Parent = parent;
            IsHeader = isHeader;
            IsHidden = isHidden;
            Type = CustomOptionType.General;

            Children = new List<CustomOption>();
            parent?.Children.Add(this);

            Selection = 0;
            if (id > 0)
            {
                Entry = Main.Instance.Config.Bind($"Preset{Preset}", id.ToString(), DefaultSelection);
                Selection = Mathf.Clamp(Entry.Value, 0, selections.Length - 1);

                if (Options.Any(x => x.Id == id))
                {
                    Main.Instance.Log.LogWarning($"CustomOption id {id} is used in multiple places.");
                }
            }
        }

        public void Init(int id, string name, object[] selections, object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format, int insertion)
        {
            InitWithoutRegistration(id, name, selections, defaultValue, parent, isHeader, isHidden, format);

            Options.Insert(insertion, this);
        }

        private static List<float> Range(float min, float max, float step)
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return selections;
        }

        public static int GetIndex(TeamTypeToT team)
        {
            // Creation order in CustomOptionHolder: Impostor => Neutral => Crewmate
            switch (team)
            {
                case TeamTypeToT.Crewmate:
                    return CustomOptionHolder.OptionInsertionIndices.Crewmate++;
                case TeamTypeToT.Neutral:
                    CustomOptionHolder.OptionInsertionIndices.Crewmate++;
                    return CustomOptionHolder.OptionInsertionIndices.Neutral++;
                case TeamTypeToT.Impostor:
                    CustomOptionHolder.OptionInsertionIndices.Crewmate++;
                    CustomOptionHolder.OptionInsertionIndices.Neutral++;
                    return CustomOptionHolder.OptionInsertionIndices.Impostor++;
                default:
                    Main.Logger.LogError($"This couldn't be possible, but argument {nameof(team)} still had a value {team} out of enum {nameof(TeamTypeToT)} members.");
                    throw new Exception("JUST LET USER KNOW AN ERROR OCCURED THROUGH NOTIFICATOR");
            }
        }

        public static CustomOption CreateInsertable(int id, string name, float defaultValue, float min, float max, float step, TeamTypeToT team, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            var index = GetIndex(team);
            return new CustomOption(id, name, Range(min, max, step).Cast<object>().ToArray(), defaultValue, parent, isHeader, isHidden, format, index);
        }

        public static CustomOption CreateInsertable(int id, string name, bool defaultValue, TeamTypeToT team, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            var index = GetIndex(team);
            return new CustomOption(id, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format, index);
        }

        public static CustomOption CreateInsertable(int id, string name, string[] selections, TeamTypeToT team, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            var index = GetIndex(team);
            return new CustomOption(id, name, selections, "", parent, isHeader, isHidden, format, index);
        }

        // =============================================== \\

        public static CustomOption Create(int id, string name, string[] selections, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, selections, "", parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, Range(min, max, step).Cast<object>().ToArray(), defaultValue, parent, isHeader, isHidden, format);
        }


        public static CustomOption Create(int id, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format);
        }

        // Static behaviour

        public static void SwitchPreset(int newPreset)
        {
            Preset = newPreset;
            foreach (CustomOption option in Options)
            {
                if (option.Id <= 0) continue;

                option.Entry = Main.Instance.Config.Bind($"Preset{Preset}", option.Id.ToString(), option.DefaultSelection);
                option.Selection = Mathf.Clamp(option.Entry.Value, 0, option.Selections.Length - 1);
                if (option.OptionBehaviour != null && option.OptionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.Selection;
                    stringOption.ValueText.text = option.GetString();
                }
            }
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;
            
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ShareOptions, SendOption.Reliable);
            messageWriter.WritePacked((uint)Options.Count);
            foreach (CustomOption option in Options)
            {
                messageWriter.WritePacked((uint)option.Id);
                messageWriter.WritePacked(Convert.ToUInt32(option.Selection));
            }
            messageWriter.EndMessage();
        }

        // Getter

        public virtual int GetSelection()
        {
            return Selection;
        }

        public virtual bool GetBool()
        {
            return Selection > 0;
        }

        public virtual float GetFloat()
        {
            return (float)Selections[Selection];
        }

        public virtual string GetString()
        {
            string sel = Selections[Selection].ToString();
            if (Format != "")
            {
                return string.Format(ModTranslation.GetString(Format), sel);
            }
            return ModTranslation.GetString(sel);
        }

        public virtual string GetName()
        {
            return ModTranslation.GetString(Name);
        }

        // Option changes

        public virtual void UpdateSelection(int newSelection)
        {
            Selection = Mathf.Clamp((newSelection + Selections.Length) % Selections.Length, 0, Selections.Length - 1);
            if (OptionBehaviour != null && OptionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = Selection;
                stringOption.ValueText.text = GetString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    if (Id == 0) SwitchPreset(Selection); // Switch presets
                    else if (Entry != null) Entry.Value = Selection; // Save selection to config

                    ShareOptionSelections();// Share all selections
                }
            }
        }
    }


    public class CustomRoleOption : CustomOption
    {
        public CustomOption countOption = null;
        public bool roleEnabled = true;

        public override bool Enabled
        {
            get
            {
                return Helpers.RolesEnabled && roleEnabled && Selection > 0;
            }
        }

        public int rate
        {
            get
            {
                return Enabled ? Selection : 0;
            }
        }

        public int count
        {
            get
            {
                if (!Enabled)
                    return 0;

                if (countOption != null)
                    return Mathf.RoundToInt(countOption.GetFloat());

                return 1;
            }
        }

        public (int, int) Data
        {
            get
            {
                return (rate, count);
            }
        }

        public CustomRoleOption(int id, string name, Color color, int max = 15, bool roleEnabled = true) :
            base(id, Helpers.ColorString(color, name), CustomOptionHolder.Rates.ToArray(), "", null, true, false, "")
        {
            this.roleEnabled = roleEnabled;

            if (max <= 0 || !roleEnabled)
            {
                IsHidden = true;
                this.roleEnabled = false;
            }

            if (max > 1)
                countOption = Create(id + 10000, "roleNumAssigned", 1f, 1f, 15f, 1f, this, false, IsHidden, "unitPlayers");
        }

        // Insertable
        public CustomRoleOption(int id, string name, Color color, TeamTypeToT team, int max = 15, bool roleEnabled = true) :
            base(id, Helpers.ColorString(color, name), CustomOptionHolder.Rates.ToArray(), "", null, true, false, "", GetIndex(team))
        {
            this.roleEnabled = roleEnabled;

            if (max <= 0 || !roleEnabled)
            {
                IsHidden = true;
                this.roleEnabled = false;
            }

            if (max > 1)
                countOption = CreateInsertable(id + 10000, "roleNumAssigned", 1f, 1f, 15f, 1f, team, this, false, IsHidden, "unitPlayers");
        }
    }

    public class CustomDualRoleOption : CustomRoleOption
    {
        public static List<CustomDualRoleOption> dualRoles = new List<CustomDualRoleOption>();
        public CustomOption roleImpChance = null;
        public CustomOption roleAssignEqually = null;
        public RoleType roleType;

        public int impChance { get { return roleImpChance.GetSelection(); } }
        
        public bool assignEqually { get { return roleAssignEqually.GetSelection() == 0; } }

        public CustomDualRoleOption(int id, string name, Color color, RoleType roleType, int max = 15, bool roleEnabled = true) : base(id, name, color, max, roleEnabled)
        {
            roleAssignEqually = new CustomOption(id + 10011, "roleAssignEqually", new string[] { "optionOn", "optionOff" }, "optionOff", this, false, IsHidden, "");
            roleImpChance = Create(id + 10010, "roleImpChance", CustomOptionHolder.Rates.ToArray(), roleAssignEqually, false, IsHidden);

            this.roleType = roleType;
            Type = CustomOptionType.General;
            dualRoles.Add(this);
        }
    }

    public class CustomTasksOption : CustomOption
    {
        public CustomOption commonTasksOption = null;
        public CustomOption longTasksOption = null;
        public CustomOption shortTasksOption = null;

        public int commonTasks { get { return Mathf.RoundToInt(commonTasksOption.GetSelection()); } }
        public int longTasks { get { return Mathf.RoundToInt(longTasksOption.GetSelection()); } }
        public int shortTasks { get { return Mathf.RoundToInt(shortTasksOption.GetSelection()); } }

        public List<byte> generateTasks()
        {
            return Helpers.GenerateTasks(commonTasks, shortTasks, longTasks);
        }

        public CustomTasksOption(int id, int commonDef, int longDef, int shortDef, CustomOption parent = null)
        {
            Type = CustomOptionType.General;
            commonTasksOption = Create(id + 20000, "numCommonTasks", commonDef, 0f, 4f, 1f, parent);
            longTasksOption = Create(id + 20001, "numLongTasks", longDef, 0f, 15f, 1f, parent);
            shortTasksOption = Create(id + 20002, "numShortTasks", shortDef, 0f, 23f, 1f, parent);
        }
    }

    public class CustomRoleSelectionOption : CustomOption
    {
        public List<RoleType> roleTypes;

        public RoleType role
        {
            get
            {
                return roleTypes[Selection];
            }
        }

        public CustomRoleSelectionOption(int id, string name, List<RoleType> roleTypes = null, CustomOption parent = null)
        {
            if (roleTypes == null)
            {
                roleTypes = Enum.GetValues(typeof(RoleType)).Cast<RoleType>().ToList();
            }

            this.roleTypes = roleTypes;
            var strings = roleTypes.Select(
                x => 
                    x == RoleType.NoRole ? "optionOff" :
                    RoleInfo.AllRoleInfos.First(y => y.MyRoleType == x).NameColored
                ).ToArray();

            Init(id, name, strings, 0, parent, false, false, "");
        }
    }

    [Obsolete("The new version of the game option doesn't need this anymore.")]
    public class CustomOptionBlank : CustomOption
    {
        public CustomOptionBlank(CustomOption parent)
        {
            this.Parent = parent;
            this.Id = -1;
            this.Name = "";
            this.IsHeader = false;
            this.IsHidden = true;
            this.Children = new List<CustomOption>();
            this.Selections = new string[] { "" };
            //options.Add(this);
        }

        public override int GetSelection()
        {
            return 0;
        }

        public override bool GetBool()
        {
            return true;
        }

        public override float GetFloat()
        {
            return 0f;
        }

        public override string GetString()
        {
            return "";
        }

        public override void UpdateSelection(int newSelection)
        {
            return;
        }

    }
    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
    class GameOptionsMenuChangeTabPatch
    {
        public static void Postfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
        {
            if (previewOnly) return;
            foreach (var tab in GameOptionsMenuStartPatch.currentTabs)
            {
                if (tab != null)
                    tab.SetActive(false);
            }
            foreach (var pbutton in GameOptionsMenuStartPatch.currentButtons)
            {
                pbutton.SelectButton(false);
            }

            if (tabNum > 2)
            {
                tabNum -= 3;
                GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
                GameOptionsMenuStartPatch.currentButtons[tabNum].SelectButton(true);
            }
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.SetTab))]
    class LobbyViewSettingsPaneRefreshTabPatch
    {
        public static bool Prefix(LobbyViewSettingsPane __instance)
        {
            if ((int)__instance.currentTab < 15)
            {
                LobbyViewSettingsPaneChangeTabPatch.Postfix(__instance, __instance.currentTab);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.ChangeTab))]
    class LobbyViewSettingsPaneChangeTabPatch
    {
        public static void Postfix(LobbyViewSettingsPane __instance, StringNames category)
        {
            int tabNum = (int)category;

            foreach (var pbutton in LobbyViewSettingsPatch.currentButtons)
            {
                pbutton.SelectButton(false);
            }
            if (tabNum > 20) // StringNames are in the range of 3000+ 
                return;
            __instance.taskTabButton.SelectButton(false);

            if (tabNum > 2)
            {
                tabNum -= 3;
                //GameOptionsMenuStartPatch.currentTabs[tabNum].SetActive(true);
                LobbyViewSettingsPatch.currentButtons[tabNum].SelectButton(true);
                LobbyViewSettingsPatch.drawTab(__instance, LobbyViewSettingsPatch.currentButtonTypes[tabNum]);
            }
        }
    }

    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Update))]
    class LobbyViewSettingsPaneUpdatePatch
    {
        public static void Postfix(LobbyViewSettingsPane __instance)
        {
            if (LobbyViewSettingsPatch.currentButtons.Count == 0)
            {
                LobbyViewSettingsPatch.gameModeChangedFlag = true;
                LobbyViewSettingsPatch.Postfix(__instance);

            }
        }
    }


    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.Awake))]
    class LobbyViewSettingsPatch
    {
        public static List<PassiveButton> currentButtons = new();
        public static List<CustomOptionType> currentButtonTypes = new();
        public static bool gameModeChangedFlag = false;

        public static void createCustomButton(LobbyViewSettingsPane __instance, int targetMenu, string buttonName, string buttonText, CustomOptionType optionType)
        {
            buttonName = "View" + buttonName;
            var buttonTemplate = GameObject.Find("OverviewTab");
            var torSettingsButton = GameObject.Find(buttonName);
            if (torSettingsButton == null)
            {
                torSettingsButton = Object.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                torSettingsButton.transform.localPosition += Vector3.right * 1.75f * (targetMenu - 2);
                torSettingsButton.name = buttonName;
                __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
                var torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
                torSettingsPassiveButton.OnClick.RemoveAllListeners();
                torSettingsPassiveButton.OnClick.AddListener((System.Action)(() => {
                    __instance.ChangeTab((StringNames)targetMenu);
                }));
                torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
                torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
                torSettingsPassiveButton.SelectButton(false);
                currentButtons.Add(torSettingsPassiveButton);
                currentButtonTypes.Add(optionType);
            }
        }

        public static void Postfix(LobbyViewSettingsPane __instance)
        {
            currentButtons.ForEach(x => x?.Destroy());
            currentButtons.Clear();
            currentButtonTypes.Clear();

            removeVanillaTabs(__instance);

            createSettingTabs(__instance);

        }

        public static void removeVanillaTabs(LobbyViewSettingsPane __instance)
        {
            GameObject.Find("RolesTabs")?.Destroy();
            var overview = GameObject.Find("OverviewTab");
            if (!gameModeChangedFlag)
            {
                overview.transform.localScale = new Vector3(0.5f * overview.transform.localScale.x, overview.transform.localScale.y, overview.transform.localScale.z);
                overview.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

            }
            overview.transform.Find("FontPlacer").transform.localScale = new Vector3(1.35f, 1f, 1f);
            overview.transform.Find("FontPlacer").transform.localPosition = new Vector3(-0.6f, -0.1f, 0f);
            gameModeChangedFlag = false;
        }

        public static void drawTab(LobbyViewSettingsPane __instance, CustomOptionType optionType)
        {
            var relevantOptions = Options.Where(x => x.Type == optionType && optionType == CustomOptionType.General).ToList();

            if ((int)optionType == 99)
            {
                // Create 4 Groups with Role settings only
                relevantOptions.Clear();
                relevantOptions.AddRange(Options.Where(x => x.Type == CustomOptionType.Impostor && x.IsHeader));
                relevantOptions.AddRange(Options.Where(x => x.Type == CustomOptionType.Neutral && x.IsHeader));
                relevantOptions.AddRange(Options.Where(x => x.Type == CustomOptionType.Crewmate && x.IsHeader));
                relevantOptions.AddRange(Options.Where(x => x.Type == CustomOptionType.Modifier && x.IsHeader));
                foreach (var option in Options)
                {
                    if (option.Parent != null && option.Parent.GetSelection() > 0)
                    {
                        if (option.Id == 103) //Deputy
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.sheriffSpawnRate) + 1, option);
                        else if (option.Id == 224) //Sidekick
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.jackalSpawnRate) + 1, option);
                        else if (option.Id == 358) //Prosecutor
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.lawyerSpawnRate) + 1, option);
                    }
                }
            }

            for (int j = 0; j < __instance.settingsInfo.Count; j++)
            {
                __instance.settingsInfo[j].gameObject.Destroy();
            }
            __instance.settingsInfo.Clear();

            float num = 1.44f;
            int i = 0;
            int singles = 0;
            int headers = 0;
            int lines = 0;
            var curType = CustomOptionType.Modifier;

            foreach (var option in relevantOptions)
            {
                if (option.IsHeader && (int)optionType != 99 || (int)optionType == 99 && curType != option.Type)
                {
                    curType = option.Type;
                    if (i != 0) num -= 0.59f;
                    if (i % 2 != 0) singles++;
                    headers++; // for header
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate<CategoryHeaderMasked>(__instance.categoryHeaderOrigin);
                    categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 61);
                    categoryHeaderMasked.Title.text = ModTranslation.GetString(option.Name);
                    if ((int)optionType == 99)
                        categoryHeaderMasked.Title.text = new Dictionary<CustomOptionType, string>() { { CustomOptionType.Impostor, "Impostor Roles" }, { CustomOptionType.Neutral, "Neutral Roles" },
                            { CustomOptionType.Crewmate, "Crewmate Roles" }, { CustomOptionType.Modifier, "Modifiers" } }[curType];
                    categoryHeaderMasked.Title.outlineColor = Color.white;
                    categoryHeaderMasked.Title.outlineWidth = 0.2f;
                    categoryHeaderMasked.transform.SetParent(__instance.settingsContainer);
                    categoryHeaderMasked.transform.localScale = Vector3.one;
                    categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
                    __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
                    num -= 0.85f;
                    i = 0;
                }

                ViewSettingsInfoPanel viewSettingsInfoPanel = Object.Instantiate<ViewSettingsInfoPanel>(__instance.infoPanelOrigin);
                viewSettingsInfoPanel.transform.SetParent(__instance.settingsContainer);
                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                float num2;
                if (i % 2 == 0)
                {
                    lines++;
                    num2 = -8.95f;
                    if (i > 0)
                    {
                        num -= 0.59f;
                    }
                }
                else
                {
                    num2 = -3f;
                }
                viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);
                int value = option.GetSelection();
                viewSettingsInfoPanel.SetInfo(StringNames.ImpostorsCategory, ModTranslation.GetString(option.Selections[value].ToString()), 61);
                viewSettingsInfoPanel.titleText.text = ModTranslation.GetString(option.Name);
                if ((int)optionType == 99)
                {
                    viewSettingsInfoPanel.titleText.outlineColor = Color.white;
                    viewSettingsInfoPanel.titleText.outlineWidth = 0.2f;
                }
                __instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

                i++;
            }

            float actual_spacing = (headers * 0.85f + lines * 0.59f) / (headers + lines);
            __instance.scrollBar.CalculateAndSetYBounds((float)(__instance.settingsInfo.Count + singles * 2 + headers), 2f, 6f, actual_spacing);
        }

        public static void createSettingTabs(LobbyViewSettingsPane __instance)
        {
            // Handle different gamemodes and tabs needed therein.
            int next = 3;
            // create TOR settings
            createCustomButton(__instance, next++, "TORSettings", ModTranslation.GetString("ModSettings"), CustomOptionType.General);
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.CreateSettings))]
    class GameOptionsMenuCreateSettingsPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.gameObject.name == "GAME SETTINGS TAB")
                adaptTaskCount(__instance);
        }

        private static void adaptTaskCount(GameOptionsMenu __instance)
        {
            // Adapt task count for main options
            var commonTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumCommonTasks).Cast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);
            var shortTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumShortTasks).TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);
            var longTasksOption = __instance.Children.ToArray().FirstOrDefault(x => x.TryCast<NumberOption>()?.intOptionName == Int32OptionNames.NumLongTasks).TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        static void Postfix()
        {
            var gameSettingsButton = GameObject.Find("GameSettingsButton");
            if (gameSettingsButton)
                gameSettingsButton.transform.localPosition = GameOptionsMenuStartPatch.FirstLeftButtonPosition;

            var torSettingsButton = GameObject.Find("TORSettings");
            if (torSettingsButton)
                torSettingsButton.transform.localPosition = GameOptionsMenuStartPatch.SecondLeftButtonPosition;
        }
    }


    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static List<GameObject> currentTabs = new();
        public static List<PassiveButton> currentButtons = new();

        public static void Postfix(GameSettingMenu __instance)
        {
            currentTabs.ForEach(x => x?.Destroy());
            currentButtons.ForEach(x => x?.Destroy());
            currentTabs = new();
            currentButtons = new();

            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;

            RemoveVanillaTabs(__instance);

            CreateSettingTabs(__instance);
        }

        private static void CreateSettings(GameOptionsMenu menu, List<CustomOption> options)
        {
            float num = 1.5f;
            foreach (CustomOption option in options)
            {
                if (option.IsHeader)
                {
                    CategoryHeaderMasked categoryHeaderMasked = Object.Instantiate(menu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
                    categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 20);
                    categoryHeaderMasked.Title.text = ModTranslation.GetString(option.Name);
                    categoryHeaderMasked.Title.outlineColor = Color.white;
                    categoryHeaderMasked.Title.outlineWidth = 0.2f;
                    categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                    categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                    num -= 0.63f;
                }
                OptionBehaviour optionBehaviour = Object.Instantiate<StringOption>(menu.stringOptionOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
                optionBehaviour.transform.localPosition = new Vector3(0.952f, num, -2f);
                optionBehaviour.SetClickMask(menu.ButtonClickMask);

                // "SetUpFromData"
                SpriteRenderer[] componentsInChildren = optionBehaviour.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
                }
                foreach (TextMeshPro textMeshPro in optionBehaviour.GetComponentsInChildren<TextMeshPro>(true))
                {
                    textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
                    textMeshPro.fontMaterial.SetFloat("_Stencil", (float)20);
                }

                var stringOption = optionBehaviour as StringOption;
                stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                stringOption.TitleText.text = option.Name;
                if (option.IsHeader && (option.Type == CustomOptionType.Neutral || option.Type == CustomOptionType.Crewmate || option.Type == CustomOptionType.Impostor || option.Type == CustomOptionType.Modifier))
                {
                    stringOption.TitleText.text = "Spawn Chance";
                }
                if (stringOption.TitleText.text.Length > 25)
                    stringOption.TitleText.fontSize = 2.2f;
                if (stringOption.TitleText.text.Length > 40)
                    stringOption.TitleText.fontSize = 2f;
                stringOption.Value = stringOption.oldValue = option.Selection;
                stringOption.ValueText.text = option.Selections[option.Selection].ToString();
                option.OptionBehaviour = stringOption;

                menu.Children.Add(optionBehaviour);
                num -= 0.45f;
                menu.scrollBar.SetYBoundsMax(-num - 1.65f);
            }

            for (int i = 0; i < menu.Children.Count; i++)
            {
                OptionBehaviour optionBehaviour = menu.Children[i];
                if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
                {
                    optionBehaviour.SetAsPlayer();
                }
            }
        }

        public static Vector3 FirstLeftButtonPosition { get; set; } = Vector3.zero;
        public static Vector3 SecondLeftButtonPosition { get; set; } = Vector3.zero;

        private static void RemoveVanillaTabs(GameSettingMenu __instance)
        {
            GameObject.Find("What Is This?")?.Destroy();
            GameObject result;
            if (result = GameObject.Find("GamePresetButton"))
            {
                SecondLeftButtonPosition = result.transform.localPosition;
                result.Destroy();
            }

            if (result = GameObject.Find("GameSettingsButton"))
                FirstLeftButtonPosition = result.transform.localPosition;

            GameObject.Find("RoleSettingsButton")?.Destroy();
            __instance.ChangeTab(1, false);
        }

        public static void CreateCustomButton(GameSettingMenu __instance, int targetMenu, string buttonName, string buttonText)
        {
            var leftPanel = GameObject.Find("LeftPanel");

            var buttonTemplate = GameObject.Find("GameSettingsButton");
            if (buttonTemplate)
                buttonTemplate.transform.localPosition = FirstLeftButtonPosition;

            var torSettingsButton = GameObject.Find(buttonName);
            if (!torSettingsButton)
            {
                torSettingsButton = Object.Instantiate(buttonTemplate, leftPanel.transform);
                torSettingsButton.transform.localPosition = SecondLeftButtonPosition;
                torSettingsButton.name = buttonName;
                __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
                var torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
                torSettingsPassiveButton.OnClick.RemoveAllListeners();
                torSettingsPassiveButton.OnClick.AddListener((Action)(() => {
                    __instance.ChangeTab(targetMenu, false);
                }));
                torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
                torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
                torSettingsPassiveButton.SelectButton(false);
                currentButtons.Add(torSettingsPassiveButton);
            }
        }

        public static void CreateGameOptionsMenu(CustomOptionType optionType, string settingName)
        {
            var tabTemplate = GameObject.Find("GAME SETTINGS TAB");
            currentTabs.RemoveAll(x => x == null);

            var torSettingsTab = Object.Instantiate(tabTemplate, tabTemplate.transform.parent);
            torSettingsTab.name = settingName;

            var torSettingsGOM = torSettingsTab.GetComponent<GameOptionsMenu>();
            foreach (var child in torSettingsGOM.Children)
            {
                child.Destroy();
            }
            torSettingsGOM.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
            torSettingsGOM.Children.Clear();
            var relevantOptions = Options.Where(x => x.Type == optionType).ToList();
            CreateSettings(torSettingsGOM, relevantOptions);

            currentTabs.Add(torSettingsTab);
            torSettingsTab.SetActive(false);
        }

        private static void CreateSettingTabs(GameSettingMenu __instance)
        {
            // Handle different gamemodes and tabs needed therein.
            int next = 3;
            {
                // create TOR settings
                CreateCustomButton(__instance, next++, "TORSettings", ModTranslation.GetString("torSettings"));
                CreateGameOptionsMenu(CustomOptionType.General, "TORSettings");
            }
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>(_ => { });
            __instance.TitleText.text = ModTranslation.GetString(option.Name);
            __instance.ValueText.text = ModTranslation.GetString(option.Selections[option.Selection].ToString());
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
            if (option == null) return true;
            option.UpdateSelection(option.Selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            var option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
            if (option == null) return true;
            option.UpdateSelection(option.Selection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
    public class StringOptionFixedUpdate
    {
        public static void Postfix(StringOption __instance)
        {
            if (!IL2CPPChainloader.Instance.Plugins.TryGetValue("com.DigiWorm.LevelImposter", out PluginInfo _)) return;
            var option = Options.FirstOrDefault(option => option.OptionBehaviour == __instance);
            if (option == null) return;
            __instance.Value = __instance.oldValue = option.Selection;
        }
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            ShareOptionSelections();
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer != null && AmongUsClient.Instance.AmHost)
            {
                GameManager.Instance.LogicOptions.SyncOptions();
                ShareOptionSelections();
            }
        }
    }
}