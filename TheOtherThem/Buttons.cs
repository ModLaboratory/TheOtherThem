using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using TheOtherThem.Modules;
using TheOtherThem.Objects;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.ToTRole;
using Iced.Intel;

namespace TheOtherThem
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
    static class GameStartPatch
    {
        static void Prefix()
        {
            CustomButton.ResetAllCooldowns();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
        public static CustomButton engineerRepairButton;
        public static CustomButton janitorCleanButton;
        public static CustomButton timeMasterShieldButton;
        public static CustomButton medicShieldButton;
        public static CustomButton shifterShiftButton;
        public static CustomButton morphlingButton;
        public static CustomButton camouflagerButton;
        public static CustomButton hackerButton;
        public static CustomButton hackerVitalsButton;
        public static CustomButton hackerAdminTableButton;
        public static CustomButton trackerTrackPlayerButton;
        public static CustomButton trackerTrackCorpsesButton;
        public static CustomButton vampireKillButton;
        public static CustomButton garlicButton;
        public static CustomButton jackalKillButton;
        public static CustomButton sidekickKillButton;
        public static CustomButton jackalSidekickButton;
        public static CustomButton eraserButton;
        public static CustomButton placeJackInTheBoxButton;
        public static CustomButton lightsOutButton;
        //public static CustomButton showInfoOverlay;
        public static CustomButton cleanerCleanButton;
        public static CustomButton warlockCurseButton;
        public static CustomButton securityGuardButton;
        public static CustomButton securityGuardCamButton;
        public static CustomButton arsonistButton;
        public static CustomButton arsonistIgniteButton;
        public static CustomButton vultureEatButton;
        public static CustomButton mediumButton;
        public static CustomButton pursuerButton;
        public static CustomButton witchSpellButton;

        public static TMPro.TMP_Text vultureNumCorpsesText;
        public static TMPro.TMP_Text securityGuardButtonScrewsText;
        public static TMPro.TMP_Text securityGuardChargesText;
        public static TMPro.TMP_Text pursuerButtonBlanksText;
        public static TMPro.TMP_Text hackerAdminTableChargesText;
        public static TMPro.TMP_Text hackerVitalsChargesText;

        public static void setCustomButtonCooldowns()
        {
            engineerRepairButton.MaxTimer = 0f;
            janitorCleanButton.MaxTimer = Janitor.cooldown;
            timeMasterShieldButton.MaxTimer = TimeMaster.cooldown;
            medicShieldButton.MaxTimer = 0f;
            shifterShiftButton.MaxTimer = 0f;
            morphlingButton.MaxTimer = Morphling.cooldown;
            camouflagerButton.MaxTimer = Camouflager.cooldown;
            hackerButton.MaxTimer = Hacker.cooldown;
            hackerVitalsButton.MaxTimer = Hacker.cooldown;
            hackerAdminTableButton.MaxTimer = Hacker.cooldown;
            vampireKillButton.MaxTimer = Vampire.cooldown;
            trackerTrackPlayerButton.MaxTimer = 0f;
            garlicButton.MaxTimer = 0f;
            jackalKillButton.MaxTimer = Jackal.cooldown;
            sidekickKillButton.MaxTimer = Sidekick.cooldown;
            jackalSidekickButton.MaxTimer = Jackal.createSidekickCooldown;
            eraserButton.MaxTimer = Eraser.cooldown;
            placeJackInTheBoxButton.MaxTimer = Trickster.placeBoxCooldown;
            lightsOutButton.MaxTimer = Trickster.lightsOutCooldown;
            cleanerCleanButton.MaxTimer = Cleaner.cooldown;
            warlockCurseButton.MaxTimer = Warlock.cooldown;
            securityGuardButton.MaxTimer = SecurityGuard.cooldown;
            securityGuardCamButton.MaxTimer = SecurityGuard.cooldown;
            arsonistButton.MaxTimer = Arsonist.cooldown;
            vultureEatButton.MaxTimer = Vulture.cooldown;
            mediumButton.MaxTimer = Medium.cooldown;
            pursuerButton.MaxTimer = Pursuer.cooldown;
            trackerTrackCorpsesButton.MaxTimer = Tracker.corpsesTrackingCooldown;
            witchSpellButton.MaxTimer = Witch.cooldown;

            timeMasterShieldButton.EffectDuration = TimeMaster.shieldDuration;
            hackerButton.EffectDuration = Hacker.duration;
            hackerVitalsButton.EffectDuration = Hacker.duration;
            hackerAdminTableButton.EffectDuration = Hacker.duration;
            vampireKillButton.EffectDuration = Vampire.delay;
            camouflagerButton.EffectDuration = Camouflager.duration;
            morphlingButton.EffectDuration = Morphling.duration;
            lightsOutButton.EffectDuration = Trickster.lightsOutDuration;
            arsonistButton.EffectDuration = Arsonist.duration;
            mediumButton.EffectDuration = Medium.duration;
            trackerTrackCorpsesButton.EffectDuration = Tracker.corpsesTrackingDuration;
            witchSpellButton.EffectDuration = Witch.spellCastingDuration;
            securityGuardCamButton.EffectDuration = SecurityGuard.duration;
            // Already set the timer to the max, as the button is enabled during the game and not available at the start
            lightsOutButton.Timer = lightsOutButton.MaxTimer;

            arsonistIgniteButton.MaxTimer = 0f;
            arsonistIgniteButton.Timer = 0f;

            ButtonsGM.setCustomButtonCooldowns();
            ToTButtons.ForEach(tuple =>
            {
                if (tuple.button == null) return;
                tuple.button.MaxTimer = tuple.cooldown;
            });
        }

        public static void resetTimeMasterButton()
        {
            timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
            timeMasterShieldButton.isEffectActive = false;
            timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }

        public static List<(CustomButton button, float cooldown)> ToTButtons { get; private set; } = new();

        public static void Postfix(HudManager __instance)
        {
            // TOT Buttons
            ToTButtons = new(CustomRole.AllRoles.Select(cr => cr.CreateButtons()).SelectMany(c => c));

            // TODO: SUPPORT THE MUSHROOM MIX UP SABOTAGE
            // Engineer Repair
            engineerRepairButton = new CustomButton(
                () =>
                {
                    engineerRepairButton.Timer = 0f;

                    MessageWriter usedRepairWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.EngineerUsedRepair, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(usedRepairWriter);
                    RpcProcedure.EngineerUsedRepair();

                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.EngineerFixLights, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RpcProcedure.EngineerFixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 0 | 64);
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 0);
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 0 | 16);
                            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                },
                () => { return Engineer.engineer != null && Engineer.engineer == PlayerControl.LocalPlayer && Engineer.remainingFixes > 0 && PlayerControl.LocalPlayer.IsAlive(); },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                            sabotageActive = true;
                    return sabotageActive && Engineer.remainingFixes > 0 && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                Engineer.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F
            );
            engineerRepairButton.buttonText = ModTranslation.GetString("RepairText");

            // Janitor Clean
            janitorCleanButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RpcProcedure.CleanBody(playerInfo.PlayerId);
                                    janitorCleanButton.Timer = janitorCleanButton.MaxTimer;

                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return Janitor.janitor != null && Janitor.janitor == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { janitorCleanButton.Timer = janitorCleanButton.MaxTimer; },
                Janitor.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            janitorCleanButton.buttonText = ModTranslation.GetString("CleanText");


            // Time Master Rewind Time
            timeMasterShieldButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.TimeMasterShield, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.timeMasterShield();
                },
                () => { return TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
                    timeMasterShieldButton.isEffectActive = false;
                    timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                TimeMaster.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F,
                true,
                TimeMaster.shieldDuration,
                () => { timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer; }
            );
            timeMasterShieldButton.buttonText = ModTranslation.GetString("TimeShieldText");

            // Medic Shield
            medicShieldButton = new CustomButton(
                () =>
                {
                    medicShieldButton.Timer = 0f;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, Medic.setShieldAfterMeeting ? (byte)CustomRpc.SetFutureShielded : (byte)CustomRpc.MedicSetShielded, Hazel.SendOption.Reliable, -1);
                    writer.Write(Medic.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (Medic.setShieldAfterMeeting)
                        RpcProcedure.setFutureShielded(Medic.currentTarget.PlayerId);
                    else
                        RpcProcedure.medicSetShielded(Medic.currentTarget.PlayerId);
                },
                () => { return Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return !Medic.usedShield && Medic.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { },
                Medic.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F
            );
            medicShieldButton.buttonText = ModTranslation.GetString("ShieldText");


            // Shifter shift
            shifterShiftButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetFutureShifted, Hazel.SendOption.Reliable, -1);
                    writer.Write(Shifter.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.setFutureShifted(Shifter.currentTarget.PlayerId);
                },
                () => { return Shifter.shifter != null && Shifter.shifter == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return Shifter.currentTarget && Shifter.futureShift == null && PlayerControl.LocalPlayer.CanMove; },
                () => { },
                Shifter.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F
            );
            shifterShiftButton.buttonText = ModTranslation.GetString("ShiftText");

            // Morphling morph
            morphlingButton = new CustomButton(
                () =>
                {
                    if (Morphling.sampledTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.MorphlingMorph, Hazel.SendOption.Reliable, -1);
                        writer.Write(Morphling.sampledTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.morphlingMorph(Morphling.sampledTarget.PlayerId);
                        Morphling.sampledTarget = null;
                        morphlingButton.EffectDuration = Morphling.duration;
                    }
                    else if (Morphling.currentTarget != null)
                    {
                        Morphling.sampledTarget = Morphling.currentTarget;
                        morphlingButton.Sprite = Morphling.getMorphSprite();
                        morphlingButton.buttonText = ModTranslation.GetString("MorphText");
                        morphlingButton.EffectDuration = 1f;
                    }
                },
                () => { return Morphling.morphling != null && Morphling.morphling == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return (Morphling.currentTarget || Morphling.sampledTarget) && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    morphlingButton.Timer = morphlingButton.MaxTimer;
                    morphlingButton.Sprite = Morphling.getSampleSprite();
                    morphlingButton.buttonText = ModTranslation.GetString("SampleText");
                    morphlingButton.isEffectActive = false;
                    morphlingButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    Morphling.sampledTarget = null;
                },
                Morphling.getSampleSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F,
                true,
                Morphling.duration,
                () =>
                {
                    if (Morphling.sampledTarget == null)
                    {
                        morphlingButton.Timer = morphlingButton.MaxTimer;
                        morphlingButton.Sprite = Morphling.getSampleSprite();
                        morphlingButton.buttonText = ModTranslation.GetString("SampleText");
                    }
                }
            );
            morphlingButton.buttonText = ModTranslation.GetString("SampleText");

            // Camouflager camouflage
            camouflagerButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.CamouflagerCamouflage, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.camouflagerCamouflage();
                },
                () => { return Camouflager.camouflager != null && Camouflager.camouflager == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    camouflagerButton.Timer = camouflagerButton.MaxTimer;
                    camouflagerButton.isEffectActive = false;
                    camouflagerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Camouflager.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F,
                true,
                Camouflager.duration,
                () => { camouflagerButton.Timer = camouflagerButton.MaxTimer; }
            );
            camouflagerButton.buttonText = ModTranslation.GetString("CamoText");

            // Hacker button
            hackerButton = new CustomButton(
                () =>
                {
                    Hacker.hackerTimer = Hacker.duration;
                },
                () => { return Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return true; },
                () =>
                {
                    hackerButton.Timer = hackerButton.MaxTimer;
                    hackerButton.isEffectActive = false;
                    hackerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Hacker.getButtonSprite(),
                new Vector3(0f, 1f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F,
                true,
                0f,
                () => { hackerButton.Timer = hackerButton.MaxTimer; }
            );
            hackerButton.buttonText = ModTranslation.GetString("HackerText");

            hackerAdminTableButton = new CustomButton(
               () =>
               {
                   if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                       DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new() { Mode = global::MapOptions.Modes.CountOverlay });

                   if (Hacker.cantMove) PlayerControl.LocalPlayer.moveable = false;
                   PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                   Hacker.chargesAdminTable--;
               },
               () => { return Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && MapOptions.couldUseAdmin && PlayerControl.LocalPlayer.IsAlive(); },
               () =>
               {
                   if (hackerAdminTableChargesText != null)
                       hackerAdminTableChargesText.text = hackerVitalsChargesText.text = String.Format(ModTranslation.GetString("hackerChargesText"), Hacker.chargesAdminTable, Hacker.toolsNumber);
                   return Hacker.chargesAdminTable > 0 && MapOptions.canUseAdmin;; 
               },
               () =>
               {
                   hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                   hackerAdminTableButton.isEffectActive = false;
                   hackerAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getAdminSprite(),
               new Vector3(-1.8f, -0.06f, 0),
               __instance,
               __instance.UseButton,
               KeyCode.Q,
               true,
               0f,
               () =>
               {
                   hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                   if (!hackerVitalsButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                   if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
               },
               GameManager.Instance.LogicOptions.MapId == 3,
               DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin)
            );

            // Hacker Admin Table Charges
            hackerAdminTableChargesText = GameObject.Instantiate(hackerAdminTableButton.actionButton.cooldownTimerText, hackerAdminTableButton.actionButton.cooldownTimerText.transform.parent);
            hackerAdminTableChargesText.text = "";
            hackerAdminTableChargesText.enableWordWrapping = false;
            hackerAdminTableChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerAdminTableChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            hackerVitalsButton = new CustomButton(
               () =>
               {
                   if (GameManager.Instance.LogicOptions.MapId != 1)
                   {
                       if (Hacker.vitals == null)
                       {
                           var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
                           if (e == null || Camera.main == null) return;
                           Hacker.vitals = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                       }
                       Hacker.vitals.transform.SetParent(Camera.main.transform, false);
                       Hacker.vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                       Hacker.vitals.Begin(null);
                   }
                   else
                   {
                       if (Hacker.doorLog == null)
                       {
                           var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                           if (e == null || Camera.main == null) return;
                           Hacker.doorLog = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                       }
                       Hacker.doorLog.transform.SetParent(Camera.main.transform, false);
                       Hacker.doorLog.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                       Hacker.doorLog.Begin(null);
                   }

                   if (Hacker.cantMove) PlayerControl.LocalPlayer.moveable = false;
                   PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 

                   Hacker.chargesVitals--;
               },
               () => { return Hacker.hacker != null && Hacker.hacker == PlayerControl.LocalPlayer && MapOptions.couldUseVitals && PlayerControl.LocalPlayer.IsAlive() && GameManager.Instance.LogicOptions.MapId != 0 && GameManager.Instance.LogicOptions.MapId != 3; },
               () =>
               {
                   if (hackerVitalsChargesText != null)
                       hackerVitalsChargesText.text = String.Format(ModTranslation.GetString("hackerChargesText"), Hacker.chargesVitals, Hacker.toolsNumber);
                   hackerVitalsButton.actionButton.graphic.sprite = GameManager.Instance.LogicOptions.MapId == 1 ? Hacker.getLogSprite() : Hacker.getVitalsSprite();
                   hackerVitalsButton.actionButton.OverrideText(GameManager.Instance.LogicOptions.MapId == 1 ?
                        TranslationController.Instance.GetString(StringNames.DoorlogLabel) :
                        TranslationController.Instance.GetString(StringNames.VitalsLabel));
                   return Hacker.chargesVitals > 0 && MapOptions.canUseVitals;
               },
               () =>
               {
                   hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                   hackerVitalsButton.isEffectActive = false;
                   hackerVitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
               },
               Hacker.getVitalsSprite(),
               new Vector3(-2.7f, -0.06f, 0),
               __instance,
               __instance.UseButton,
               KeyCode.Q,
               true,
               0f,
               () =>
               {
                   hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                   if(!hackerAdminTableButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                   if (Minigame.Instance)
                   {
                       if (GameManager.Instance.LogicOptions.MapId == 1) Hacker.doorLog.ForceClose();
                       else Hacker.vitals.ForceClose();
                   }
               },
               false,
               GameManager.Instance.LogicOptions.MapId == 1 ?
                    TranslationController.Instance.GetString(StringNames.DoorlogLabel) :
                    TranslationController.Instance.GetString(StringNames.VitalsLabel)
            );

            // Hacker Vitals Charges
            hackerVitalsChargesText = GameObject.Instantiate(hackerVitalsButton.actionButton.cooldownTimerText, hackerVitalsButton.actionButton.cooldownTimerText.transform.parent);
            hackerVitalsChargesText.text = "";
            hackerVitalsChargesText.enableWordWrapping = false;
            hackerVitalsChargesText.transform.localScale = Vector3.one * 0.5f;
            hackerVitalsChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Tracker button
            trackerTrackPlayerButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.TrackerUsedTracker, Hazel.SendOption.Reliable, -1);
                    writer.Write(Tracker.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.trackerUsedTracker(Tracker.currentTarget.PlayerId);
                },
                () => { return Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove && Tracker.currentTarget != null && !Tracker.usedTracker; },
                () => { if (Tracker.resetTargetAfterMeeting) Tracker.resetTracked(); },
                Tracker.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F
            );
            trackerTrackPlayerButton.buttonText = ModTranslation.GetString("TrackerText");

            trackerTrackCorpsesButton = new CustomButton(
                () => { Tracker.corpsesTrackingTimer = Tracker.corpsesTrackingDuration; },
                () => { return Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive() && Tracker.canTrackCorpses; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                    trackerTrackCorpsesButton.isEffectActive = false;
                    trackerTrackCorpsesButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Tracker.getTrackCorpsesButtonSprite(),
                new Vector3(-2.7f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.Q,
                true,
                Tracker.corpsesTrackingDuration,
                () =>
                {
                    trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                }
            );
            trackerTrackCorpsesButton.buttonText = ModTranslation.GetString("PathfindText");

            vampireKillButton = new CustomButton(
                () =>
                {
                    MurderAttemptResult murder = Helpers.CheckMuderAttempt(Vampire.vampire, Vampire.currentTarget);
                    if (murder == MurderAttemptResult.PerformKill)
                    {
                        if (Vampire.targetNearGarlic)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
                            writer.Write(Vampire.vampire.PlayerId);
                            writer.Write(Vampire.currentTarget.PlayerId);
                            writer.Write(Byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RpcProcedure.UncheckedMurderPlayer(Vampire.vampire.PlayerId, Vampire.currentTarget.PlayerId, Byte.MaxValue);

                            vampireKillButton.HasEffect = false; // Block effect on this click
                            vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        }
                        else
                        {
                            Vampire.bitten = Vampire.currentTarget;
                            // Notify players about bitten
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.VampireSetBitten, Hazel.SendOption.Reliable, -1);
                            writer.Write(Vampire.bitten.PlayerId);
                            writer.Write((byte)0);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RpcProcedure.vampireSetBitten(Vampire.bitten.PlayerId, 0);

                            HudManager.Instance.StartCoroutine(Effects.Lerp(Vampire.delay, new Action<float>((p) =>
                            { // Delayed action
                                if (p == 1f)
                                {
                                    // Perform kill if possible and reset bitten (regardless whether the kill was successful or not)
                                    Helpers.checkMuderAttemptAndKill(Vampire.vampire, Vampire.bitten, showAnimation: false);
                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.VampireSetBitten, Hazel.SendOption.Reliable, -1);
                                    writer.Write(byte.MaxValue);
                                    writer.Write(byte.MaxValue);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RpcProcedure.vampireSetBitten(byte.MaxValue, byte.MaxValue);
                                }
                            })));

                            vampireKillButton.HasEffect = true; // Trigger effect on this click
                        }
                    }
                    else if (murder == MurderAttemptResult.BlankKill)
                    {
                        vampireKillButton.Timer = vampireKillButton.MaxTimer;
                        vampireKillButton.HasEffect = false;
                    }
                },
                () => { return Vampire.vampire != null && Vampire.vampire == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () =>
                {
                    if (Vampire.targetNearGarlic && Vampire.canKillNearGarlics)
                    {
                        vampireKillButton.Sprite = __instance.KillButton.graphic.sprite;
                        vampireKillButton.buttonText = TranslationController.Instance.GetString(StringNames.KillLabel);
                    }
                    else
                    {
                        vampireKillButton.Sprite = Vampire.getButtonSprite();
                        vampireKillButton.buttonText = ModTranslation.GetString("VampireText");
                    }
                    return Vampire.currentTarget != null && PlayerControl.LocalPlayer.CanMove && (!Vampire.targetNearGarlic || Vampire.canKillNearGarlics);
                },
                () =>
                {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                    vampireKillButton.isEffectActive = false;
                    vampireKillButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                Vampire.getButtonSprite(),
                new Vector3(0, 1f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q,
                false,
                0f,
                () =>
                {
                    vampireKillButton.Timer = vampireKillButton.MaxTimer;
                }
            );
            vampireKillButton.buttonText = ModTranslation.GetString("VampireText");

            garlicButton = new CustomButton(
                () =>
                {
                    Vampire.localPlacedGarlic = true;
                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlaceGarlic, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.placeGarlic(buff);
                },
                () => { return !Vampire.localPlacedGarlic && PlayerControl.LocalPlayer.IsAlive() && Vampire.garlicsActive && !PlayerControl.LocalPlayer.IsGM(); },
                () => { return PlayerControl.LocalPlayer.CanMove && !Vampire.localPlacedGarlic; },
                () => { },
                Vampire.getGarlicButtonSprite(),
                new Vector3(0, -0.06f, 0),
                __instance,
                __instance.UseButton,
                null,
                true
            );
            garlicButton.buttonText = ModTranslation.GetString("GarlicText");


            // Jackal Sidekick Button
            jackalSidekickButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.JackalCreatesSidekick, Hazel.SendOption.Reliable, -1);
                    writer.Write(Jackal.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.jackalCreatesSidekick(Jackal.currentTarget.PlayerId);
                },
                () => { return Jackal.canCreateSidekick && Jackal.jackal != null && Jackal.jackal == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return Jackal.canCreateSidekick && Jackal.currentTarget != null && PlayerControl.LocalPlayer.CanMove; },
                () => { jackalSidekickButton.Timer = jackalSidekickButton.MaxTimer; },
                Jackal.getSidekickButtonSprite(),
                new Vector3(-3f, 0, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            jackalSidekickButton.buttonText = ModTranslation.GetString("SidekickText");

            // Jackal Kill
            jackalKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMuderAttemptAndKill(Jackal.jackal, Jackal.currentTarget) == MurderAttemptResult.SuppressKill) return;

                    jackalKillButton.Timer = jackalKillButton.MaxTimer;
                    Jackal.currentTarget = null;
                },
                () => { return PlayerControl.LocalPlayer.IsRole(RoleType.Jackal) && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return Jackal.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { jackalKillButton.Timer = jackalKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                new Vector3(0, 1f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q
            );

            // Sidekick Kill
            sidekickKillButton = new CustomButton(
                () =>
                {
                    if (Helpers.checkMuderAttemptAndKill(Sidekick.sidekick, Sidekick.currentTarget) == MurderAttemptResult.SuppressKill) return;
                    sidekickKillButton.Timer = sidekickKillButton.MaxTimer;
                    Sidekick.currentTarget = null;
                },
                () => { return Sidekick.canKill && Sidekick.sidekick != null && Sidekick.sidekick == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return Sidekick.currentTarget && PlayerControl.LocalPlayer.CanMove; },
                () => { sidekickKillButton.Timer = sidekickKillButton.MaxTimer; },
                __instance.KillButton.graphic.sprite,
                new Vector3(0, 1f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q
            );


            // Eraser erase button
            eraserButton = new CustomButton(
                () =>
                {
                    eraserButton.MaxTimer += Eraser.cooldownIncrease;
                    eraserButton.Timer = eraserButton.MaxTimer;

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetFutureErased, Hazel.SendOption.Reliable, -1);
                    writer.Write(Eraser.currentTarget.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.setFutureErased(Eraser.currentTarget.PlayerId);
                },
                () => { return Eraser.eraser != null && Eraser.eraser == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove && Eraser.currentTarget != null; },
                () => { eraserButton.Timer = eraserButton.MaxTimer; },
                Eraser.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            eraserButton.buttonText = ModTranslation.GetString("EraserText");

            placeJackInTheBoxButton = new CustomButton(
                () =>
                {
                    placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer;

                    var pos = PlayerControl.LocalPlayer.transform.position;
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlaceJackInTheBox, Hazel.SendOption.Reliable);
                    writer.WriteBytesAndSize(buff);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.placeJackInTheBox(buff);
                },
                () => { return Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive() && !JackInTheBox.HasJackInTheBoxLimitReached(); },
                () => { return PlayerControl.LocalPlayer.CanMove && !JackInTheBox.HasJackInTheBoxLimitReached(); },
                () => { placeJackInTheBoxButton.Timer = placeJackInTheBoxButton.MaxTimer; },
                Trickster.getPlaceBoxButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            placeJackInTheBoxButton.buttonText = ModTranslation.GetString("PlaceJackInTheBoxText");

            lightsOutButton = new CustomButton(
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LightsOut, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.lightsOut();
                },
                () => { return Trickster.trickster != null && Trickster.trickster == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive() && JackInTheBox.HasJackInTheBoxLimitReached() && JackInTheBox.BoxesConvertedToVents; },
                () => { return PlayerControl.LocalPlayer.CanMove && JackInTheBox.HasJackInTheBoxLimitReached() && JackInTheBox.BoxesConvertedToVents; },
                () =>
                {
                    lightsOutButton.Timer = lightsOutButton.MaxTimer;
                    lightsOutButton.isEffectActive = false;
                    lightsOutButton.actionButton.graphic.color = Palette.EnabledColor;
                },
                Trickster.getLightsOutButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F,
                true,
                Trickster.lightsOutDuration,
                () => { lightsOutButton.Timer = lightsOutButton.MaxTimer; }
            );
            lightsOutButton.buttonText = ModTranslation.GetString("LightsOutText");

            // Cleaner Clean
            cleanerCleanButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RpcProcedure.CleanBody(playerInfo.PlayerId);

                                    Cleaner.cleaner.killTimer = cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                },
                () => { return Cleaner.cleaner != null && Cleaner.cleaner == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
                () => { cleanerCleanButton.Timer = cleanerCleanButton.MaxTimer; },
                Cleaner.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            cleanerCleanButton.buttonText = ModTranslation.GetString("CleanText");

            // Warlock curse
            warlockCurseButton = new CustomButton(
                () =>
                {
                    if (Warlock.curseVictim == null)
                    {
                        // Apply Curse
                        Warlock.curseVictim = Warlock.currentTarget;
                        warlockCurseButton.Sprite = Warlock.getCurseKillButtonSprite();
                        warlockCurseButton.Timer = 1f;
                        warlockCurseButton.buttonText = ModTranslation.GetString("CurseKillText");
                    }
                    else if (Warlock.curseVictim != null && Warlock.curseVictimTarget != null)
                    {
                        MurderAttemptResult murder = Helpers.checkMuderAttemptAndKill(Warlock.warlock, Warlock.curseVictimTarget, showAnimation: false);
                        if (murder == MurderAttemptResult.SuppressKill) return;

                        // If blanked or killed
                        warlockCurseButton.buttonText = ModTranslation.GetString("CurseText");
                        if (Warlock.rootTime > 0)
                        {
                            PlayerControl.LocalPlayer.moveable = false;
                            PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement so the warlock is not just running straight into the next object
                            HudManager.Instance.StartCoroutine(Effects.Lerp(Warlock.rootTime, new Action<float>((p) =>
                            { // Delayed action
                                if (p == 1f)
                                {
                                    PlayerControl.LocalPlayer.moveable = true;
                                }
                            })));
                        }

                        Warlock.curseVictim = null;
                        Warlock.curseVictimTarget = null;
                        warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
                        Warlock.warlock.killTimer = warlockCurseButton.Timer = warlockCurseButton.MaxTimer;

                    }
                },
                () => { return Warlock.warlock != null && Warlock.warlock == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return ((Warlock.curseVictim == null && Warlock.currentTarget != null) || (Warlock.curseVictim != null && Warlock.curseVictimTarget != null)) && PlayerControl.LocalPlayer.CanMove; },
                () =>
                {
                    warlockCurseButton.Timer = warlockCurseButton.MaxTimer;
                    warlockCurseButton.Sprite = Warlock.getCurseButtonSprite();
                    warlockCurseButton.buttonText = ModTranslation.GetString("CurseText");
                    Warlock.curseVictim = null;
                    Warlock.curseVictimTarget = null;
                },
                Warlock.getCurseButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            warlockCurseButton.buttonText = ModTranslation.GetString("CurseText");

            // Security Guard button
            securityGuardButton = new CustomButton(
                () =>
                {
                    if (SecurityGuard.ventTarget != null)
                    { // Seal vent
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SealVent, Hazel.SendOption.Reliable);
                        writer.WritePacked(SecurityGuard.ventTarget.Id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.sealVent(SecurityGuard.ventTarget.Id);
                        SecurityGuard.ventTarget = null;

                    }
                    else if (GameManager.Instance.LogicOptions.MapId != 1 && MapOptions.couldUseCameras)
                    { // Place camera if there's no vent and it's not MiraHQ
                        var pos = PlayerControl.LocalPlayer.transform.position;
                        byte[] buff = new byte[sizeof(float) * 2];
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                        Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                        byte roomId;
                        try
                        {
                            roomId = (byte)HudManager.Instance.roomTracker.LastRoom.RoomId;
                        }
                        catch
                        {
                            roomId = 255;
                        }

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.PlaceCamera, Hazel.SendOption.Reliable);
                        writer.WriteBytesAndSize(buff);
                        writer.Write(roomId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.placeCamera(buff, roomId);
                    }
                    securityGuardButton.Timer = securityGuardButton.MaxTimer;
                },
                () => { return SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive() && SecurityGuard.remainingScrews >= Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice); },
                () =>
                {
                    if (SecurityGuard.ventTarget == null && GameManager.Instance.LogicOptions.MapId != 1)
                    {
                        securityGuardButton.buttonText = ModTranslation.GetString("PlaceCameraText");
                        securityGuardButton.Sprite = SecurityGuard.getPlaceCameraButtonSprite();
                    }
                    else
                    {
                        securityGuardButton.buttonText = ModTranslation.GetString("CloseVentText");
                        securityGuardButton.Sprite = SecurityGuard.getCloseVentButtonSprite();
                    }
                    if (securityGuardButtonScrewsText != null) securityGuardButtonScrewsText.text = String.Format(ModTranslation.GetString("securityGuardScrews"), SecurityGuard.remainingScrews);

                    if (SecurityGuard.ventTarget != null)
                    {
                        return SecurityGuard.remainingScrews >= SecurityGuard.ventPrice && PlayerControl.LocalPlayer.CanMove;
                    }

                    return GameManager.Instance.LogicOptions.MapId != 1 && MapOptions.couldUseCameras && SecurityGuard.remainingScrews >= SecurityGuard.camPrice && PlayerControl.LocalPlayer.CanMove;
                },
                () => { securityGuardButton.Timer = securityGuardButton.MaxTimer; },
                SecurityGuard.getPlaceCameraButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F
            );
            securityGuardButton.buttonText = ModTranslation.GetString("PlaceCameraText");

            // Security Guard button screws counter
            securityGuardButtonScrewsText = GameObject.Instantiate(securityGuardButton.actionButton.cooldownTimerText, securityGuardButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardButtonScrewsText.text = "";
            securityGuardButtonScrewsText.enableWordWrapping = false;
            securityGuardButtonScrewsText.transform.localScale = Vector3.one * 0.5f;
            securityGuardButtonScrewsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            securityGuardCamButton = new CustomButton(
                () => {
                    if (GameManager.Instance.LogicOptions.MapId != 1) {
                        if (SecurityGuard.minigame == null) {
                            byte mapId = GameManager.Instance.LogicOptions.MapId;
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("Surv_Panel"));
                            if (mapId == 0 || mapId == 3) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvConsole"));
                            else if (mapId == 4) e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("task_cams"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.minigame.Begin(null);
                    } else {
                        if (SecurityGuard.minigame == null) {
                            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                            if (e == null || Camera.main == null) return;
                            SecurityGuard.minigame = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                        }
                        SecurityGuard.minigame.transform.SetParent(Camera.main.transform, false);
                        SecurityGuard.minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                        SecurityGuard.minigame.Begin(null);
                    }
                    SecurityGuard.charges--;

                    if (SecurityGuard.cantMove) PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement 
                },
                () => { return SecurityGuard.securityGuard != null && SecurityGuard.securityGuard == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && SecurityGuard.remainingScrews < Mathf.Min(SecurityGuard.ventPrice, SecurityGuard.camPrice); },
                () => {
                    if (securityGuardChargesText != null)
                        securityGuardChargesText.text = securityGuardChargesText.text = String.Format(ModTranslation.GetString("hackerChargesText"), SecurityGuard.charges, SecurityGuard.maxCharges);
                    securityGuardCamButton.actionButton.graphic.sprite = GameManager.Instance.LogicOptions.MapId == 1 ? SecurityGuard.getLogSprite() : SecurityGuard.getCamSprite();
                    securityGuardCamButton.actionButton.OverrideText(GameManager.Instance.LogicOptions.MapId == 1 ?
                        TranslationController.Instance.GetString(StringNames.SecurityLogsSystem) :
                        TranslationController.Instance.GetString(StringNames.SecurityCamsSystem));
                    return PlayerControl.LocalPlayer.CanMove && SecurityGuard.charges > 0;
                },
                () => {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    securityGuardCamButton.isEffectActive = false;
                    securityGuardCamButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                },
                SecurityGuard.getCamSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.Q,
                true,
                0f,
                () => {
                    securityGuardCamButton.Timer = securityGuardCamButton.MaxTimer;
                    if (Minigame.Instance) {
                        SecurityGuard.minigame.ForceClose();
                    }
                    PlayerControl.LocalPlayer.moveable = true;
                },
                false,
                GameManager.Instance.LogicOptions.MapId == 1 ?
                    TranslationController.Instance.GetString(StringNames.SecurityLogsSystem) :
                    TranslationController.Instance.GetString(StringNames.SecurityCamsSystem)
            );

            // Security Guard cam button charges
            securityGuardChargesText = GameObject.Instantiate(securityGuardCamButton.actionButton.cooldownTimerText, securityGuardCamButton.actionButton.cooldownTimerText.transform.parent);
            securityGuardChargesText.text = "";
            securityGuardChargesText.enableWordWrapping = false;
            securityGuardChargesText.transform.localScale = Vector3.one * 0.5f;
            securityGuardChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            // Arsonist button
            arsonistButton = new CustomButton(
                () =>
                {
                    if (Arsonist.currentTarget != null)
                    {
                        Arsonist.douseTarget = Arsonist.currentTarget;
                    }
                },
                () => { return Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer && !Arsonist.dousedEveryone && PlayerControl.LocalPlayer.IsAlive(); },
                () =>
                {
                    if (arsonistButton.isEffectActive && Arsonist.douseTarget != Arsonist.currentTarget)
                    {
                        Arsonist.douseTarget = null;
                        arsonistButton.Timer = 0f;
                        arsonistButton.isEffectActive = false;
                    }

                    return PlayerControl.LocalPlayer.CanMove && Arsonist.currentTarget != null;
                },
                () =>
                {
                    arsonistButton.Timer = arsonistButton.MaxTimer;
                    arsonistButton.isEffectActive = false;
                    Arsonist.douseTarget = null;
                    Arsonist.updateStatus();
                },
                Arsonist.getDouseSprite(),
                new Vector3(-1.8f, 0f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F,
                true,
                Arsonist.duration,
                () =>
                {
                    if (Arsonist.douseTarget != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ArsonistDouse, Hazel.SendOption.Reliable, -1);
                        writer.Write(Arsonist.douseTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.arsonistDouse(Arsonist.douseTarget.PlayerId);
                    }

                    Arsonist.douseTarget = null;
                    Arsonist.updateStatus();
                    arsonistButton.Timer = Arsonist.dousedEveryone ? 0 : arsonistButton.MaxTimer;

                    foreach (PlayerControl p in Arsonist.dousedPlayers)
                    {
                        if (MapOptions.PlayerIcons.ContainsKey(p.PlayerId))
                        {
                            MapOptions.PlayerIcons[p.PlayerId].SetSemiTransparent(false);
                        }
                    }
                }
            );
            arsonistButton.buttonText = ModTranslation.GetString("DouseText");

            arsonistIgniteButton = new CustomButton(
                () =>
                {
                    if (Arsonist.dousedEveryone)
                    {
                        MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ArsonistWin, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                        RpcProcedure.arsonistWin();
                    }
                },
                () => { return Arsonist.arsonist != null && Arsonist.arsonist == PlayerControl.LocalPlayer && Arsonist.dousedEveryone && PlayerControl.LocalPlayer.IsAlive(); },
                () => { return PlayerControl.LocalPlayer.CanMove && Arsonist.dousedEveryone; },
                () => { },
                Arsonist.getIgniteSprite(),
                new Vector3(-1.8f, 0f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q
            );
            arsonistIgniteButton.buttonText = ModTranslation.GetString("IgniteText");

            // Vulture Eat
            vultureEatButton = new CustomButton(
                () =>
                {
                    foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                    {
                        if (collider2D.tag == "DeadBody")
                        {
                            DeadBody component = collider2D.GetComponent<DeadBody>();
                            if (component && !component.Reported)
                            {
                                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                                Vector2 truePosition2 = component.TruePosition;
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.VultureEat, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RpcProcedure.vultureEat(playerInfo.PlayerId);

                                    Vulture.cooldown = vultureEatButton.Timer = vultureEatButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                    if (Vulture.eatenBodies >= Vulture.vultureNumberToWin)
                    {
                        MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.VultureWin, Hazel.SendOption.Reliable, -1);
                        AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                        RpcProcedure.vultureWin();
                        return;
                    }
                },
                () => { return Vulture.vulture != null && Vulture.vulture == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () =>
                {
                    if (vultureNumCorpsesText != null)
                        vultureNumCorpsesText.text = String.Format(ModTranslation.GetString("vultureCorpses"), Vulture.vultureNumberToWin - Vulture.eatenBodies);
                    return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
                },
                () => { vultureEatButton.Timer = vultureEatButton.MaxTimer; },
                Vulture.getButtonSprite(),
                new Vector3(-3f, 0, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F
            );
            vultureEatButton.buttonText = ModTranslation.GetString("VultureText");

            vultureNumCorpsesText = GameObject.Instantiate(vultureEatButton.actionButton.cooldownTimerText, vultureEatButton.actionButton.cooldownTimerText.transform.parent);
            vultureNumCorpsesText.text = "";
            vultureNumCorpsesText.enableWordWrapping = false;
            vultureNumCorpsesText.transform.localScale = Vector3.one * 0.5f;
            vultureNumCorpsesText.transform.localPosition += new Vector3(0.0f, 0.7f, 0);

            // Medium button
            mediumButton = new CustomButton(
                () =>
                {
                    if (Medium.target != null)
                    {
                        Medium.soulTarget = Medium.target;
                        mediumButton.HasEffect = true;
                    }
                },
                () => { return Medium.medium != null && Medium.medium == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () =>
                {
                    if (mediumButton.isEffectActive && Medium.target != Medium.soulTarget)
                    {
                        Medium.soulTarget = null;
                        mediumButton.Timer = 0f;
                        mediumButton.isEffectActive = false;
                    }
                    return Medium.target != null && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    mediumButton.isEffectActive = false;
                    Medium.soulTarget = null;
                },
                Medium.getQuestionSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F,
                true,
                Medium.duration,
                () =>
                {
                    mediumButton.Timer = mediumButton.MaxTimer;
                    if (Medium.target == null || Medium.target.player == null) return;
                    string msg = "";

                    int randomNumber = Medium.target.killerIfExisting?.PlayerId == Mini.mini?.PlayerId ? TheOtherRoles.rnd.Next(3) : TheOtherRoles.rnd.Next(4);
                    string typeOfColor = Helpers.IsLighterColor(Medium.target.killerIfExisting.Data.DefaultOutfit.ColorId) ?
                        ModTranslation.GetString("detectiveColorLight") :
                        ModTranslation.GetString("detectiveColorDark");
                    float timeSinceDeath = ((float)(Medium.meetingStartTime - Medium.target.timeOfDeath).TotalMilliseconds);
                    string name = " (" + Medium.target.player.Data.PlayerName + ")";

                    if (randomNumber == 0) msg = string.Format(ModTranslation.GetString("mediumQuestion1"), RoleInfo.GetRolesString(Medium.target.player, false, includeHidden: true)) + name;
                    else if (randomNumber == 1) msg = string.Format(ModTranslation.GetString("mediumQuestion2"), typeOfColor) + name;
                    else if (randomNumber == 2) msg = string.Format(ModTranslation.GetString("mediumQuestion3"), Math.Round(timeSinceDeath / 1000)) + name;
                    else msg = string.Format(ModTranslation.GetString("mediumQuestion4"), RoleInfo.GetRolesString(Medium.target.killerIfExisting, false, includeHidden: true)) + name; ; // Excludes mini 

                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{msg}");

                    // Remove soul
                    if (Medium.oneTimeUse)
                    {
                        float closestDistance = float.MaxValue;
                        SpriteRenderer target = null;

                        foreach ((DeadPlayer db, Vector3 ps) in Medium.deadBodies)
                        {
                            if (db == Medium.target)
                            {
                                Tuple<DeadPlayer, Vector3> deadBody = Tuple.Create(db, ps);
                                Medium.deadBodies.Remove(deadBody);
                                break;
                            }

                        }
                        foreach (SpriteRenderer rend in Medium.souls)
                        {
                            float distance = Vector2.Distance(rend.transform.position, PlayerControl.LocalPlayer.GetTruePosition());
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                target = rend;
                            }
                        }

                        HudManager.Instance.StartCoroutine(Effects.Lerp(5f, new Action<float>((p) =>
                        {
                            if (target != null)
                            {
                                var tmp = target.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                target.color = tmp;
                            }
                            if (p == 1f && target != null && target.gameObject != null) UnityEngine.Object.Destroy(target.gameObject);
                        })));

                        Medium.souls.Remove(target);
                    }
                }
            );
            mediumButton.buttonText = ModTranslation.GetString("MediumText");

            // Pursuer button
            pursuerButton = new CustomButton(
                () =>
                {
                    if (Pursuer.target != null)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetBlanked, Hazel.SendOption.Reliable, -1);
                        writer.Write(Pursuer.target.PlayerId);
                        writer.Write(Byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.setBlanked(Pursuer.target.PlayerId, Byte.MaxValue);

                        Pursuer.target = null;

                        Pursuer.blanks++;
                        pursuerButton.Timer = pursuerButton.MaxTimer;
                    }

                },
                () => { return Pursuer.pursuer != null && Pursuer.pursuer == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive() && Pursuer.blanks < Pursuer.blanksNumber; },
                () =>
                {
                    if (pursuerButtonBlanksText != null) pursuerButtonBlanksText.text = String.Format(ModTranslation.GetString("pursuerBlanks"), Pursuer.blanksNumber - Pursuer.blanks);

                    return Pursuer.blanksNumber > Pursuer.blanks && PlayerControl.LocalPlayer.CanMove && Pursuer.target != null;
                },
                () => { pursuerButton.Timer = pursuerButton.MaxTimer; },
                Pursuer.getTargetSprite(),
                new Vector3(-1.8f, 0f, 0),
                __instance,
                __instance.UseButton,
                KeyCode.F
            );
            pursuerButton.buttonText = ModTranslation.GetString("PursuerText");

            // Pursuer button blanks left
            pursuerButtonBlanksText = GameObject.Instantiate(pursuerButton.actionButton.cooldownTimerText, pursuerButton.actionButton.cooldownTimerText.transform.parent);
            pursuerButtonBlanksText.text = "";
            pursuerButtonBlanksText.enableWordWrapping = false;
            pursuerButtonBlanksText.transform.localScale = Vector3.one * 0.5f;
            pursuerButtonBlanksText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);


            // Witch Spell button
            witchSpellButton = new CustomButton(
                () =>
                {
                    if (Witch.currentTarget != null)
                    {
                        Witch.spellCastingTarget = Witch.currentTarget;
                    }
                },
                () => { return Witch.witch != null && Witch.witch == PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsAlive(); },
                () =>
                {
                    if (witchSpellButton.isEffectActive && Witch.spellCastingTarget != Witch.currentTarget)
                    {
                        Witch.spellCastingTarget = null;
                        witchSpellButton.Timer = 0f;
                        witchSpellButton.isEffectActive = false;
                    }
                    return PlayerControl.LocalPlayer.CanMove && Witch.currentTarget != null;
                },
                () =>
                {
                    witchSpellButton.Timer = witchSpellButton.MaxTimer;
                    witchSpellButton.isEffectActive = false;
                    Witch.spellCastingTarget = null;
                },
                Witch.getButtonSprite(),
                new Vector3(-1.8f, 0f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.F,
                true,
                Witch.spellCastingDuration,
                () =>
                {
                    if (Witch.spellCastingTarget == null) return;
                    MurderAttemptResult attempt = Helpers.CheckMuderAttempt(Witch.witch, Witch.spellCastingTarget);
                    if (attempt == MurderAttemptResult.PerformKill)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetFutureSpelled, Hazel.SendOption.Reliable, -1);
                        writer.Write(Witch.currentTarget.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.setFutureSpelled(Witch.currentTarget.PlayerId);
                    }
                    if (attempt == MurderAttemptResult.BlankKill || attempt == MurderAttemptResult.PerformKill)
                    {
                        witchSpellButton.MaxTimer += Witch.cooldownAddition;
                        witchSpellButton.Timer = witchSpellButton.MaxTimer;
                        if (Witch.triggerBothCooldowns)
                            Witch.witch.killTimer = GameManager.Instance.LogicOptions.GetKillCooldown();
                    }
                    else
                    {
                        witchSpellButton.Timer = 0f;
                    }
                    Witch.spellCastingTarget = null;
                }
            );
            witchSpellButton.buttonText = ModTranslation.GetString("WitchText");

            ButtonsGM.makeButtons(__instance);

            // Set the default (or settings from the previous game) timers/durations when spawning the buttons
            setCustomButtonCooldowns();
        }
    }
}
