using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using UnityEngine;

namespace TheOtherThem.Roles.ToT.Impostor
{
    [RoleAutoInitialize]
    [HarmonyPatch]
    public class SorcererRole : CustomRole
    {
        public static SorcererRole Instance { get; private set; }

        public static CustomRoleOption SorcererSpawnRate { get; set; }
        public static CustomOption SorcererCurseCooldownOption { get; set; }

        private bool _usedThisRound = false;
        private static DeadBody _body;
        private static readonly List<DeadBody> _cursed = new();

        SorcererRole() : base("Sorcerer", Palette.ImpostorRed,
           (nameKey, roleColor) => SorcererSpawnRate = new(GetAvailableOptionId(), nameKey, roleColor, TeamTypeToT.Impostor),
           RoleType.Sorcerer, TeamTypeToT.Impostor, false)
        {
            Instance = this;

            SorcererCurseCooldownOption = CustomOption.CreateInsertable(GetNextAvailableOptionId(), "SorcererCurseCooldownOption", 20, 10, 60, 5, TeamTypeToT.Impostor, SorcererSpawnRate);
        }

        public override (CustomButton, float)[] CreateButtons()
        {
            var curseButton = new CustomButton(
                () =>
                {
                    _cursed.Add(_body);
                    var writer = new RpcWriter(CustomRpc.CurseDeadBody);
                    writer.WritePacked(_body.ParentId).Finish();
                    _usedThisRound = true;
                },
                CanLocalPlayerUse,
                () =>
                {
                    if (_body)
                        _body.ClearOutline();

                    _body = Helpers.GetClosestBody();

                    if (_body)
                        _body.SetOutline(RoleColor);

                    return !_usedThisRound && _body && !_cursed.Contains(_body);
                },
                () =>
                {
                    _usedThisRound = false;
                    _cursed.Clear();
                },
                null,
                null,
                HudManager.Instance,
                HudManager.Instance.KillButton,
                KeyCode.C,
                buttonText: ModTranslation.GetString("SorcererCurseLabel"));

            return new[]
            {
                (curseButton, SorcererCurseCooldownOption.GetFloat())
            };
        }

        public override void OnRpcReceive(byte callId, MessageReader reader)
        {
            if (callId == (byte)CustomRpc.CurseDeadBody)
                _cursed.Add(Object.FindObjectsOfType<DeadBody>().First(db => db.ParentId == reader.ReadPackedInt32()));
        }

        public override void ClearData()
        {
            _usedThisRound = false;
            _body = null;
            _cursed.Clear();
        }

        [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
        [HarmonyPrefix]
        static void ReportBodyPatch(DeadBody __instance)
        {
            if (!__instance.Reported && GameManager.Instance.CanReportBodies())
            {
                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                Vector2 truePosition2 = __instance.TruePosition;
                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, useTriggers: false))
                {
                    if (_cursed.Contains(__instance))
                    {
                        var writer = new RpcWriter(CustomRpc.UncheckedMurderPlayer);

                        var local = PlayerControl.LocalPlayer.PlayerId;

                        writer.Write(local)
                            .Write(local)
                            .Write((byte)1)
                            .Finish();

                        RpcProcedure.UncheckedMurderPlayer(local, local, 1);
                    }
                }
            }
        }
    }
}