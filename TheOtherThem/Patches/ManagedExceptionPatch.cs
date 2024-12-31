using Il2CppInterop.HarmonySupport;
using Il2CppInterop.Runtime;
using System;
using System.Reflection;
using TheOtherThem.Objects;

namespace TheOtherThem.Patches
{
    [HarmonyPatch]
    public static class ManagedExceptionPatch
    {
        public static Assembly Il2CppInteropHarmonySupport => typeof(HarmonySupport).Assembly;
        public static string Il2CppDetourMethodPatcherTypeName => "Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher";
        public static string ReportExceptionMethodName => "ReportException";
        public static BindingFlags PrivateStaticFlag => BindingFlags.NonPublic | BindingFlags.Static;
        public static MethodBase TargetMethod() => Il2CppInteropHarmonySupport.GetType(Il2CppDetourMethodPatcherTypeName).GetMethod(ReportExceptionMethodName, PrivateStaticFlag);

        public static void Postfix()
        {
            ErrorNotification.SetErrorString(ModTranslation.GetString("ManagedExceptinOccuredMsg"));
        }
    }
}