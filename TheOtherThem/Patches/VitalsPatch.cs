using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static TheOtherThem.GameHistory;
using static TheOtherThem.TheOtherRoles;

namespace TheOtherThem.Patches
{
    [Harmony]
    public class VitalsPatch
    {
        private static float _vitalsTimer = 0f;
        private static TextMeshPro _timeRemaining;
        private static List<TextMeshPro> _hackerTexts = new();

        public static void ResetData()
        {
            _vitalsTimer = 0f;
            if (_timeRemaining != null)
            {
                Object.Destroy(_timeRemaining);
                _timeRemaining = null;
            }
        }

        static void UseVitalsTime()
        {
            // Don't waste network traffic if we're out of time.
            if (MapOptions.restrictDevices > 0 && MapOptions.restrictVitalsTime > 0f && PlayerControl.LocalPlayer.IsAlive())
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.UseVitalsTime, SendOption.Reliable, -1);
                writer.Write(_vitalsTimer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.useVitalsTime(_vitalsTimer);
            }
            _vitalsTimer = 0f;
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Begin))]
        class VitalsMinigameStartPatch
        {
            static void Postfix(VitalsMinigame __instance)
            {
                _vitalsTimer = 0f;

                if (Hacker.hacker != null && PlayerControl.LocalPlayer == Hacker.hacker)
                {
                    _hackerTexts = new List<TMPro.TextMeshPro>();
                    foreach (VitalsPanel panel in __instance.vitals)
                    {
                        TMPro.TextMeshPro text = Object.Instantiate(__instance.SabText, panel.transform);
                        _hackerTexts.Add(text);
                        Object.DestroyImmediate(text.GetComponent<AlphaBlink>());
                        text.gameObject.SetActive(false);
                        text.transform.localScale = Vector3.one * 0.75f;
                        text.transform.localPosition = new Vector3(-0.75f, -0.23f, 0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        class VitalsMinigameUpdatePatch
        {
            static bool Prefix(VitalsMinigame __instance)
            {
                _vitalsTimer += Time.deltaTime;
                if (_vitalsTimer > 0.1f)
                    UseVitalsTime();

                if (MapOptions.restrictDevices > 0)
                {
                    if (_timeRemaining == null)
                    {
                        _timeRemaining = UnityEngine.Object.Instantiate(HudManager.Instance.TaskPanel.taskText, __instance.transform);
                        _timeRemaining.alignment = TMPro.TextAlignmentOptions.BottomRight;
                        _timeRemaining.transform.position = Vector3.zero;
                        _timeRemaining.transform.localPosition = new Vector3(1.7f, 4.45f);
                        _timeRemaining.transform.localScale *= 1.8f;
                        _timeRemaining.color = Palette.White;
                    }

                    if (MapOptions.restrictVitalsTime <= 0f)
                    {
                        __instance.Close();
                        return false;
                    }

                    string timeString = TimeSpan.FromSeconds(MapOptions.restrictVitalsTime).ToString(@"mm\:ss\.ff");
                    _timeRemaining.text = String.Format(ModTranslation.GetString("timeRemaining"), timeString);
                    _timeRemaining.gameObject.SetActive(true);
                }

                return true;
            }

            static void Postfix(VitalsMinigame __instance)
            {

                // Hacker show time since death
                if (Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && Hacker.hackerTimer > 0)
                {
                    for (int k = 0; k < __instance.vitals.Length; k++)
                    {
                        VitalsPanel vitalsPanel = __instance.vitals[k];
                        NetworkedPlayerInfo player = GameData.Instance.AllPlayers[k];

                        // Hacker update
                        if (vitalsPanel.IsDead)
                        {
                            DeadPlayer deadPlayer = DeadPlayers?.Where(x => x.player?.PlayerId == player?.PlayerId)?.FirstOrDefault();
                            if (deadPlayer != null && k < _hackerTexts.Count && _hackerTexts[k] != null)
                            {
                                float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                                _hackerTexts[k].gameObject.SetActive(true);
                                _hackerTexts[k].text = Mathf.CeilToInt(timeSinceDeath / 1000) + "s";
                            }
                        }
                    }
                }
                else
                {
                    foreach (TMPro.TextMeshPro text in _hackerTexts)
                        if (text != null && text.gameObject != null)
                            text.gameObject.SetActive(false);
                }
            }
        }

        // Minigame.Close(bool allowMovement) is marked by ObsoleteAttribute("Don't use, I just don't want to reselect the close button event handlers", true)
        // So if this overload is used the game can't be compiled
        // What's more, the implementation of it directly calls Minigame.Close()
        // In short, there's no need to patch this overload
        [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[] { })] 
        class VitalsMinigameClosePatch
        {
            static void Prefix(Minigame __instance)
            {
                if (__instance is VitalsMinigame)
                    UseVitalsTime();
            }
        }
    }
}