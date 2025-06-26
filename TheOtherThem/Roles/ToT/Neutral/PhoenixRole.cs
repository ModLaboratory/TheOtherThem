using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem.ToTRole.Neutral
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class PhoenixRole : CustomRole
    {
        public static PhoenixRole Instance { get; private set; }

        public static CustomRoleOption PhoenixRoleSpawnRate { get; set; }
        public static CustomOption WinningKillingPlayerCount { get; set; }
        public static CustomOption PossessionToReviveMaxTime { get; set; }
        public static CustomOption HasArrowToDeadBodyForPossession { get; set; }
        public static CustomButton PossessButton { get; set; }

        private static int _realWinningKillCount = int.MaxValue;
        private static int _currentKilledCount = 0;
        private static int _revivalTimes = 0;
        private static int _deadTimes = 0;
        private static bool _canRevive = true;
        private static bool _killedAfterThisRevival = false;
        private static DeadBody _currentTarget = null;
        private static Dictionary<DeadBody, Arrow> _arrows = new();

        PhoenixRole() : base("Phoenix", Palette.Orange,
           (nameKey, roleColor) => PhoenixRoleSpawnRate = new(2300, nameKey, roleColor, TeamTypeToT.Neutral, 1),
           RoleType.Phoenix, TeamTypeToT.Neutral, true)
        {
            Instance = this;

            IsKillableNonImpostor = true;

            WinningKillingPlayerCount = CustomOption.CreateInsertable(2301, "PhoenixWinningKillCount", 3, 1, 5, 1, TeamTypeToT.Neutral, PhoenixRoleSpawnRate);
            PossessionToReviveMaxTime = CustomOption.CreateInsertable(2302, "PhoenixPossessionRevivalMaxTime", 2, 1, 3, 1, TeamTypeToT.Neutral, PhoenixRoleSpawnRate);
            HasArrowToDeadBodyForPossession = CustomOption.CreateInsertable(2303, "PhoenixHasArrowToDeadBodyForPossession", true, TeamTypeToT.Neutral, PhoenixRoleSpawnRate);
        }
        
        public void AdjustWinningKillingCount() // make game a bit easier
        {
            int playerCount = PlayerControl.AllPlayerControls.Count;
            int maxPlayerToKill = (int)(playerCount * (1f / 3f));
            int checkedMaxPlayerToKill = Mathf.Clamp(maxPlayerToKill, 1, maxPlayerToKill); // 0 check

            if (_realWinningKillCount > checkedMaxPlayerToKill) // e.g. 5 to kill - 10 alive: too difficult
                _realWinningKillCount = checkedMaxPlayerToKill; // Also every client SHOULD HAVE THE SAME _realWinningKillCount value without RPC sync
        }

        public static DeadBody GetClosestBody(List<DeadBody> untargettable = null)
        {
            DeadBody result = null;

            var num = PlayerControl.LocalPlayer.MaxReportDistance;
            if (!ShipStatus.Instance) return null;
            var position = PlayerControl.LocalPlayer.GetTruePosition();

            foreach (var body in Object.FindObjectsOfType<DeadBody>()
                         .Where(b => untargettable?.Contains(b) ?? true))
            {
                var vector = body.TruePosition - position;
                var magnitude = vector.magnitude;
                if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(position, vector.normalized,
                        magnitude, Constants.ShipAndObjectsMask))
                {
                    result = body;
                    num = magnitude;
                }
            }

            return result;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        [HarmonyPostfix]
        public static void OnGameUpdate(PlayerControl __instance)
        {
            // Killing count adjustment
            if (Instance.Players.Any() && !Instance.Players.First().IsDead)
                Instance.AdjustWinningKillingCount();

            if (Instance.IsLocalPlayerRole() && __instance.AmOwner && __instance.IsDead() && _canRevive)
            {
                var bodies = Object.FindObjectsOfType<DeadBody>().Where(db => !db.Reported); // Ensure hasnt been possessed
                foreach (var body in bodies)
                {
                    var exists = _arrows.TryGetValue(body, out var arrow);
                    if (exists)
                        arrow.Update();
                    else
                        arrow = _arrows[body] = new(Instance.RoleColor);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
        [HarmonyPostfix]
        public static void OnDeath(PlayerControl __instance, [HarmonyArgument(0)] DeathReason reason)
        {
            if (!__instance.AmOwner) return;
            if (!Instance.IsLocalPlayerRole()) return; // Just local Phoenix does this

            _killedAfterThisRevival = false;

            if (reason == DeathReason.Kill)
            {
                if (++_deadTimes > PossessionToReviveMaxTime.GetFloat())
                    _canRevive = false;
            }
            else
            {
                _canRevive = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        [HarmonyPostfix]
        public static void OnKillingOthers(PlayerControl __instance)
        {
            if (!__instance.IsRole(RoleType.Phoenix)) return; // Isnt Phoenix

            if (_deadTimes != 0) 
                _killedAfterThisRevival = true;

            _currentKilledCount++; // Every client SHOULD HAVE the SAME _currentKilledCount value without RPC sync since MurderPlayer executes for every client
        }

        public void RpcPossess(DeadBody target)
        {
            new RpcWriter(CustomRpc.PhoenixPossess).WritePacked(target.ParentId).Finish();
            Possess(target.ParentId);
        }

        public void Possess(byte target)
        {
            var body = Object.FindObjectsOfType<DeadBody>().First(b => b.ParentId == target);
            body.Reported = true; // Set reported as possessed
            body.gameObject.SetActive(false);

            PlayerControl.LocalPlayer.Revive();
            PlayerControl.LocalPlayer.Shapeshift(Helpers.PlayerById(body.ParentId), false);

            _arrows.Values.Do(a => a.Destroy());
            _arrows.Clear();
        }


        public override string GetRoleTaskHintText() => string.Format(base.GetRoleTaskHintText(), _currentKilledCount, _realWinningKillCount);
        
        public override bool ShouldShowKillButton() => !_killedAfterThisRevival;

        public override void OnRpcReceived(byte callId, MessageReader reader)
        {
            if ((CustomRpc)callId == CustomRpc.PhoenixPossess)
                Possess((byte)reader.ReadPackedInt32());
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            PossessButton = new(
                () => RpcPossess(_currentTarget),
                () => CanLocalPlayerUse() && PlayerControl.LocalPlayer.IsDead() && _canRevive,
                () =>
                {
                    var mat = _currentTarget.bodyRenderers.First().material;
                    if (_currentTarget)
                        mat.SetFloat("_Outline", 0);
                    
                    _currentTarget = GetClosestBody();
                    if (_currentTarget)
                    {
                        mat.SetFloat("_Outline", 1);
                        mat.SetColor("_OutlineColor", RoleColor);
                    }
                    return _currentTarget;
                },
                () => { },
                ModTranslation.GetImage("PossessButton", 230),
                null,
                HudManager.Instance,
                HudManager.Instance.KillButton,
                KeyCode.P,
                buttonText: ModTranslation.GetString("PhoenixAbilityLabel")
            );

            return new[]
            {
                (PossessButton, 0f)
            };
        }

        public override bool CanWin(ShipStatus ship)
        {
            if (!Players.Any()) return false;
            if (Players.FirstOrDefault().IsDead) return false;

            if (_currentKilledCount >= _realWinningKillCount) 
                return true;

            return false;
        }

        public override void ClearData()
        {
            _realWinningKillCount = (int)WinningKillingPlayerCount.GetFloat();
            _currentKilledCount = 0;
            _revivalTimes = 0;
            _deadTimes = 0;
            _canRevive = true;
            _killedAfterThisRevival = false;
            _currentTarget = null;
            _arrows.Clear();
        }
    }
}
