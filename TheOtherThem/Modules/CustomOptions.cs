using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.CustomOption;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using TMPro;

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
        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;

        public int id;
        public string name;
        public string format;
        public System.Object[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public List<CustomOption> children;
        public bool isHeader;
        public bool isHidden;
        public CustomOptionType type;

        public virtual bool enabled
        {
            get
            {
                return Helpers.RolesEnabled && this.GetBool();
            }
        }

        // Option creation
        public CustomOption()
        {

        }

        public CustomOption(int id, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            Init(id, name, selections, defaultValue, parent, isHeader, isHidden, format);
        }

        public void Init(int id, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            InitWithoutRegistration(id, name, selections, defaultValue, parent, isHeader, isHidden, format);

            options.Add(this);
        }

        // =========== INSERTABLE INITIALIZERS =========== \\

        public CustomOption(int id, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format, ref int index)
        {
            Init(id, name, selections, defaultValue, parent, isHeader, isHidden, format, ref index);
        }

        private void InitWithoutRegistration(int id, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            this.id = id;
            this.name = name;
            this.format = format;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.isHidden = isHidden;
            type = CustomOptionType.General;

            this.children = new List<CustomOption>();
            if (parent != null)
            {
                parent.children.Add(this);
            }

            selection = 0;
            if (id > 0)
            {
                entry = Main.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);

                if (options.Any(x => x.id == id))
                {
                    Main.Instance.Log.LogWarning($"CustomOption id {id} is used in multiple places.");
                }
            }
        }

        public void Init(int id, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format, ref int insertion)
        {
            InitWithoutRegistration(id, name, selections, defaultValue, parent, isHeader, isHidden, format);

            options.Insert(insertion++, this);
        }

        private static List<float> Range(float min, float max, float step)
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return selections;
        }

        public static CustomOption CreateInsertable(int id, string name, float defaultValue, float min, float max, float step, ref int index, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, Range(min, max, step).Cast<object>().ToArray(), defaultValue, parent, isHeader, isHidden, format, ref index);
        }

        public static CustomOption CreateInsertable(int id, string name, bool defaultValue, ref int index, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format, ref index);
        }

        public static CustomOption CreateInsertable(int id, string name, string[] selections, ref int index, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, selections, "", parent, isHeader, isHidden, format, ref index);
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

        public static void switchPreset(int newPreset)
        {
            CustomOption.preset = newPreset;
            foreach (CustomOption option in CustomOption.options)
            {
                if (option.id <= 0) continue;

                option.entry = Main.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.getString();
                }
            }
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;
            
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ShareOptions, Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)CustomOption.options.Count);
            foreach (CustomOption option in CustomOption.options)
            {
                messageWriter.WritePacked((uint)option.id);
                messageWriter.WritePacked((uint)Convert.ToUInt32(option.selection));
            }
            messageWriter.EndMessage();
        }

        // Getter

        public virtual int GetSelection()
        {
            return selection;
        }

        public virtual bool GetBool()
        {
            return selection > 0;
        }

        public virtual float GetFloat()
        {
            return (float)selections[selection];
        }

        public virtual string getString()
        {
            string sel = selections[selection].ToString();
            if (format != "")
            {
                return string.Format(ModTranslation.GetString(format), sel);
            }
            return ModTranslation.GetString(sel);
        }

        public virtual string getName()
        {
            return ModTranslation.GetString(name);
        }

        // Option changes

        public virtual void updateSelection(int newSelection)
        {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = getString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    if (id == 0) switchPreset(selection); // Switch presets
                    else if (entry != null) entry.Value = selection; // Save selection to config

                    ShareOptionSelections();// Share all selections
                }
            }
        }
    }


    public class CustomRoleOption : CustomOption
    {
        public CustomOption countOption = null;
        public bool roleEnabled = true;

        public override bool enabled
        {
            get
            {
                return Helpers.RolesEnabled && roleEnabled && selection > 0;
            }
        }

        public int rate
        {
            get
            {
                return enabled ? selection : 0;
            }
        }

        public int count
        {
            get
            {
                if (!enabled)
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
            base(id, Helpers.ColorString(color, name), CustomOptionHolder.rates, "", null, true, false, "")
        {
            this.roleEnabled = roleEnabled;

            if (max <= 0 || !roleEnabled)
            {
                isHidden = true;
                this.roleEnabled = false;
            }

            if (max > 1)
                countOption = Create(id + 10000, "roleNumAssigned", 1f, 1f, 15f, 1f, this, false, isHidden, "unitPlayers");
        }

        public CustomRoleOption(int id, string name, Color color, ref int insertion, int max = 15, bool roleEnabled = true) :
            base(id, Helpers.ColorString(color, name), CustomOptionHolder.rates, "", null, true, false, "", ref insertion)
        {
            this.roleEnabled = roleEnabled;

            if (max <= 0 || !roleEnabled)
            {
                isHidden = true;
                this.roleEnabled = false;
            }

            if (max > 1)
                countOption = CreateInsertable(id + 10000, "roleNumAssigned", 1f, 1f, 15f, 1f, ref insertion, this, false, isHidden, "unitPlayers");
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
            roleAssignEqually = new CustomOption(id + 10011, "roleAssignEqually", new string[] { "optionOn", "optionOff" }, "optionOff", this, false, isHidden, "");
            roleImpChance = Create(id + 10010, "roleImpChance", CustomOptionHolder.rates, roleAssignEqually, false, isHidden);

            this.roleType = roleType;
            type = CustomOptionType.General;
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
            return Helpers.generateTasks(commonTasks, shortTasks, longTasks);
        }

        public CustomTasksOption(int id, int commonDef, int longDef, int shortDef, CustomOption parent = null)
        {
            type = CustomOptionType.General;
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
                return roleTypes[selection];
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

    [Obsolete]
    public class CustomOptionBlank : CustomOption
    {
        public CustomOptionBlank(CustomOption parent)
        {
            this.parent = parent;
            this.id = -1;
            this.name = "";
            this.isHeader = false;
            this.isHidden = true;
            this.children = new List<CustomOption>();
            this.selections = new string[] { "" };
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

        public override string getString()
        {
            return "";
        }

        public override void updateSelection(int newSelection)
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
                torSettingsButton = GameObject.Instantiate(buttonTemplate, buttonTemplate.transform.parent);
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
            var relevantOptions = options.Where(x => x.type == optionType && optionType == CustomOptionType.General).ToList();

            if ((int)optionType == 99)
            {
                // Create 4 Groups with Role settings only
                relevantOptions.Clear();
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Impostor && x.isHeader));
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Neutral && x.isHeader));
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Crewmate && x.isHeader));
                relevantOptions.AddRange(options.Where(x => x.type == CustomOptionType.Modifier && x.isHeader));
                foreach (var option in options)
                {
                    if (option.parent != null && option.parent.GetSelection() > 0)
                    {
                        if (option.id == 103) //Deputy
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.sheriffSpawnRate) + 1, option);
                        else if (option.id == 224) //Sidekick
                            relevantOptions.Insert(relevantOptions.IndexOf(CustomOptionHolder.jackalSpawnRate) + 1, option);
                        else if (option.id == 358) //Prosecutor
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
                if (option.isHeader && (int)optionType != 99 || (int)optionType == 99 && curType != option.type)
                {
                    curType = option.type;
                    if (i != 0) num -= 0.59f;
                    if (i % 2 != 0) singles++;
                    headers++; // for header
                    CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(__instance.categoryHeaderOrigin);
                    categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 61);
                    categoryHeaderMasked.Title.text = ModTranslation.GetString(option.name);
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

                ViewSettingsInfoPanel viewSettingsInfoPanel = UnityEngine.Object.Instantiate<ViewSettingsInfoPanel>(__instance.infoPanelOrigin);
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
                viewSettingsInfoPanel.SetInfo(StringNames.ImpostorsCategory, ModTranslation.GetString(option.selections[value].ToString()), 61);
                viewSettingsInfoPanel.titleText.text = ModTranslation.GetString(option.name);
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

            removeVanillaTabs(__instance);

            createSettingTabs(__instance);
        }

        private static void createSettings(GameOptionsMenu menu, List<CustomOption> options)
        {
            float num = 1.5f;
            foreach (CustomOption option in options)
            {
                if (option.isHeader)
                {
                    CategoryHeaderMasked categoryHeaderMasked = UnityEngine.Object.Instantiate<CategoryHeaderMasked>(menu.categoryHeaderOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
                    categoryHeaderMasked.SetHeader(StringNames.ImpostorsCategory, 20);
                    categoryHeaderMasked.Title.text = ModTranslation.GetString(option.name);
                    categoryHeaderMasked.Title.outlineColor = Color.white;
                    categoryHeaderMasked.Title.outlineWidth = 0.2f;
                    categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                    categoryHeaderMasked.transform.localPosition = new Vector3(-0.903f, num, -2f);
                    num -= 0.63f;
                }
                OptionBehaviour optionBehaviour = UnityEngine.Object.Instantiate<StringOption>(menu.stringOptionOrigin, Vector3.zero, Quaternion.identity, menu.settingsContainer);
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
                stringOption.TitleText.text = option.name;
                if (option.isHeader && (option.type == CustomOptionType.Neutral || option.type == CustomOptionType.Crewmate || option.type == CustomOptionType.Impostor || option.type == CustomOptionType.Modifier))
                {
                    stringOption.TitleText.text = "Spawn Chance";
                }
                if (stringOption.TitleText.text.Length > 25)
                    stringOption.TitleText.fontSize = 2.2f;
                if (stringOption.TitleText.text.Length > 40)
                    stringOption.TitleText.fontSize = 2f;
                stringOption.Value = stringOption.oldValue = option.selection;
                stringOption.ValueText.text = option.selections[option.selection].ToString();
                option.optionBehaviour = stringOption;

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

        private static Vector3 ModButtonPosition = Vector3.zero;

        private static void removeVanillaTabs(GameSettingMenu __instance)
        {
            GameObject.Find("What Is This?")?.Destroy();
            GameObject result;
            if (result = GameObject.Find("GamePresetButton"))
            {
                ModButtonPosition = result.transform.localPosition;
                result.Destroy();
            }
            GameObject.Find("RoleSettingsButton")?.Destroy();
            __instance.ChangeTab(1, false);
        }

        public static void createCustomButton(GameSettingMenu __instance, int targetMenu, string buttonName, string buttonText)
        {
            var leftPanel = GameObject.Find("LeftPanel");
            var buttonTemplate = GameObject.Find("GameSettingsButton");
            var torSettingsButton = GameObject.Find(buttonName);
            if (torSettingsButton == null)
            {
                torSettingsButton = GameObject.Instantiate(buttonTemplate, leftPanel.transform);
                torSettingsButton.transform.localPosition = ModButtonPosition;
                torSettingsButton.name = buttonName;
                __instance.StartCoroutine(Effects.Lerp(2f, new Action<float>(p => { torSettingsButton.transform.FindChild("FontPlacer").GetComponentInChildren<TextMeshPro>().text = buttonText; })));
                var torSettingsPassiveButton = torSettingsButton.GetComponent<PassiveButton>();
                torSettingsPassiveButton.OnClick.RemoveAllListeners();
                torSettingsPassiveButton.OnClick.AddListener((System.Action)(() => {
                    __instance.ChangeTab(targetMenu, false);
                }));
                torSettingsPassiveButton.OnMouseOut.RemoveAllListeners();
                torSettingsPassiveButton.OnMouseOver.RemoveAllListeners();
                torSettingsPassiveButton.SelectButton(false);
                currentButtons.Add(torSettingsPassiveButton);
            }
        }

        public static void createGameOptionsMenu(GameSettingMenu __instance, CustomOptionType optionType, string settingName)
        {
            var tabTemplate = GameObject.Find("GAME SETTINGS TAB");
            currentTabs.RemoveAll(x => x == null);

            var torSettingsTab = GameObject.Instantiate(tabTemplate, tabTemplate.transform.parent);
            torSettingsTab.name = settingName;

            var torSettingsGOM = torSettingsTab.GetComponent<GameOptionsMenu>();
            foreach (var child in torSettingsGOM.Children)
            {
                child.Destroy();
            }
            torSettingsGOM.scrollBar.transform.FindChild("SliderInner").DestroyChildren();
            torSettingsGOM.Children.Clear();
            var relevantOptions = options.Where(x => x.type == optionType).ToList();
            createSettings(torSettingsGOM, relevantOptions);

            currentTabs.Add(torSettingsTab);
            torSettingsTab.SetActive(false);
        }

        private static void createSettingTabs(GameSettingMenu __instance)
        {
            // Handle different gamemodes and tabs needed therein.
            int next = 3;
            {

                // create TOR settings
                createCustomButton(__instance, next++, "TORSettings", ModTranslation.GetString("torSettings"));
                createGameOptionsMenu(__instance, CustomOptionType.General, "TORSettings");

            }
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>(_ => { });
            __instance.TitleText.text = ModTranslation.GetString(option.name);
            __instance.ValueText.text = ModTranslation.GetString(option.selections[option.selection].ToString());
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
    public class StringOptionFixedUpdate
    {
        public static void Postfix(StringOption __instance)
        {
            if (!IL2CPPChainloader.Instance.Plugins.TryGetValue("com.DigiWorm.LevelImposter", out PluginInfo _)) return;
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return;
            __instance.Value = __instance.oldValue = option.selection;
        }
    }


    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            CustomOption.ShareOptionSelections();
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
                CustomOption.ShareOptionSelections();
            }
        }
    }
}