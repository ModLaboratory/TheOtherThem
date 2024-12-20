using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;
using UnityEngine;

namespace TheOtherThem.Modules;

public static class CoroutineUtils
{
    public static CustomCoroutine Instance { get; set; }
    public static void StartCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null) return;
        Instance.StartCoroutine(coroutine.WrapToIl2Cpp());
    }

    public static void StopCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null) return;
        Instance.StopCoroutine(coroutine.WrapToIl2Cpp());
    }

    public static void StopAllCoroutines() => Instance.StopAllCoroutines();
    
    public class CustomCoroutine : MonoBehaviour
    {
        void Start()
        {
            Instance = this;

            name = nameof(CustomCoroutine);
            Main.Logger.LogInfo(nameof(CustomCoroutine) + " initialized");
        }
    }
}