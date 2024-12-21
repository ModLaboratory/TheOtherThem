using TheOtherThem.Modules;
using TMPro;
using UnityEngine;

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
            public void Start()
            {
                ManagerInstance = this;

                name = nameof(ErrorNotificationManager);
                DontDestroyOnLoad(this);

                Main.Logger.LogInfo(nameof(ErrorNotificationManager) + " initialized");
            }
            
            public void LateUpdate()
            {
                if (!ErrorTextMesh)
                {
                    if (LastOne == "")
                    {
                        return;
                    }
                    else
                    {
                        CreateOrOverride(LastOne);
                    }
                }

                var cam = HudManager.InstanceExists ? HudManager.Instance.PlayerCam.GetComponent<Camera>() : Camera.main;

                var pos = cam.transform.position;
                ErrorTextMesh.transform.position = AspectPosition.ComputeWorldPosition(cam, AspectPosition.EdgeAlignments.Top, new(0,0.3f,-1000));
                ErrorTextMesh.transform.SetWorldZ(-100);
                ErrorTextMesh.color = Palette.ImpostorRed;
                ErrorTextMesh.alignment = TextAlignmentOptions.Top;
                ErrorTextMesh.fontSizeMax = 3;
                ErrorTextMesh.fontSizeMin = 3;
                ErrorTextMesh.fontSize = 3;
            }

            public void CreateOrOverride(string text)
            {
                if (!ErrorTextMesh)
                {
                    ErrorTextMesh = new GameObject(text).AddComponent<TextMeshPro>();
                    ErrorTextMesh.text = text;
                    DontDestroyOnLoad(ErrorTextMesh);
                }
                else
                {
                    ErrorTextMesh.text = text;
                }
                LastOne = text;
            }
        }

    }
}