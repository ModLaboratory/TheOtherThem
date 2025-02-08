using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherThem.Objects
{

    public class CustomMessage
    {
        private TMPro.TMP_Text _text;
        public static List<CustomMessage> CustomMessages { get; } = new List<CustomMessage>();

        public CustomMessage(string message, float duration, string format = "", bool doFlash = true)
        {
            RoomTracker roomTracker = HudManager.Instance?.roomTracker;
            if (roomTracker != null)
            {
                GameObject gameObject = Object.Instantiate(roomTracker.gameObject);

                gameObject.transform.SetParent(HudManager.Instance.transform);
                Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                _text = gameObject.GetComponent<TMPro.TMP_Text>();

                // Use local position to place it in the player's view instead of the world location
                gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                CustomMessages.Add(this);

                var translated = ModTranslation.GetString(message);

                HudManager.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
                {
                    bool even = ((int)(p * duration / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                    if (!doFlash) even = true;
                    string prefix = even ? "<color=#FCBA03FF>" : "<color=#FF0000FF>";
                    string formatted = translated;
                    if (format != "") formatted = string.Format(formatted, format);
                    _text.text = prefix + formatted + "</color>";
                    if (_text != null) _text.color = even ? Color.yellow : Color.red;
                    if (p == 1f && _text != null && _text.gameObject != null)
                    {
                        Object.Destroy(_text.gameObject);
                        CustomMessages.Remove(this);
                    }
                })));
            }
        }

        public CustomMessage(Func<string> message, Func<bool> checkDestroy, bool doFlash = true)
        {
            RoomTracker roomTracker = HudManager.Instance?.roomTracker;
            if (roomTracker != null)
            {
                GameObject gameObject = Object.Instantiate(roomTracker.gameObject);

                gameObject.transform.SetParent(HudManager.Instance.transform);
                Object.DestroyImmediate(gameObject.GetComponent<RoomTracker>());
                _text = gameObject.GetComponent<TMPro.TMP_Text>();
                _text.text = message();

                // Use local position to place it in the player's view instead of the world location
                gameObject.transform.localPosition = new Vector3(0, -1.8f, gameObject.transform.localPosition.z);
                CustomMessages.Add(this);

                Il2CppSystem.Collections.IEnumerator coroutine = null;
                HudManager.Instance.StartCoroutine(coroutine = Effects.Lerp(10000, new Action<float>((p) =>
                {
                    if (!_text) HudManager.Instance.StopCoroutine(coroutine);
                    bool even = ((int)(p * 10000 / 0.25f)) % 2 == 0; // Bool flips every 0.25 seconds
                    if (!doFlash) even = true;
                    string prefix = even ? "<color=#FCBA03FF>" : "<color=#FF0000FF>";
                    _text.text = prefix + message() + "</color>";
                    if (_text != null) _text.color = even ? Color.yellow : Color.red;
                    bool destroy = false;
                    if ((p == 1f && _text != null && _text.gameObject != null) || (destroy = checkDestroy()))
                    {
                        if (destroy) HudManager.Instance.StopCoroutine(coroutine);
                        Object.Destroy(_text.gameObject);
                        CustomMessages.Remove(this);
                    }
                })));
            }
        }
    }
}