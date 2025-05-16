using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;

namespace TheOtherThem {
    [Harmony]
    public class CustomOverlays {

        public static Sprite HelpButton { get; set; }
        public static bool OverlayShown { get; set; } = false;

        private static Sprite _colorBackground;
        private static SpriteRenderer _meetingUnderlay;
        private static SpriteRenderer _infoUnderlay;
        private static TMPro.TextMeshPro _infoOverlayRules;
        private static TMPro.TextMeshPro _infoOverlayRoles;

        public static void ResetOverlays()
        {
            HideBlackBackground();
            HideInfoOverlay();
            Object.Destroy(_meetingUnderlay);
            Object.Destroy(_infoUnderlay);
            Object.Destroy(_infoOverlayRules);
            Object.Destroy(_infoOverlayRoles);
            _meetingUnderlay = _infoUnderlay = null;
            _infoOverlayRules = _infoOverlayRoles = null;
            OverlayShown = false;
        }

        public static bool InitializeOverlays()
        {
            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (hudManager == null) return false;

            if (HelpButton == null)
            {
                HelpButton = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.HelpButton.png", 115f);
            }

            if (_colorBackground == null)
            {
                _colorBackground = Helpers.LoadSpriteFromResources("TheOtherThem.Resources.White.png", 100f);
            }

            if (_meetingUnderlay == null)
            {
                _meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                _meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
                _meetingUnderlay.gameObject.SetActive(true);
                _meetingUnderlay.enabled = false;
            }

            if (_infoUnderlay == null)
            {
                _infoUnderlay = UnityEngine.Object.Instantiate(_meetingUnderlay, hudManager.transform);
                _infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
                _infoUnderlay.gameObject.SetActive(true);
                _infoUnderlay.enabled = false;
            }

            if (_infoOverlayRules == null)
            {
                _infoOverlayRules = UnityEngine.Object.Instantiate(HudManager.Instance.TaskPanel.taskText, hudManager.transform);
                _infoOverlayRules.fontSize = _infoOverlayRules.fontSizeMin = _infoOverlayRules.fontSizeMax = 1.15f;
                _infoOverlayRules.autoSizeTextContainer = false;
                _infoOverlayRules.enableWordWrapping = false;
                _infoOverlayRules.alignment = TMPro.TextAlignmentOptions.TopLeft;
                _infoOverlayRules.transform.position = Vector3.zero;
                _infoOverlayRules.transform.localPosition = new Vector3(-2.5f, 1.15f, -910f);
                _infoOverlayRules.transform.localScale = Vector3.one;
                _infoOverlayRules.color = Palette.White;
                _infoOverlayRules.enabled = false;
            }

            if (_infoOverlayRoles == null) { 
                _infoOverlayRoles = UnityEngine.Object.Instantiate(_infoOverlayRules, hudManager.transform);
                _infoOverlayRoles.maxVisibleLines = 28;
                _infoOverlayRoles.fontSize = _infoOverlayRoles.fontSizeMin = _infoOverlayRoles.fontSizeMax = 1.15f;
                _infoOverlayRoles.outlineWidth += 0.02f;
                _infoOverlayRoles.autoSizeTextContainer = false;
                _infoOverlayRoles.enableWordWrapping = false;
                _infoOverlayRoles.alignment = TMPro.TextAlignmentOptions.TopLeft;
                _infoOverlayRoles.transform.position = Vector3.zero;
                _infoOverlayRoles.transform.localPosition = _infoOverlayRules.transform.localPosition + new Vector3(2.5f, 0.0f, 0.0f);
                _infoOverlayRoles.transform.localScale = Vector3.one;
                _infoOverlayRoles.color = Palette.White;
                _infoOverlayRoles.enabled = false;
            }

            return true;
        }

        public static void ShowBlackBackground()
        {
            if (HudManager.Instance == null) return;
            if (!InitializeOverlays()) return;

            _meetingUnderlay.sprite = _colorBackground;
            _meetingUnderlay.enabled = true;
            _meetingUnderlay.transform.localScale = new Vector3(20f, 20f, 1f);
            var clearBlack = new Color32(0, 0, 0, 0);

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                _meetingUnderlay.color = Color.Lerp(clearBlack, Palette.Black, t);
            })));
        }

        public static void HideBlackBackground()
        {
            if (_meetingUnderlay == null) return;
            _meetingUnderlay.enabled = false;
        }

        public static void ShowInfoOverlay()
        {
            if (OverlayShown || MapOptions.HideSettings) return;

            HudManager hudManager = DestroyableSingleton<HudManager>.Instance;
            if (ShipStatus.Instance == null || PlayerControl.LocalPlayer == null || hudManager == null || HudManager.Instance.IsIntroDisplayed || (!PlayerControl.LocalPlayer.CanMove && MeetingHud.Instance == null))
                return;

            if (!InitializeOverlays()) return;

            if (MapBehaviour.Instance != null)
                MapBehaviour.Instance.Close();

            hudManager.SetHudActive(false);

            OverlayShown = true;

            Transform parent;
            if (MeetingHud.Instance != null)
                parent = MeetingHud.Instance.transform;
            else
                parent = hudManager.transform;

            _infoUnderlay.transform.parent = parent;
            _infoOverlayRules.transform.parent = parent;
            _infoOverlayRoles.transform.parent = parent;

            _infoUnderlay.sprite = _colorBackground;
            _infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            _infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
            _infoUnderlay.enabled = true;

            _infoOverlayRules.enabled = true;

            string rolesText = "";
            foreach (RoleInfo r in RoleInfo.GetRoleInfoForPlayer(PlayerControl.LocalPlayer))
            {string roleDesc = r.FullDescription;
                rolesText += $"<size=150%>{r.NameColored}</size>" +
                    (roleDesc != "" ? $"\n{r.FullDescription}" : "") + "\n\n";
            }

            _infoOverlayRoles.text = rolesText;
            _infoOverlayRoles.enabled = true;

            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);
            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                _infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
                _infoOverlayRules.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
                _infoOverlayRoles.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            })));
        }

        public static void HideInfoOverlay()
        {
            if (!OverlayShown) return;

            if (MeetingHud.Instance == null) DestroyableSingleton<HudManager>.Instance.SetHudActive(true);

            OverlayShown = false;
            var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
            var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
            {
                if (_infoUnderlay != null)
                {
                    _infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                    if (t >= 1.0f) _infoUnderlay.enabled = false;
                }

                if (_infoOverlayRules != null)
                {
                    _infoOverlayRules.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) _infoOverlayRules.enabled = false;
                }

                if (_infoOverlayRoles != null)
                {
                    _infoOverlayRoles.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                    if (t >= 1.0f) _infoOverlayRoles.enabled = false;
                }
            })));
        }

        public static void ToggleInfoOverlay()
        {
            if (OverlayShown)
                HideInfoOverlay();
            else
                ShowInfoOverlay();
        }

        [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
        public static class CustomOverlayKeybinds
        {
            public static void Postfix()
            {
                if (Input.GetKeyDown(KeyCode.H) && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                    ToggleInfoOverlay();
            }
        }
    }
}