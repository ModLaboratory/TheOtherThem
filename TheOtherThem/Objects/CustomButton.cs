using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheOtherThem.Objects
{
    public class CustomButton
    {
        public static List<CustomButton> Buttons = new List<CustomButton>();
        public ActionButton actionButton;
        public Vector3? PositionOffset;
        public Vector3 LocalScale = Vector3.one;
        public float MaxTimer = float.MaxValue;
        public float Timer = 0f;
        public bool effectCancellable = false;
        public Action OnClick;
        public Action OnMeetingEnds;
        public Func<bool> HasButton;
        public Func<bool> CouldUse;
        public Action OnEffectEnds;
        public bool HasEffect;
        public bool isEffectActive = false;
        public bool showButtonText = true;
        public string buttonText = null;
        public float EffectDuration;
        public Sprite Sprite;
        private HudManager hudManager;
        private bool mirror;
        private KeyCode? hotkey;

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3? PositionOffset, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false, string buttonText = null)
        {
            this.hudManager = hudManager;
            this.OnClick = OnClick;
            this.HasButton = HasButton;
            this.CouldUse = CouldUse;
            this.PositionOffset = PositionOffset;
            this.OnMeetingEnds = OnMeetingEnds;
            this.HasEffect = HasEffect;
            this.EffectDuration = EffectDuration;
            this.OnEffectEnds = OnEffectEnds;
            this.Sprite = Sprite;
            this.mirror = mirror;
            this.hotkey = hotkey;
            this.buttonText = buttonText;
            Timer = 16.2f;
            Buttons.Add(this);
            actionButton = Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
            actionButton.name = buttonText ?? "ModButton";
            actionButton.graphic.sprite = Sprite;
            PassiveButton button = actionButton.GetComponent<PassiveButton>();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)OnClickEvent);

            LocalScale = actionButton.transform.localScale;
            if (textTemplate)
            {
                Object.Destroy(actionButton.buttonLabelText);
                actionButton.buttonLabelText = Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
            }

            SetActive(false);
        }

        public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3? PositionOffset, HudManager hudManager, ActionButton? textTemplate, KeyCode? hotkey, bool mirror = false, string buttonText = null)
        : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, textTemplate, hotkey, false, 0f, () => { }, mirror, buttonText) { }

        private void OnClickEvent()
        {
            if ((this.Timer < 0f && HasButton() && CouldUse()) || (this.HasEffect && this.isEffectActive && this.effectCancellable))
            {
                actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
                this.OnClick();

                if (this.HasEffect && !this.isEffectActive)
                {
                    this.Timer = this.EffectDuration;
                    actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    this.isEffectActive = true;
                }
            }
        }

        public void ResetTimer() => Timer = MaxTimer;

        public static void HudUpdate()
        {
            Buttons.RemoveAll(item => item.actionButton == null);

            for (int i = 0; i < Buttons.Count; i++)
            {
                try
                {
                    Buttons[i].Update();
                }
                catch (NullReferenceException)
                {
                    Main.Logger.LogWarning("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void MeetingEndedUpdate()
        {
            Buttons.RemoveAll(item => item.actionButton == null);

            foreach (var button in Buttons)
            {
                try
                {
                    button.OnMeetingEnds();
                    button.Update();
                }
                catch (NullReferenceException)
                {
                    Main.Logger.LogWarning("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public static void ResetAllCooldowns()
        {
            foreach (var button in Buttons)
            {
                try
                {
                    button.ResetTimer();
                    button.Update();
                }
                catch (NullReferenceException)
                {
                    Main.Logger.LogWarning("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
                }
            }
        }

        public void SetActive(bool isActive)
        {
            actionButton.gameObject.SetActive(isActive);
            actionButton.graphic.enabled = isActive;
        }

        private void Update()
        {
            if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton())
            {
                SetActive(false);
                return;
            }
            SetActive(hudManager.UseButton.isActiveAndEnabled || hudManager.PetButton.isActiveAndEnabled);

            actionButton.graphic.sprite = Sprite;

            if (showButtonText && buttonText != null)
            {
                actionButton.OverrideText(buttonText);
            }
            actionButton.buttonLabelText.enabled = showButtonText; // Only show the text if it's a kill button

            if (hudManager.UseButton != null)
            {
                Vector3 pos = new(0, 0, 0);
                if (mirror) pos = new Vector3(-pos.x, pos.y, pos.z);
                if (PositionOffset.HasValue)
                    actionButton.transform.localPosition = pos + PositionOffset.Value;
                actionButton.transform.localScale = LocalScale;
            }

            if (CouldUse())
            {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
                actionButton.graphic.material.SetFloat("_Desat", 0f);
            }
            else
            {
                actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
                actionButton.graphic.material.SetFloat("_Desat", 1f);
            }

            if (Timer >= 0)
            {
                if (HasEffect && isEffectActive)
                    Timer -= Time.deltaTime;
                else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                    Timer -= Time.deltaTime;
            }

            if (Timer <= 0 && HasEffect && isEffectActive)
            {
                isEffectActive = false;
                actionButton.cooldownTimerText.color = Palette.EnabledColor;
                OnEffectEnds();
            }

            actionButton.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);

            // Trigger OnClickEvent if the hotkey is being pressed down
            if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) OnClickEvent();
        }
    }
}