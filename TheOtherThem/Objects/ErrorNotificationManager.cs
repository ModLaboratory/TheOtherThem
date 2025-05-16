using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheOtherThem.Objects
{
    public static class ErrorNotification
    {
        public static ErrorNotificationManager ManagerInstance { get; private set; }
        public static TextMeshPro ErrorTextMesh { get; set; }
        public static string LastOne { get; set; } = "";

        public static void SetErrorString(string info) => ManagerInstance.CreateOrOverride(info);
        public static void ClearErrorString() => SetErrorString("");

        public class ErrorNotificationManager : MonoBehaviour
        {
            private float _timer = float.MinValue;
            private float _countdown = 10f;

            public void Start()
            {
                ManagerInstance = this;

                name = nameof(ErrorNotificationManager);
                DontDestroyOnLoad(this);

                SceneManager.sceneLoaded += new Action<Scene, LoadSceneMode>((scene, _) =>
                {
                    if (LastOne != "")
                        CreateOrOverride(LastOne);
                });

                Main.Logger.LogInfo(nameof(ErrorNotificationManager) + " initialized");
            }
            
            public void Update()
            {
                if (!ErrorTextMesh) return;
                //if (Input.GetKeyDown(KeyCode.F3)) CreateOrOverride("This is a test");
                if (_timer <= 0) ClearErrorString();

                var cam = HudManager.InstanceExists ? HudManager.Instance.PlayerCam.GetComponent<Camera>() : Camera.main;

                if (!ErrorTextMesh.transform.parent)
                    ErrorTextMesh.transform.SetParent(cam.transform);

                ErrorTextMesh.transform.localPosition = new(0, 0, -1000);
                ErrorTextMesh.color = Palette.ImpostorRed;
                ErrorTextMesh.alignment = TextAlignmentOptions.Top;
                ErrorTextMesh.fontSizeMax = 3f;
                ErrorTextMesh.fontSizeMin = 1.5f;
                ErrorTextMesh.fontSize = 2f;

                if (_timer > 0) _timer -= Time.deltaTime;
            }

            public void CreateOrOverride(string text)
            {
                if (text != "")
                {
                    Main.Logger.LogMessage($"{text} attempted to be displayed...");
                    _timer = _countdown;
                    
                    if (!ErrorTextMesh)
                    {
                        ErrorTextMesh = new GameObject("ErrorNotification").AddComponent<TextMeshPro>();
                        ErrorTextMesh.gameObject.layer = LayerMask.NameToLayer("UI");
                    }
                }

                if (ErrorTextMesh)
                    ErrorTextMesh.text = text;
                LastOne = text;
            }
        }

    }
}