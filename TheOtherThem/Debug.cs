using System;
using System.Linq;
using TheOtherThem;
using TheOtherThem.Modules;
using TheOtherThem.Objects;

#if DEBUG
#pragma warning disable IDE1006
namespace _tot
{
    public static class _dbg
    {
        public static void _tt()
        {
            var normallyStop = Helpers.Random();
            Main.Logger.LogInfo($"{nameof(normallyStop)} = {normallyStop}");
            var timer = new Timer(nameof(_tt), 3f, 
                n => Main.Logger.LogInfo(n ? "Normal" : "Abnormal"), 
                () => normallyStop ? false : new Random().Next(2) == 1);
            timer.Start();
        }

        // Set role for local player
        public static void _sr4lp(RoleType type) => PlayerControl.LocalPlayer.SetRole(type);

        // Disable custom buttons usable check
        public static void _dbc() => CustomButton.Buttons.Where(b => b != null).Do(b => b.CouldUse = () => true);
    }
}
#pragma warning restore IDE1006
#endif