using AmongUs.Data.Legacy;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem
{
    public class ModTranslation
    {
        public const int DefaultLanguage = (int)SupportedLangs.English;
        public static Dictionary<string, Dictionary<int, string>> StringData { get; set; }

        private const string BlankText = "[BLANK]";

        public ModTranslation()
        {

        }

        public static void Load()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("TheOtherThem.Resources.StringData.json");
            var byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);
            string json = System.Text.Encoding.UTF8.GetString(byteArray);

            StringData = new Dictionary<string, Dictionary<int, string>>();
            JObject parsed = JObject.Parse(json);

            for (int i = 0; i < parsed.Count; i++)
            {
                JProperty token = parsed.ChildrenTokens[i].TryCast<JProperty>();
                if (token == null) continue;

                string stringName = token.Name;
                var val = token.Value.TryCast<JObject>();

                if (token.HasValues)
                {
                    var strings = new Dictionary<int, string>();

                    for (int j = 0; j < (int)SupportedLangs.Irish + 1; j++)
                    {
                        string key = j.ToString();
                        var text = val[key]?.TryCast<JValue>().Value.ToString();

                        if (text != null && text.Length > 0)
                        {
                            if (text == BlankText) strings[j] = "";
                            else strings[j] = text;
                        }
                    }

                    StringData[stringName] = strings;
                }
            }

            //TheOtherRolesPlugin.Instance.Log.LogInfo($"Language: {stringData.Keys}");
        }

        public static string GetString(string key, string def = null)
        {
            // Strip out color tags.
            string keyClean = Regex.Replace(key, "<.*?>", "");
            keyClean = Regex.Replace(keyClean, "^-\\s*", "");
            keyClean = keyClean.Trim();

            def ??= key;
            if (!StringData.ContainsKey(keyClean))
            {
                return def;
            }

            var data = StringData[keyClean];
            var lang = TranslationController.InstanceExists ? (int)TranslationController.Instance.currentLanguage.languageID : (int)LegacySaveManager.LastLanguage;

            if (data.ContainsKey(lang))
            {
                return key.Replace(keyClean, data[lang]);
            }
            else if (data.ContainsKey(DefaultLanguage))
            {
                return key.Replace(keyClean, data[DefaultLanguage]);
            }

            return key;
        }

        public static string GetString(StringNames key, string def = null, params Il2CppSystem.Object[] parts) => TranslationController.Instance.GetString(key.ToString(), def, parts);

        public static Sprite GetImage(string key, float pixelsPerUnit)
        {
            key = GetString(key);
            key = key.Replace("/", ".");
            key = key.Replace("\\", ".");
            key = "TheOtherThem.Resources." + key;

            return Helpers.LoadSpriteFromResources(key, pixelsPerUnit);
        }
    }

    [HarmonyPatch(typeof(LanguageSetter), nameof(LanguageSetter.SetLanguage))]
    class SetLanguagePatch
    {
        static void Postfix()
        {
            ClientOptionsPatch.UpdateTranslations();
        }
    }
}