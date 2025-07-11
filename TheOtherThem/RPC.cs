using AmongUs.GameOptions;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using TheOtherThem.ToTRole;
using UnityEngine;
using static TheOtherThem.GameHistory;
using static TheOtherThem.HudManagerStartPatch;
using static TheOtherThem.MapOptions;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;

namespace TheOtherThem
{

    public enum CustomRpc
    {
        // Main Controls

        ResetVaribles = 100,
        ShareOptions,
        ForceEnd,
        SetRole,
        SetLovers,
        VersionHandshake,
        UseUncheckedVent,
        UncheckedMurderPlayer,
        UncheckedCmdReportDeadBody,
        OverrideNativeRole,
        UncheckedExilePlayer,
        UncheckedEndGame,
        UncheckedSetTasks,
        DynamicMapOption,

        // Role functionality

        EngineerFixLights,
        EngineerUsedRepair,
        CleanBody,
        SheriffKill,
        MedicSetShielded,
        ShieldedMurderAttempt,
        TimeMasterShield,
        TimeMasterRewindTime,
        ShifterShift,
        SwapperSwap,
        MorphlingMorph,
        CamouflagerCamouflage,
        TrackerUsedTracker,
        VampireSetBitten,
        PlaceGarlic,
        JackalCreatesSidekick,
        SidekickPromotes,
        ErasePlayerRoles,
        SetFutureErased,
        SetFutureShifted,
        SetFutureShielded,
        SetFutureSpelled,
        WitchSpellCast,
        PlaceJackInTheBox,
        LightsOut,
        PlaceCamera,
        SealVent,
        ArsonistWin,
        GuesserShoot,
        VultureWin,
        LawyerWin,
        LawyerSetTarget,
        LawyerPromotesToPursuer,
        SetBlanked,

        // GM Edition functionality
        AddModifier,
        NinjaStealth,
        SetShifterType,
        GMKill,
        GMRevive,
        UseAdminTime,
        UseCameraTime,
        UseVitalsTime,
        ArsonistDouse,
        VultureEat,
        PlagueDoctorWin,
        PlagueDoctorSetInfected,
        PlagueDoctorUpdateProgress,
        NekoKabochaExile,
        SerialKillerSuicide,
        FortuneTellerUsedDivine,
        FoxStealth,
        FoxCreatesImmoralist,
        SwapperAnimate,

        // ToT RPCs
        InnerslothSabotage,
        ResetAllKillCooldown,
        RoleDataSync,
        PhoenixPossess,
    }

    public static class RpcProcedure
    {
        // Main Controls

        public static void ResetVariables()
        {
            Garlic.clearGarlics();
            JackInTheBox.ClearJackInTheBoxes();
            ClearAndReloadMapOptions();
            TheOtherRoles.ClearAndReloadRoles();
            clearGameHistory();
            setCustomButtonCooldowns();
            AdminPatch.ResetData();
            CameraPatch.ResetData();
            VitalsPatch.ResetData();
            MapBehaviorPatch.resetIcons();
            CustomOverlays.ResetOverlays();
            CustomRole.ReloadAll();

            KillAnimationCoPerformKillPatch.hideNextAnimation = false;
        }

        public static void ShareOptions(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption option = CustomOption.Options.FirstOrDefault(option => option.Id == (int)optionId);
                    option.UpdateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                Main.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }

        public static void ForceEnd()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (!player.Data.Role.IsImpostor)
                {
                    RoleManager.Instance.SetRole(player, AmongUs.GameOptions.RoleTypes.Crewmate);
                    player.MurderPlayerQuick(player);
                    player.Data.IsDead = true;
                }
            }
        }

        public static void SetRole(byte roleId, byte playerId, byte flag)
        {
            var modRole = (RoleType)roleId;
            var nativeRole = (byte)RoleType.Impostor <= roleId && (byte)RoleType.ImpostorMax > roleId ? RoleTypes.Impostor : RoleTypes.Crewmate;
            var player = GameData.Instance.GetPlayerById(playerId).Object;
            player.SetRole(modRole);
            player.StartCoroutine(player.CoSetRole(nativeRole, false));
        }

        public static void AddModifier(byte modId, byte playerId)
        {
            PlayerControl.AllPlayerControls.ToArray().DoIf(
                x => x.PlayerId == playerId,
                x => x.AddModifier((ModifierType)modId)
            );
        }

        public static void SetLovers(byte playerId1, byte playerId2)
        {
            Lovers.AddCouple(Helpers.PlayerById(playerId1), Helpers.PlayerById(playerId2));
        }

        public static void OverrideNativeRole(byte playerId, byte roleType)
        {
            var player = Helpers.PlayerById(playerId);
            player.roleAssigned = false;
            DestroyableSingleton<RoleManager>.Instance.SetRole(player, (RoleTypes)roleType);
        }

        public static void VersionHandshake(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            Version ver;
            if (revision < 0)
                ver = new(major, minor, build);
            else
                ver = new(major, minor, build, revision);
            GameStartManagerPatch.PlayerVersions[clientId] = new(ver, guid);
        }

        public static void UseUncheckedVent(int ventId, byte playerId, byte isEnter)
        {
            PlayerControl player = Helpers.PlayerById(playerId);
            if (player == null) return;
            // Fill dummy MessageReader and call MyPhysics.HandleRpc as the corountines cannot be accessed
            var reader = new MessageReader();
            byte[] bytes = BitConverter.GetBytes(ventId);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            reader.Buffer = bytes;
            reader.Length = bytes.Length;

            JackInTheBox.StartAnimation(ventId);
            player.MyPhysics.HandleRpc((byte)(isEnter != 0 ? RpcCalls.EnterVent : RpcCalls.ExitVent), reader);
        }

        public static void UncheckedMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
        {
            PlayerControl source = Helpers.PlayerById(sourceId);
            PlayerControl target = Helpers.PlayerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayerQuick(target);
            }
        }

        public static void UncheckedCmdReportDeadBody(byte sourceId, byte targetId)
        {
            PlayerControl source = Helpers.PlayerById(sourceId);
            PlayerControl target = Helpers.PlayerById(targetId);
            if (source != null && target != null) source.ReportDeadBody(target.Data);
        }

        public static void UncheckedExilePlayer(byte targetId)
        {
            PlayerControl target = Helpers.PlayerById(targetId);
            target.DoIfNotNull(t => t.Exiled());
        }

        public static void uncheckedEndGame(byte reason)
        {
            AmongUsClient.Instance.GameState = InnerNet.InnerNetClient.GameStates.Ended;
            var obj2 = AmongUsClient.Instance.allClients;
            lock (obj2)
            {
                AmongUsClient.Instance.allClients.Clear();
            }

            var obj = AmongUsClient.Instance.Dispatcher;
            lock (obj)
            {
                AmongUsClient.Instance.Dispatcher.Add(new Action(() =>
                {
                    ShipStatus.Instance.enabled = false;
                    AmongUsClient.Instance.OnGameEnd(new EndGameResult((GameOverReason)reason, false));

                    if (AmongUsClient.Instance.AmHost)
                        GameManager.Instance.RpcEndGame((GameOverReason)reason, false);
                }));
            }
        }

        public static void uncheckedSetTasks(byte playerId, byte[] taskTypeIds)
        {
            var player = Helpers.PlayerById(playerId);
            player.ClearAllTasks();

            player.Data.SetTasks(taskTypeIds);
        }

        public static void DynamicMapOption(byte mapId)
        {
            GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().MapId = mapId;
        }

        // Role functionality

        public static void EngineerFixLights()
        {
            var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }

        public static void EngineerUsedRepair()
        {
            Engineer.remainingFixes--;
        }

        public static void CleanBody(byte playerId)
        {
            Object.FindObjectsOfType<DeadBody>().DoIf(body => GameData.Instance.GetPlayerById(body.ParentId).PlayerId == playerId, body => body.Destroy());
        }

        public static void SheriffKill(byte sheriffId, byte targetId, bool misfire)
        {
            PlayerControl sheriff = Helpers.PlayerById(sheriffId);
            PlayerControl target = Helpers.PlayerById(targetId);
            if (sheriff == null || target == null) return;

            Sheriff role = Sheriff.GetRole(sheriff);
            if (role != null)
                role.numShots--;

            if (misfire)
            {
                sheriff.MurderPlayerQuick(sheriff);
                FinalStatuses[sheriffId] = FinalStatus.Misfire;

                if (!Sheriff.misfireKillsTarget) return;
                FinalStatuses[targetId] = FinalStatus.Misfire;
            }

            sheriff.MurderPlayerQuick(target);
        }

        public static void TimeMasterRewindTime()
        {
            TimeMaster.shieldActive = false; // Shield is no longer active when rewinding
            if (TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer)
            {
                resetTimeMasterButton();
            }
            HudManager.Instance.FullScreen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
            HudManager.Instance.FullScreen.enabled = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(TimeMaster.rewindTime / 2, new Action<float>((p) =>
            {
                if (p == 1f) HudManager.Instance.FullScreen.enabled = false;
            })));

            if (TimeMaster.timeMaster == null || PlayerControl.LocalPlayer == TimeMaster.timeMaster) return; // Time Master himself does not rewind
            if (PlayerControl.LocalPlayer.IsGM()) return; // GM does not rewind

            TimeMaster.isRewinding = true;

            if (MapBehaviour.Instance)
                MapBehaviour.Instance.Close();
            if (Minigame.Instance)
                Minigame.Instance.ForceClose();
            PlayerControl.LocalPlayer.moveable = false;
        }

        public static void timeMasterShield()
        {
            TimeMaster.shieldActive = true;
            HudManager.Instance.StartCoroutine(Effects.Lerp(TimeMaster.shieldDuration, new Action<float>((p) =>
            {
                if (p == 1f) TimeMaster.shieldActive = false;
            })));
        }

        public static void medicSetShielded(byte shieldedId)
        {
            Medic.usedShield = true;
            Medic.shielded = Helpers.PlayerById(shieldedId);
            Medic.futureShielded = null;
        }

        public static void shieldedMurderAttempt()
        {
            if (Medic.shielded == null || Medic.medic == null) return;

            bool isShieldedAndShow = Medic.shielded == PlayerControl.LocalPlayer && Medic.showAttemptToShielded;
            bool isMedicAndShow = Medic.medic == PlayerControl.LocalPlayer && Medic.showAttemptToMedic;

            if ((isShieldedAndShow || isMedicAndShow) && HudManager.Instance?.FullScreen != null)
            {
                HudManager.Instance.FullScreen.enabled = true;
                HudManager.Instance.StartCoroutine(Effects.Lerp(0.5f, new Action<float>((p) =>
                {
                    var renderer = HudManager.Instance.FullScreen;
                    Color c = Palette.ImpostorRed;
                    if (p < 0.5)
                    {
                        if (renderer != null)
                            renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(p * 2 * 0.75f));
                    }
                    else
                    {
                        if (renderer != null)
                            renderer.color = new Color(c.r, c.g, c.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                })));
            }
        }

        public static void shifterShift(byte targetId)
        {
            PlayerControl oldShifter = Shifter.shifter;
            PlayerControl player = Helpers.PlayerById(targetId);
            if (player == null || oldShifter == null) return;

            Shifter.futureShift = null;
            if (!Shifter.isNeutral)
                Shifter.clearAndReload();

            if (player == GM.gm)
            {
                return;
            }

            // Suicide (exile) when impostor or impostor variants
            if (!Shifter.isNeutral && (player.Data.Role.IsImpostor || player.IsNeutral() || player.HasModifier(ModifierType.Madmate)))
            {
                oldShifter.Exiled();
                FinalStatuses[oldShifter.PlayerId] = FinalStatus.Suicide;
                return;
            }

            if (Shifter.shiftModifiers)
            {
                // Switch shield
                if (Medic.shielded != null && Medic.shielded == player)
                {
                    Medic.shielded = oldShifter;
                }
                else if (Medic.shielded != null && Medic.shielded == oldShifter)
                {
                    Medic.shielded = player;
                }

                player.swapModifiers(oldShifter);
                Lovers.SwapLovers(oldShifter, player);
            }

            // Shift role
            player.SwapRoles(oldShifter);

            if (Shifter.isNeutral)
            {
                Shifter.shifter = player;
                Shifter.pastShifters.Add(oldShifter.PlayerId);

                if (player.Data.Role.IsImpostor)
                {
                    DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                    DestroyableSingleton<RoleManager>.Instance.SetRole(oldShifter, RoleTypes.Impostor);
                }
            }

            if (Lawyer.lawyer != null && Lawyer.target == player)
            {
                Lawyer.target = oldShifter;
            }

            // Set cooldowns to max for both players
            if (PlayerControl.LocalPlayer == oldShifter || PlayerControl.LocalPlayer == player)
                CustomButton.ResetAllCooldowns();
        }

        public static void swapperSwap(byte playerId1, byte playerId2)
        {
            if (MeetingHud.Instance)
            {
                Swapper.playerId1 = playerId1;
                Swapper.playerId2 = playerId2;
            }
        }

        public static void swapperAnimate()
        {
            MeetingHudPatch.animateSwap = true;
        }

        public static void morphlingMorph(byte playerId)
        {
            PlayerControl target = Helpers.PlayerById(playerId);
            if (Morphling.morphling == null || target == null) return;
            Morphling.startMorph(target);
        }

        public static void camouflagerCamouflage()
        {
            if (Camouflager.camouflager == null) return;
            Camouflager.startCamouflage();
        }

        public static void vampireSetBitten(byte targetId, byte performReset)
        {
            if (performReset != 0)
            {
                Vampire.bitten = null;
                return;
            }

            if (Vampire.vampire == null) return;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == targetId && !player.Data.IsDead)
                {
                    Vampire.bitten = player;
                }
            }
        }

        public static void placeGarlic(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new Garlic(position);
        }

        public static void trackerUsedTracker(byte targetId)
        {
            Tracker.usedTracker = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == targetId)
                    Tracker.tracked = player;
        }

        public static void jackalCreatesSidekick(byte targetId)
        {
            PlayerControl player = Helpers.PlayerById(targetId);
            if (player == null) return;

            if (!Jackal.canCreateSidekickFromImpostor && player.Data.Role.IsImpostor)
            {
                Jackal.fakeSidekick = player;
            }
            else if (!Jackal.canCreateSidekickFromFox && player.IsRole(RoleType.Fox))
            {
                Jackal.fakeSidekick = player;
            }
            else
            {
                DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                erasePlayerRoles(player.PlayerId, true);
                Sidekick.sidekick = player;
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) PlayerControl.LocalPlayer.moveable = true;
                if (Fox.Exists && !Fox.isFoxAlive())
                {
                    foreach (var immoralist in Immoralist.AllPlayers)
                    {
                        immoralist.MurderPlayerQuick(immoralist);
                    }
                }
            }
            Jackal.canCreateSidekick = false;
        }

        public static void sidekickPromotes()
        {
            Jackal.removeCurrentJackal();
            Jackal.jackal = Sidekick.sidekick;
            Jackal.canCreateSidekick = Jackal.jackalPromotedFromSidekickCanCreateSidekick;
            Sidekick.ClearAndReload();
            return;
        }

        public static void erasePlayerRoles(byte playerId, bool ignoreLovers = false)
        {
            PlayerControl player = Helpers.PlayerById(playerId);
            if (player == null) return;

            // Don't give a former neutral role tasks because that destroys the balance.
            if (player.IsNeutral())
                player.ClearAllTasks();

            player.EraseAllRoles();
            player.eraseAllModifiers();

            if (!ignoreLovers && player.IsInLove())
            { // The whole Lover couple is being erased
                Lovers.EraseCouple(player);
            }
        }

        public static void setFutureErased(byte playerId)
        {
            PlayerControl player = Helpers.PlayerById(playerId);
            if (Eraser.futureErased == null)
                Eraser.futureErased = new List<PlayerControl>();
            if (player != null)
            {
                Eraser.futureErased.Add(player);
            }
        }

        public static void setFutureShifted(byte playerId)
        {
            if (Shifter.isNeutral && !Shifter.shiftPastShifters && Shifter.pastShifters.Contains(playerId))
                return;
            Shifter.futureShift = Helpers.PlayerById(playerId);
        }

        public static void setFutureShielded(byte playerId)
        {
            Medic.futureShielded = Helpers.PlayerById(playerId);
            Medic.usedShield = true;
        }

        public static void setFutureSpelled(byte playerId)
        {
            PlayerControl player = Helpers.PlayerById(playerId);
            if (Witch.futureSpelled == null)
                Witch.futureSpelled = new List<PlayerControl>();
            if (player != null)
            {
                Witch.futureSpelled.Add(player);
            }
        }


        public static void placeJackInTheBox(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new JackInTheBox(position);
        }

        public static void lightsOut()
        {
            Trickster.lightsOutTimer = Trickster.lightsOutDuration;
            // If the local player is impostor indicate lights out
            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                new CustomMessage("tricksterLightsOutText", Trickster.lightsOutDuration);
            }
        }

        public static void placeCamera(byte[] buff, byte roomId)
        {
            var referenceCamera = UnityEngine.Object.FindObjectOfType<SurvCamera>();
            if (referenceCamera == null) return; // Mira HQ

            SecurityGuard.remainingScrews -= SecurityGuard.camPrice;
            SecurityGuard.placedCameras++;

            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));

            SystemTypes roomType = (SystemTypes)roomId;

            var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera {SecurityGuard.placedCameras}";
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);

            switch (roomType)
            {
                case SystemTypes.Hallway: camera.NewName = StringNames.Hallway; break;
                case SystemTypes.Storage: camera.NewName = StringNames.Storage; break;
                case SystemTypes.Cafeteria: camera.NewName = StringNames.Cafeteria; break;
                case SystemTypes.Reactor: camera.NewName = StringNames.Reactor; break;
                case SystemTypes.UpperEngine: camera.NewName = StringNames.UpperEngine; break;
                case SystemTypes.Nav: camera.NewName = StringNames.Nav; break;
                case SystemTypes.Admin: camera.NewName = StringNames.Admin; break;
                case SystemTypes.Electrical: camera.NewName = StringNames.Electrical; break;
                case SystemTypes.LifeSupp: camera.NewName = StringNames.LifeSupp; break;
                case SystemTypes.Shields: camera.NewName = StringNames.Shields; break;
                case SystemTypes.MedBay: camera.NewName = StringNames.MedBay; break;
                case SystemTypes.Security: camera.NewName = StringNames.Security; break;
                case SystemTypes.Weapons: camera.NewName = StringNames.Weapons; break;
                case SystemTypes.LowerEngine: camera.NewName = StringNames.LowerEngine; break;
                case SystemTypes.Comms: camera.NewName = StringNames.Comms; break;
                case SystemTypes.Decontamination: camera.NewName = StringNames.Decontamination; break;
                case SystemTypes.Launchpad: camera.NewName = StringNames.Launchpad; break;
                case SystemTypes.LockerRoom: camera.NewName = StringNames.LockerRoom; break;
                case SystemTypes.Laboratory: camera.NewName = StringNames.Laboratory; break;
                case SystemTypes.Balcony: camera.NewName = StringNames.Balcony; break;
                case SystemTypes.Office: camera.NewName = StringNames.Office; break;
                case SystemTypes.Greenhouse: camera.NewName = StringNames.Greenhouse; break;
                case SystemTypes.Dropship: camera.NewName = StringNames.Dropship; break;
                case SystemTypes.Decontamination2: camera.NewName = StringNames.Decontamination2; break;
                case SystemTypes.Outside: camera.NewName = StringNames.Outside; break;
                case SystemTypes.Specimens: camera.NewName = StringNames.Specimens; break;
                case SystemTypes.BoilerRoom: camera.NewName = StringNames.BoilerRoom; break;
                case SystemTypes.VaultRoom: camera.NewName = StringNames.VaultRoom; break;
                case SystemTypes.Cockpit: camera.NewName = StringNames.Cockpit; break;
                case SystemTypes.Armory: camera.NewName = StringNames.Armory; break;
                case SystemTypes.Kitchen: camera.NewName = StringNames.Kitchen; break;
                case SystemTypes.ViewingDeck: camera.NewName = StringNames.ViewingDeck; break;
                case SystemTypes.HallOfPortraits: camera.NewName = StringNames.HallOfPortraits; break;
                case SystemTypes.CargoBay: camera.NewName = StringNames.CargoBay; break;
                case SystemTypes.Ventilation: camera.NewName = StringNames.Ventilation; break;
                case SystemTypes.Showers: camera.NewName = StringNames.Showers; break;
                case SystemTypes.Engine: camera.NewName = StringNames.Engine; break;
                case SystemTypes.Brig: camera.NewName = StringNames.Brig; break;
                case SystemTypes.MeetingRoom: camera.NewName = StringNames.MeetingRoom; break;
                case SystemTypes.Records: camera.NewName = StringNames.Records; break;
                case SystemTypes.Lounge: camera.NewName = StringNames.Lounge; break;
                case SystemTypes.GapRoom: camera.NewName = StringNames.GapRoom; break;
                case SystemTypes.MainHall: camera.NewName = StringNames.MainHall; break;
                case SystemTypes.Medical: camera.NewName = StringNames.Medical; break;
                default: camera.NewName = StringNames.ExitButton; break;
            }

            if (GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().MapId == 2 || GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().MapId == 4) camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 

            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard)
            {
                camera.gameObject.SetActive(true);
                camera.gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                camera.gameObject.SetActive(false);
            }
            MapOptions.CamerasToAdd.Add(camera);
        }

        public static void sealVent(int ventId)
        {
            Vent vent = ShipStatus.Instance.AllVents.FirstOrDefault((x) => x != null && x.Id == ventId);
            if (vent == null) return;

            SecurityGuard.remainingScrews -= SecurityGuard.ventPrice;
            if (PlayerControl.LocalPlayer == SecurityGuard.securityGuard)
            {
                PowerTools.SpriteAnim animator = vent.GetComponent<PowerTools.SpriteAnim>();
                animator?.Stop();
                vent.EnterVentAnim = vent.ExitVentAnim = null;
                vent.myRend.sprite = animator == null ? SecurityGuard.getStaticVentSealedSprite() : SecurityGuard.getAnimatedVentSealedSprite();
                vent.myRend.color = new Color(1f, 1f, 1f, 0.5f);
                vent.name = "FutureSealedVent_" + vent.name;
            }

            MapOptions.VentsToSeal.Add(vent);
        }

        public static void arsonistDouse(byte playerId)
        {
            Arsonist.dousedPlayers.Add(Helpers.PlayerById(playerId));
        }

        public static void arsonistWin()
        {
            Arsonist.triggerArsonistWin = true;
            var livingPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.IsRole(RoleType.Arsonist) && p.IsAlive());
            foreach (PlayerControl p in livingPlayers)
            {
                p.Exiled();
                FinalStatuses[p.PlayerId] = FinalStatus.Torched;
            }
        }

        public static void vultureEat(byte playerId)
        {
            CleanBody(playerId);
            Vulture.eatenBodies++;
        }

        public static void vultureWin()
        {
            Vulture.triggerVultureWin = true;
        }

        public static void lawyerWin()
        {
            Lawyer.triggerLawyerWin = true;
        }

        public static void lawyerSetTarget(byte playerId)
        {
            Lawyer.target = Helpers.PlayerById(playerId);
        }

        public static void lawyerPromotesToPursuer()
        {
            PlayerControl player = Lawyer.lawyer;
            PlayerControl client = Lawyer.target;
            Lawyer.ClearAndReload();
            Pursuer.pursuer = player;

            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId && client != null)
            {
                Transform playerInfoTransform = client.cosmetics.nameText.transform.parent.FindChild("Info");
                TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                if (playerInfo != null) playerInfo.text = "";
            }
        }

        public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
        {
            PlayerControl killer = Helpers.PlayerById(killerId);
            PlayerControl dyingTarget = Helpers.PlayerById(dyingTargetId);
            if (dyingTarget == null) return;
            if (dyingTarget.IsRole(RoleType.NekoKabocha))
            {
                NekoKabocha.meetingKill(dyingTarget, killer);
            }
            dyingTarget.Exiled();
            PlayerControl dyingLoverPartner = Lovers.BothDie ? dyingTarget.GetPartner() : null; // Lover check

            Guesser.remainingShots(killerId, true);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);

            PlayerControl guesser = Helpers.PlayerById(killerId);
            if (HudManager.Instance != null && guesser != null)
                if (PlayerControl.LocalPlayer == dyingTarget)
                    HudManager.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                else if (dyingLoverPartner != null && PlayerControl.LocalPlayer == dyingLoverPartner)
                    HudManager.Instance.KillOverlay.ShowKillAnimation(dyingLoverPartner.Data, dyingLoverPartner.Data);

            PlayerControl guessedTarget = Helpers.PlayerById(guessedTargetId);
            if (Guesser.showInfoInGhostChat && PlayerControl.LocalPlayer.Data.IsDead && guessedTarget != null)
            {
                RoleInfo roleInfo = RoleInfo.AllRoleInfos.FirstOrDefault(x => (byte)x.MyRoleType == guessedRoleId);
                string msg = string.Format(ModTranslation.GetString("guesserGuessChat"), roleInfo.Name, guessedTarget.Data.PlayerName);
                if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(guesser, msg);
                if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                    DestroyableSingleton<Assets.CoreScripts.UnityTelemetry>.Instance.SendWho();
            }
        }

        public static void setBlanked(byte playerId, byte value)
        {
            PlayerControl target = Helpers.PlayerById(playerId);
            if (target == null) return;
            Pursuer.blankedList.RemoveAll(x => x.PlayerId == playerId);
            if (value > 0) Pursuer.blankedList.Add(target);
        }

        public static void witchSpellCast(byte playerId)
        {
            UncheckedExilePlayer(playerId);
            FinalStatuses[playerId] = FinalStatus.Spelled;
        }

        public static void setShifterType(bool isNeutral)
        {
            Shifter.isNeutral = isNeutral;
        }

        public static void ninjaStealth(byte playerId, bool stealthed)
        {
            PlayerControl player = Helpers.PlayerById(playerId);
            Ninja.setStealthed(player, stealthed);
        }
        public static void foxStealth(byte playerId, bool stealthed)
        {
            PlayerControl player = Helpers.PlayerById(playerId);
            Fox.setStealthed(player, stealthed);
        }

        public static void foxCreatesImmoralist(byte targetId)
        {
            PlayerControl player = Helpers.PlayerById(targetId);
            DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            erasePlayerRoles(player.PlayerId, true);
            player.SetRole(RoleType.Immoralist);
            player.ClearAllTasks();
        }

        public static void GMKill(byte targetId)
        {
            PlayerControl target = Helpers.PlayerById(targetId);

            if (target == null) return;
            target.MyPhysics.ExitAllVents();
            target.Exiled();
            FinalStatuses[target.PlayerId] = FinalStatus.GMExecuted;

            PlayerControl partner = target.GetPartner(); // Lover check
            if (partner != null)
            {
                partner?.MyPhysics.ExitAllVents();
                FinalStatuses[partner.PlayerId] = FinalStatus.GMExecuted;
            }

            if (HudManager.Instance != null && GM.gm != null)
            {
                if (PlayerControl.LocalPlayer == target)
                    HudManager.Instance.KillOverlay.ShowKillAnimation(GM.gm.Data, target.Data);
                else if (partner != null && PlayerControl.LocalPlayer == partner)
                    HudManager.Instance.KillOverlay.ShowKillAnimation(GM.gm.Data, partner.Data);
            }
        }

        public static void GMRevive(byte targetId)
        {
            PlayerControl target = Helpers.PlayerById(targetId);
            if (target == null) return;
            target.Revive();
            UpdateMeeting(targetId, false);
            FinalStatuses[target.PlayerId] = FinalStatus.Alive;

            PlayerControl partner = target.GetPartner(); // Lover check
            if (partner != null)
            {
                partner.Revive();
                UpdateMeeting(partner.PlayerId, false);
                FinalStatuses[partner.PlayerId] = FinalStatus.Alive;
            }

            if (PlayerControl.LocalPlayer.IsGM())
            {
                HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
            }
        }

        public static void UpdateMeeting(byte targetId, bool dead = true)
        {
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == targetId)
                    {
                        pva.SetDead(pva.DidReport, dead);
                        pva.Overlay.gameObject.SetActive(dead);
                    }

                    // Give players back their vote if target is shot dead
                    if (Helpers.RefundVotes && dead)
                    {
                        if (pva.VotedFor != targetId) continue;
                        pva.UnsetVote();
                        var voteAreaPlayer = Helpers.PlayerById(pva.TargetPlayerId);
                        if (!voteAreaPlayer.AmOwner) continue;
                        MeetingHud.Instance.ClearVote();
                    }
                }

                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }
        }

        public static void useAdminTime(float time)
        {
            MapOptions.RestrictAdminTime -= time;
        }

        public static void useCameraTime(float time)
        {
            MapOptions.RestrictCamerasTime -= time;
        }

        public static void useVitalsTime(float time)
        {
            MapOptions.RestrictVitalsTime -= time;
        }

        public static void plagueDoctorWin()
        {
            PlagueDoctor.triggerPlagueDoctorWin = true;
            var livingPlayers = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.IsRole(RoleType.PlagueDoctor) && p.IsAlive());
            foreach (PlayerControl p in livingPlayers)
            {
                // Check again so we don't re-kill any lovers
                if (p.IsAlive())
                    p.Exiled();
                FinalStatuses[p.PlayerId] = FinalStatus.Diseased;
            }
        }

        public static void plagueDoctorInfected(byte targetId)
        {
            var p = Helpers.PlayerById(targetId);
            if (!PlagueDoctor.infected.ContainsKey(targetId))
            {
                PlagueDoctor.infected[targetId] = p;
            }
        }

        public static void plagueDoctorProgress(byte targetId, float progress)
        {
            PlagueDoctor.progress[targetId] = progress;
        }

        public static void nekoKabochaExile(byte playerId)
        {
            UncheckedExilePlayer(playerId);
            FinalStatuses[playerId] = FinalStatus.Revenge;
        }

        public static void serialKillerSuicide(byte serialKillerId)
        {
            PlayerControl serialKiller = Helpers.PlayerById(serialKillerId);
            if (serialKiller == null) return;
            serialKiller.MurderPlayerQuick(serialKiller);
        }

        public static void fortuneTellerUsedDivine(byte fortuneTellerId, byte targetId)
        {
            PlayerControl fortuneTeller = Helpers.PlayerById(fortuneTellerId);
            PlayerControl target = Helpers.PlayerById(targetId);
            if (target == null) return;
            if (target.IsDead()) return;
            // 呪殺
            if (target.IsRole(RoleType.Fox))
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                if (PlayerControl.LocalPlayer.IsRole(RoleType.FortuneTeller))
                {
                    // 狐を殺せたことを分からなくするためにキル音を鳴らさないための処置
                    target.MurderPlayerQuick(target);
                }
                else
                {
                    fortuneTeller.MurderPlayerQuick(target);
                }
                FinalStatuses[targetId] = FinalStatus.Divined;
            }

            // インポスターの場合は占い師の位置に矢印を表示
            if (PlayerControl.LocalPlayer.IsImpostor())
            {
                FortuneTeller.fortuneTellerMessage(ModTranslation.GetString("fortuneTellerDivinedSomeone"), 5f, Color.white);
                FortuneTeller.setDivinedFlag(fortuneTeller, true);
            }

            // 占われたのが背徳者の場合は通知を表示
            if (target.IsRole(RoleType.Immoralist) && target == PlayerControl.LocalPlayer)
            {
                FortuneTeller.fortuneTellerMessage(ModTranslation.GetString("fortuneTellerDivinedYou"), 5f, Color.white);
            }
        }



        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch
        {
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                byte packetId = callId;
                switch (packetId)
                {

                    // Main Controls

                    case (byte)CustomRpc.ResetVaribles:
                        RpcProcedure.ResetVariables();
                        break;
                    case (byte)CustomRpc.ShareOptions:
                        RpcProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                        break;
                    case (byte)CustomRpc.ForceEnd:
                        RpcProcedure.ForceEnd();
                        break;
                    case (byte)CustomRpc.SetRole:
                        byte roleId = reader.ReadByte();
                        byte playerId = reader.ReadByte();
                        byte flag = reader.ReadByte();
                        RpcProcedure.SetRole(roleId, playerId, flag);
                        break;
                    case (byte)CustomRpc.SetLovers:
                        RpcProcedure.SetLovers(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRpc.OverrideNativeRole:
                        RpcProcedure.OverrideNativeRole(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRpc.VersionHandshake:
                        int major = reader.ReadPackedInt32();
                        int minor = reader.ReadPackedInt32();
                        int patch = reader.ReadPackedInt32();
                        int versionOwnerId = reader.ReadPackedInt32();
                        byte revision = 0xFF;
                        Guid guid;
                        if (reader.Length - reader.Position >= 17)
                        { // enough bytes left to read
                            revision = reader.ReadByte();
                            // GUID
                            byte[] gbytes = reader.ReadBytes(16);
                            guid = new Guid(gbytes);
                        }
                        else
                        {
                            guid = new Guid(new byte[16]);
                        }
                        RpcProcedure.VersionHandshake(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                        break;
                    case (byte)CustomRpc.UseUncheckedVent:
                        int ventId = reader.ReadPackedInt32();
                        byte ventingPlayer = reader.ReadByte();
                        byte isEnter = reader.ReadByte();
                        RpcProcedure.UseUncheckedVent(ventId, ventingPlayer, isEnter);
                        break;
                    case (byte)CustomRpc.UncheckedMurderPlayer:
                        byte source = reader.ReadByte();
                        byte target = reader.ReadByte();
                        byte showAnimation = reader.ReadByte();
                        RpcProcedure.UncheckedMurderPlayer(source, target, showAnimation);
                        break;
                    case (byte)CustomRpc.UncheckedExilePlayer:
                        byte exileTarget = reader.ReadByte();
                        RpcProcedure.UncheckedExilePlayer(exileTarget);
                        break;
                    case (byte)CustomRpc.UncheckedCmdReportDeadBody:
                        byte reportSource = reader.ReadByte();
                        byte reportTarget = reader.ReadByte();
                        RpcProcedure.UncheckedCmdReportDeadBody(reportSource, reportTarget);
                        break;
                    case (byte)CustomRpc.UncheckedEndGame:
                        RpcProcedure.uncheckedEndGame(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.UncheckedSetTasks:
                        RpcProcedure.uncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                        break;
                    case (byte)CustomRpc.DynamicMapOption:
                        byte mapId = reader.ReadByte();
                        RpcProcedure.DynamicMapOption(mapId);
                        break;

                    // Role functionality

                    case (byte)CustomRpc.EngineerFixLights:
                        RpcProcedure.EngineerFixLights();
                        break;
                    case (byte)CustomRpc.EngineerUsedRepair:
                        RpcProcedure.EngineerUsedRepair();
                        break;
                    case (byte)CustomRpc.CleanBody:
                        RpcProcedure.CleanBody(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SheriffKill:
                        RpcProcedure.SheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case (byte)CustomRpc.TimeMasterRewindTime:
                        RpcProcedure.TimeMasterRewindTime();
                        break;
                    case (byte)CustomRpc.TimeMasterShield:
                        RpcProcedure.timeMasterShield();
                        break;
                    case (byte)CustomRpc.MedicSetShielded:
                        RpcProcedure.medicSetShielded(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.ShieldedMurderAttempt:
                        RpcProcedure.shieldedMurderAttempt();
                        break;
                    case (byte)CustomRpc.ShifterShift:
                        RpcProcedure.shifterShift(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SwapperSwap:
                        byte playerId1 = reader.ReadByte();
                        byte playerId2 = reader.ReadByte();
                        RpcProcedure.swapperSwap(playerId1, playerId2);
                        break;
                    case (byte)CustomRpc.MorphlingMorph:
                        RpcProcedure.morphlingMorph(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.CamouflagerCamouflage:
                        RpcProcedure.camouflagerCamouflage();
                        break;
                    case (byte)CustomRpc.VampireSetBitten:
                        byte bittenId = reader.ReadByte();
                        byte reset = reader.ReadByte();
                        RpcProcedure.vampireSetBitten(bittenId, reset);
                        break;
                    case (byte)CustomRpc.PlaceGarlic:
                        RpcProcedure.placeGarlic(reader.ReadBytesAndSize());
                        break;
                    case (byte)CustomRpc.TrackerUsedTracker:
                        RpcProcedure.trackerUsedTracker(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.JackalCreatesSidekick:
                        RpcProcedure.jackalCreatesSidekick(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SidekickPromotes:
                        RpcProcedure.sidekickPromotes();
                        break;
                    case (byte)CustomRpc.ErasePlayerRoles:
                        RpcProcedure.erasePlayerRoles(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SetFutureErased:
                        RpcProcedure.setFutureErased(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SetFutureShifted:
                        RpcProcedure.setFutureShifted(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SetFutureShielded:
                        RpcProcedure.setFutureShielded(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.PlaceJackInTheBox:
                        RpcProcedure.placeJackInTheBox(reader.ReadBytesAndSize());
                        break;
                    case (byte)CustomRpc.LightsOut:
                        RpcProcedure.lightsOut();
                        break;
                    case (byte)CustomRpc.PlaceCamera:
                        RpcProcedure.placeCamera(reader.ReadBytesAndSize(), reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SealVent:
                        RpcProcedure.sealVent(reader.ReadPackedInt32());
                        break;
                    case (byte)CustomRpc.ArsonistWin:
                        RpcProcedure.arsonistWin();
                        break;
                    case (byte)CustomRpc.GuesserShoot:
                        byte killerId = reader.ReadByte();
                        byte dyingTarget = reader.ReadByte();
                        byte guessedTarget = reader.ReadByte();
                        byte guessedRoleId = reader.ReadByte();
                        RpcProcedure.guesserShoot(killerId, dyingTarget, guessedTarget, guessedRoleId);
                        break;
                    case (byte)CustomRpc.VultureWin:
                        RpcProcedure.vultureWin();
                        break;
                    case (byte)CustomRpc.LawyerWin:
                        RpcProcedure.lawyerWin();
                        break;
                    case (byte)CustomRpc.LawyerSetTarget:
                        RpcProcedure.lawyerSetTarget(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.LawyerPromotesToPursuer:
                        RpcProcedure.lawyerPromotesToPursuer();
                        break;
                    case (byte)CustomRpc.SetBlanked:
                        var pid = reader.ReadByte();
                        var blankedValue = reader.ReadByte();
                        RpcProcedure.setBlanked(pid, blankedValue);
                        break;
                    case (byte)CustomRpc.SetFutureSpelled:
                        RpcProcedure.setFutureSpelled(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.WitchSpellCast:
                        RpcProcedure.witchSpellCast(reader.ReadByte());
                        break;

                    // GM functionality
                    case (byte)CustomRpc.AddModifier:
                        RpcProcedure.AddModifier(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SetShifterType:
                        RpcProcedure.setShifterType(reader.ReadBoolean());
                        break;
                    case (byte)CustomRpc.NinjaStealth:
                        RpcProcedure.ninjaStealth(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case (byte)CustomRpc.ArsonistDouse:
                        RpcProcedure.arsonistDouse(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.VultureEat:
                        RpcProcedure.vultureEat(reader.ReadByte());
                        break;

                    case (byte)CustomRpc.GMKill:
                        RpcProcedure.GMKill(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.GMRevive:
                        RpcProcedure.GMRevive(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.UseAdminTime:
                        RpcProcedure.useAdminTime(reader.ReadSingle());
                        break;
                    case (byte)CustomRpc.UseCameraTime:
                        RpcProcedure.useCameraTime(reader.ReadSingle());
                        break;
                    case (byte)CustomRpc.UseVitalsTime:
                        RpcProcedure.useVitalsTime(reader.ReadSingle());
                        break;
                    case (byte)CustomRpc.PlagueDoctorWin:
                        RpcProcedure.plagueDoctorWin();
                        break;
                    case (byte)CustomRpc.PlagueDoctorSetInfected:
                        RpcProcedure.plagueDoctorInfected(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.PlagueDoctorUpdateProgress:
                        byte progressTarget = reader.ReadByte();
                        byte[] progressByte = reader.ReadBytes(4);
                        float progress = System.BitConverter.ToSingle(progressByte, 0);
                        RpcProcedure.plagueDoctorProgress(progressTarget, progress);
                        break;
                    case (byte)CustomRpc.NekoKabochaExile:
                        RpcProcedure.nekoKabochaExile(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SerialKillerSuicide:
                        RpcProcedure.serialKillerSuicide(reader.ReadByte());
                        break;
                    case (byte)CustomRpc.SwapperAnimate:
                        RpcProcedure.swapperAnimate();
                        break;
                    case (byte)CustomRpc.FortuneTellerUsedDivine:
                        byte fId = reader.ReadByte();
                        byte tId = reader.ReadByte();
                        RpcProcedure.fortuneTellerUsedDivine(fId, tId);
                        break;
                    case (byte)CustomRpc.FoxStealth:
                        RpcProcedure.foxStealth(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case (byte)CustomRpc.FoxCreatesImmoralist:
                        RpcProcedure.foxCreatesImmoralist(reader.ReadByte());
                        break;
                }

                CustomRole.AllRoles.ForEach(r => r.OnRpcReceived(callId, reader));

                if ((CustomRpc)callId == CustomRpc.RoleDataSync) // For readability, i dont put it in switch-case block
                {
                    var roleType = (RoleType)reader.ReadPackedInt32();
                    CustomRole.AllRoles.First(r => r.MyRoleType == roleType).OnRoleDataBeingSynchronized(reader);
                }
            }
        }
    }
}