using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheOtherThem.Objects
{
    public static class ErrorNotification
    {
        public static ErrorNotificationManager ManagerInstance { get; private set; }
        public static TextMeshPro ErrorTextMesh { get; set; }

        private static readonly Dictionary<(int, string), float> _messages = new();

        public static int AddErrorString(string msg, float seconds = 10)
        {
            int id = -1;

            if (ManagerInstance.ProcessMessage(msg, seconds))
            {
                var random = new System.Random();

                do
                    id = random.Next();
                while (_messages.Keys.Select(pair => pair.Item1).Contains(id));

                _messages.Add((id, msg), seconds);
                ManagerInstance.UpdateString();
            }
            else
            {
                return _messages.FirstOrDefault(kvp => kvp.Key.Item2 == msg).Key.Item1;
            }

            return id;
        }

        public static void SetErrorString(int id, string msg, float seconds = 10)
        {
            RemoveErrorString(id);
            _messages.Add((id, msg), seconds);
        }

        public static bool RemoveErrorString(int id)
        {
            var pair = _messages.Keys.FirstOrDefault(pair => pair.Item1 == id);
            if (pair == default)
                return false;
            return _messages.Remove(pair);
        }

        public static bool HasError(int id) => _messages.Keys.Select(pair => pair.Item1).Any(i => i == id);

        public static void ClearErrorString() => _messages.Clear();

        public class ErrorNotificationManager : MonoBehaviour
        {
            public void Start()
            {
                ManagerInstance = this;

                name = nameof(ErrorNotificationManager);
                DontDestroyOnLoad(this);

                SceneManager.sceneLoaded += new Action<Scene, LoadSceneMode>((scene, _) =>
                {
                    ErrorTextMesh = new GameObject("ErrorNotification").AddComponent<TextMeshPro>();
                    ErrorTextMesh.gameObject.layer = LayerMask.NameToLayer("UI");

                    if (_messages.Any())
                        UpdateString();
                });

                Main.Logger.LogInfo(nameof(ErrorNotificationManager) + " initialized");
            }

            public void Update()
            {
                if (!ErrorTextMesh) return;

                if (Input.GetKeyDown(KeyCode.F3)) AddErrorString("test" + new System.Random().Next());

                var cam = HudManager.InstanceExists ? HudManager.Instance.PlayerCam.GetComponent<Camera>() : Camera.main;

                if (!ErrorTextMesh.transform.parent)
                    ErrorTextMesh.transform.SetParent(cam.transform);

                ErrorTextMesh.transform.localPosition = new(0, 0, -1000);
                ErrorTextMesh.color = Palette.ImpostorRed;
                ErrorTextMesh.alignment = TextAlignmentOptions.Top;
                ErrorTextMesh.fontSizeMax = 3f;
                ErrorTextMesh.fontSizeMin = 1.5f;
                ErrorTextMesh.fontSize = 2f;

                foreach (var (pair, _) in _messages)
                {
                    _messages[pair] -= Time.deltaTime;
                    if (_messages[pair] <= 0)
                    {
                        _messages.Remove(pair);
                        Main.Logger.LogInfo("removed");
                        UpdateString();
                    }
                }
            }

            public void UpdateString()
            {
                var text = string.Join('\n', _messages.Select(kvp => kvp.Key.Item2).ToArray().Reverse());

                ErrorTextMesh.text = text;
            }

            public bool ProcessMessage(string msg, float seconds)
            {
                if (!_messages.Any()) return true;

                if (_messages.Keys.Any(pair => pair.Item2 == msg))
                {
                    _messages[_messages.Keys.Last()] = seconds;
                    return false;
                }
                return true;
            }
        }

    }
}