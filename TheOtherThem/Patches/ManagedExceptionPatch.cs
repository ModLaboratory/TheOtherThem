using Il2CppInterop.Runtime;
using System;

namespace TheOtherThem.Patches
{
    [HarmonyPatch(typeof(Il2CppException), nameof(Il2CppException.RaiseExceptionIfNecessary))]
    public static class ManagedExceptionPatch
    {
        public static void Postfix([HarmonyArgument(0)] IntPtr returnedException)
        {
            if (returnedException == IntPtr.Zero) return;
            
        }
    }
}