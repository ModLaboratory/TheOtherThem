using System;
using System.Linq;
using TheOtherThem;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using TheOtherThem.Patches;

#if DEBUG
#pragma warning disable IDE1006 // 命名样式
#pragma warning disable CA1050 // 在命名空间中声明类型

public static class _dbg
{
    // Timer test
    public static void _tt()
    {
        var normallyStop = Helpers.Random();
        Main.Logger.LogInfo($"{nameof(normallyStop)} = {normallyStop}");
        var timer = new Timer(nameof(_tt), 3f,
            n => Main.Logger.LogInfo(n ? "Normal" : "Abnormal"),
            () => !normallyStop && Helpers.Random());
        timer.Start();
    }

    // Set role for local player
    public static void _sr4lp(RoleType type) => PlayerControl.LocalPlayer.SetRole(type);

    // Disable custom buttons usable check
    public static void _dbc() => CustomButton.Buttons.Where(b => b != null).Do(b => b.CouldUse = () => true);
}

#pragma warning restore CA1050 // 在命名空间中声明类型
#pragma warning restore IDE1006 // 命名样式
#endif