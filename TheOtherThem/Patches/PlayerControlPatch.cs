﻿using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using static TheOtherThem.GameHistory;
using TheOtherThem.Objects;
using UnityEngine;
using AmongUs.GameOptions;
using Assets.CoreScripts;

namespace TheOtherThem.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class PlayerControlFixedUpdatePatch
    {
        // Helpers

        public static PlayerControl setTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameManager.Instance.LogicOptions.GetKillDistance();
            if (!ShipStatus.Instance) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.IsDead() || targetingPlayer.inVent) return result;
            if (targetingPlayer.IsGM()) return result;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new List<PlayerControl>();
            }

            // GM is untargetable by anything
            if (GM.gm != null)
            {
                untargetablePlayers.Add(GM.gm);
            }

            // Can't target stealthed ninjas if setting on
            if (!Ninja.canBeTargeted)
            {
                foreach (Ninja n in Ninja.Players)
                {
                    if (n.stealthed) untargetablePlayers.Add(n.Player);
                }
            }

            // Can't target stealthed Fox
            foreach (Fox f in Fox.Players)
            {
                if (f.stealthed) untargetablePlayers.Add(f.Player);
            }


            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                NetworkedPlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }

        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) return;

            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
            target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", color);
        }

        // Update functions

        static void setBasePlayerOutlines()
        {
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.cosmetics.currentBodySprite.BodySprite == null) continue;

                bool isMorphedMorphling = target == Morphling.morphling && Morphling.morphTarget != null && Morphling.morphTimer > 0f;
                bool hasVisibleShield = false;
                if (Camouflager.camouflageTimer <= 0f && Medic.shielded != null && ((target == Medic.shielded && !isMorphedMorphling) || (isMorphedMorphling && Morphling.morphTarget == Medic.shielded)))
                {
                    hasVisibleShield = Medic.showShielded == 0 // Everyone
                        || (Medic.showShielded == 1 && (PlayerControl.LocalPlayer == Medic.shielded || PlayerControl.LocalPlayer == Medic.medic)) // Shielded + Medic
                        || (Medic.showShielded == 2 && PlayerControl.LocalPlayer == Medic.medic); // Medic only
                }

                if (hasVisibleShield)
                {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 1f);
                    target.cosmetics.currentBodySprite.BodySprite.material.SetColor("_OutlineColor", Medic.shieldedColor);
                }
                else
                {
                    target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
                }
            }
        }

        public static void bendTimeUpdate()
        {
            if (TimeMaster.isRewinding)
            {
                if (localPlayerPositions.Count > 0)
                {
                    // Set position
                    var next = localPlayerPositions[0];
                    if (next.Item2 == true)
                    {
                        // Exit current vent if necessary
                        if (PlayerControl.LocalPlayer.inVent)
                        {
                            foreach (Vent vent in ShipStatus.Instance.AllVents)
                            {
                                bool canUse;
                                bool couldUse;
                                vent.CanUse(PlayerControl.LocalPlayer.Data, out canUse, out couldUse);
                                if (canUse)
                                {
                                    PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(vent.Id);
                                    vent.SetButtons(false);
                                }
                            }
                        }
                        // Set position
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    }
                    else if (localPlayerPositions.Any(x => x.Item2 == true))
                    {
                        PlayerControl.LocalPlayer.transform.position = next.Item1;
                    }

                    localPlayerPositions.RemoveAt(0);

                    if (localPlayerPositions.Count > 1) localPlayerPositions.RemoveAt(0); // Skip every second position to rewinde twice as fast, but never skip the last position
                }
                else
                {
                    TimeMaster.isRewinding = false;
                    PlayerControl.LocalPlayer.moveable = true;
                }
            }
            else
            {
                while (localPlayerPositions.Count >= Mathf.Round(TimeMaster.rewindTime / Time.fixedDeltaTime)) localPlayerPositions.RemoveAt(localPlayerPositions.Count - 1);
                localPlayerPositions.Insert(0, new Tuple<Vector3, bool>(PlayerControl.LocalPlayer.transform.position, PlayerControl.LocalPlayer.CanMove)); // CanMove = CanMove
            }
        }

        static void medicSetTarget()
        {
            if (Medic.medic == null || Medic.medic != PlayerControl.LocalPlayer) return;
            Medic.currentTarget = setTarget();
            if (!Medic.usedShield) setPlayerOutline(Medic.currentTarget, Medic.shieldedColor);
        }

        static void shifterUpdate()
        {
            if (Shifter.shifter == null || Shifter.shifter != PlayerControl.LocalPlayer) return;

            List<PlayerControl> blockShift = null;
            if (Shifter.isNeutral && !Shifter.shiftPastShifters)
            {
                blockShift = new List<PlayerControl>();
                foreach (var playerId in Shifter.pastShifters)
                {
                    blockShift.Add(Helpers.PlayerById((byte)playerId));
                }
            }

            Shifter.currentTarget = setTarget(untargetablePlayers: blockShift);
            if (Shifter.futureShift == null) setPlayerOutline(Shifter.currentTarget, Shifter.color);
        }

        static void morphlingSetTarget()
        {
            if (Morphling.morphling == null || Morphling.morphling != PlayerControl.LocalPlayer) return;
            Morphling.currentTarget = setTarget();
            setPlayerOutline(Morphling.currentTarget, Morphling.color);
        }

        static void trackerSetTarget()
        {
            if (Tracker.tracker == null || Tracker.tracker != PlayerControl.LocalPlayer) return;
            Tracker.currentTarget = setTarget();
            if (!Tracker.usedTracker) setPlayerOutline(Tracker.currentTarget, Tracker.color);
        }

        static void detectiveUpdateFootPrints()
        {
            if (Detective.detective == null || Detective.detective != PlayerControl.LocalPlayer) return;

            Detective.timer -= Time.fixedDeltaTime;
            if (Detective.timer <= 0f)
            {
                Detective.timer = Detective.footprintIntervall;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player != PlayerControl.LocalPlayer && !player.Data.IsDead && !player.inVent && !player.IsGM())
                    {
                        new Footprint(Detective.footprintDuration, Detective.anonymousFootprints, player);
                    }
                }
            }
        }

        static void vampireSetTarget()
        {
            if (Vampire.vampire == null || Vampire.vampire != PlayerControl.LocalPlayer) return;

            PlayerControl target = null;
            if (Spy.spy != null)
            {
                if (Spy.impostorsCanKillAnyone)
                {
                    target = setTarget(false, true);
                }
                else
                {
                    target = setTarget(true, true, new List<PlayerControl>() { Spy.spy });
                }
            }
            else
            {
                target = setTarget(true, true);
            }

            bool targetNearGarlic = false;
            if (target != null)
            {
                foreach (Garlic garlic in Garlic.garlics)
                {
                    if (Vector2.Distance(garlic.garlic.transform.position, target.transform.position) <= 1.91f)
                    {
                        targetNearGarlic = true;
                    }
                }
            }
            Vampire.targetNearGarlic = targetNearGarlic;
            Vampire.currentTarget = target;
            setPlayerOutline(Vampire.currentTarget, Vampire.color);
        }

        static void jackalSetTarget()
        {
            if (Jackal.jackal == null || Jackal.jackal != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Jackal.canCreateSidekickFromImpostor)
            {
                // Only exclude sidekick from beeing targeted if the jackal can create sidekicks from impostors
                if (Sidekick.sidekick != null) untargetablePlayers.Add(Sidekick.sidekick);
            }
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Jackal from targeting the Mini unless it has grown up
            Jackal.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            setPlayerOutline(Jackal.currentTarget, Palette.ImpostorRed);
        }

        static void sidekickSetTarget()
        {
            if (Sidekick.sidekick == null || Sidekick.sidekick != PlayerControl.LocalPlayer) return;
            var untargetablePlayers = new List<PlayerControl>();
            if (Jackal.jackal != null) untargetablePlayers.Add(Jackal.jackal);
            if (Mini.mini != null && !Mini.isGrownUp()) untargetablePlayers.Add(Mini.mini); // Exclude Sidekick from targeting the Mini unless it has grown up
            Sidekick.currentTarget = setTarget(untargetablePlayers: untargetablePlayers);
            if (Sidekick.canKill) setPlayerOutline(Sidekick.currentTarget, Palette.ImpostorRed);
        }

        static void sidekickCheckPromotion()
        {
            // If LocalPlayer is Sidekick, the Jackal is disconnected and Sidekick promotion is enabled, then trigger promotion
            if (Sidekick.promotesToJackal && 
                PlayerControl.LocalPlayer.IsRole(RoleType.Sidekick) &&
                PlayerControl.LocalPlayer.IsAlive() && 
                (Jackal.jackal == null || Jackal.jackal.Data.Disconnected))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.sidekickPromotes();
            }
        }

        static void eraserSetTarget()
        {
            if (Eraser.eraser == null || Eraser.eraser != PlayerControl.LocalPlayer) return;

            List<PlayerControl> untargetables = new List<PlayerControl>();
            if (Spy.spy != null) untargetables.Add(Spy.spy);
            Eraser.currentTarget = setTarget(onlyCrewmates: !Eraser.canEraseAnyone, untargetablePlayers: Eraser.canEraseAnyone ? new List<PlayerControl>() : untargetables);
            setPlayerOutline(Eraser.currentTarget, Eraser.color);
        }

        static void engineerUpdate()
        {
            if (Engineer.engineer == null) return;

            bool jackalHighlight = Engineer.highlightForTeamJackal && (PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Sidekick.sidekick);
            bool impostorHighlight = Engineer.highlightForImpostors && PlayerControl.LocalPlayer.Data.Role.IsImpostor;
            if ((jackalHighlight || impostorHighlight) && ShipStatus.Instance?.AllVents != null)
            {
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    try
                    {
                        if (vent?.myRend?.material != null)
                        {
                            if (Engineer.engineer.inVent)
                            {
                                vent.myRend.material.SetFloat("_Outline", 1f);
                                vent.myRend.material.SetColor("_OutlineColor", Engineer.color);
                            }
                            else if (vent.myRend.material.GetColor("_AddColor").a == 0f)
                            {
                                vent.myRend.material.SetFloat("_Outline", 0);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        static void impostorSetTarget()
        {
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor || !PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.Data.IsDead)
            { // !isImpostor || !canMove || isDead
                HudManager.Instance.KillButton.SetTarget(null);
                return;
            }

            PlayerControl target = null;
            if (Spy.spy != null)
            {
                if (Spy.impostorsCanKillAnyone)
                {
                    target = setTarget(false, true);
                }
                else
                {
                    target = setTarget(true, true, new List<PlayerControl>() { Spy.spy });
                }
            }
            else
            {
                target = setTarget(true, true);
            }

            HudManager.Instance.KillButton.SetTarget(target); // Includes setPlayerOutline(target, Palette.ImpstorRed);
        }

        static void warlockSetTarget()
        {
            if (Warlock.warlock == null || Warlock.warlock != PlayerControl.LocalPlayer) return;
            if (Warlock.curseVictim != null && (Warlock.curseVictim.Data.Disconnected || Warlock.curseVictim.Data.IsDead))
            {
                // If the cursed victim is disconnected or dead reset the curse so a new curse can be applied
                Warlock.resetCurse();
            }
            if (Warlock.curseVictim == null)
            {
                Warlock.currentTarget = setTarget();
                setPlayerOutline(Warlock.currentTarget, Warlock.color);
            }
            else
            {
                Warlock.curseVictimTarget = setTarget(targetingPlayer: Warlock.curseVictim);
                setPlayerOutline(Warlock.curseVictimTarget, Warlock.color);
            }
        }

        static void trackerUpdate()
        {
            // Handle player tracking
            if (Tracker.arrow?.ArrowObject != null)
            {
                if (Tracker.tracker == null || PlayerControl.LocalPlayer != Tracker.tracker)
                {
                    Tracker.arrow.ArrowObject.SetActive(false);
                    return;
                }

                if (Tracker.tracker != null && Tracker.tracked != null && PlayerControl.LocalPlayer == Tracker.tracker && !Tracker.tracker.Data.IsDead)
                {
                    Tracker.timeUntilUpdate -= Time.fixedDeltaTime;

                    if (Tracker.timeUntilUpdate <= 0f)
                    {
                        bool trackedOnMap = !Tracker.tracked.Data.IsDead;
                        Vector3 position = Tracker.tracked.transform.position;
                        if (!trackedOnMap)
                        { // Check for dead body
                            DeadBody body = UnityEngine.Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == Tracker.tracked.PlayerId);
                            if (body != null)
                            {
                                trackedOnMap = true;
                                position = body.transform.position;
                            }
                        }

                        Tracker.arrow.Update(position);
                        Tracker.arrow.ArrowObject.SetActive(trackedOnMap);
                        Tracker.timeUntilUpdate = Tracker.updateIntervall;
                    }
                    else
                    {
                        Tracker.arrow.Update();
                    }
                }
            }

            // Handle corpses tracking
            if (Tracker.tracker != null && Tracker.tracker == PlayerControl.LocalPlayer && Tracker.corpsesTrackingTimer >= 0f && !Tracker.tracker.Data.IsDead)
            {
                bool arrowsCountChanged = Tracker.localArrows.Count != Tracker.DeadBodyPositions.Count();
                int index = 0;

                if (arrowsCountChanged)
                {
                    foreach (Arrow arrow in Tracker.localArrows) UnityEngine.Object.Destroy(arrow.ArrowObject);
                    Tracker.localArrows = new List<Arrow>();
                }
                foreach (Vector3 position in Tracker.DeadBodyPositions)
                {
                    if (arrowsCountChanged)
                    {
                        Tracker.localArrows.Add(new Arrow(Tracker.color));
                        Tracker.localArrows[index].ArrowObject.SetActive(true);
                    }
                    if (Tracker.localArrows[index] != null) Tracker.localArrows[index].Update(position);
                    index++;
                }
            }
            else if (Tracker.localArrows.Count > 0)
            {
                foreach (Arrow arrow in Tracker.localArrows) UnityEngine.Object.Destroy(arrow.ArrowObject);
                Tracker.localArrows = new List<Arrow>();
            }
        }

        public static void playerSizeUpdate(PlayerControl p)
        {
            // Set default player size
            CircleCollider2D collider = p.GetComponent<CircleCollider2D>();

            p.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            collider.radius = Mini.defaultColliderRadius;
            collider.offset = Mini.defaultColliderOffset * Vector2.down;

            // Set adapted player size to Mini and Morphling
            if (Mini.mini == null || Camouflager.camouflageTimer > 0f) return;

            float growingProgress = Mini.growingProgress();
            float scale = growingProgress * 0.35f + 0.35f;
            float correctedColliderRadius = Mini.defaultColliderRadius * 0.7f / scale; // scale / 0.7f is the factor by which we decrease the player size, hence we need to increase the collider size by 0.7f / scale

            if (p == Mini.mini)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
            if (Morphling.morphling != null && p == Morphling.morphling && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f)
            {
                p.transform.localScale = new Vector3(scale, scale, 1f);
                collider.radius = correctedColliderRadius;
            }
        }

        public static void updatePlayerInfo()
        {
            bool commsActive = false;
            foreach (PlayerTask t in PlayerControl.LocalPlayer.myTasks)
            {
                if (t.TaskType == TaskTypes.FixComms)
                {
                    commsActive = true;
                    break;
                }
            }

            var canSeeEverything = PlayerControl.LocalPlayer.IsDead() || PlayerControl.LocalPlayer.IsGM();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p == null) continue;

                var canSeeInfo =
                    canSeeEverything ||
                    p == PlayerControl.LocalPlayer || p.IsGM() || 
                    (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target);

                if (canSeeInfo)
                {
                    Transform playerInfoTransform = p.cosmetics.nameText.transform.parent.FindChild("Info");
                    TMPro.TextMeshPro playerInfo = playerInfoTransform != null ? playerInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (playerInfo == null)
                    {
                        playerInfo = UnityEngine.Object.Instantiate(p.cosmetics.nameText, p.cosmetics.nameText.transform.parent);
                        playerInfo.fontSize *= 0.75f;
                        playerInfo.gameObject.name = "Info";
                    }

                    // Set the position every time bc it sometimes ends up in the wrong place due to camoflauge
                    playerInfo.transform.localPosition = p.cosmetics.nameText.transform.localPosition + Vector3.up * 0.5f;

                    PlayerVoteArea playerVoteArea = MeetingHud.Instance?.playerStates?.FirstOrDefault(x => x.TargetPlayerId == p.PlayerId);
                    Transform meetingInfoTransform = playerVoteArea != null ? playerVoteArea.NameText.transform.parent.FindChild("Info") : null;
                    TMPro.TextMeshPro meetingInfo = meetingInfoTransform != null ? meetingInfoTransform.GetComponent<TMPro.TextMeshPro>() : null;
                    if (meetingInfo == null && playerVoteArea != null)
                    {
                        meetingInfo = UnityEngine.Object.Instantiate(playerVoteArea.NameText, playerVoteArea.NameText.transform.parent);
                        meetingInfo.transform.localPosition += Vector3.down * 0.10f;
                        meetingInfo.fontSize *= 0.60f;
                        meetingInfo.gameObject.name = "Info";
                    }

                    // Set player name higher to align in middle
                    if (meetingInfo != null && playerVoteArea != null) {
                        var playerName = playerVoteArea.NameText;
                        playerName.transform.localPosition = new Vector3(0.3384f, (0.0311f + 0.0683f), -0.1f);    
                    }

                    var (tasksCompleted, tasksTotal) = TaskHandler.GetTaskInfo(p.Data);
                    string roleNames = RoleInfo.GetRolesString(p, true, new RoleType[] { RoleType.Lovers });

                    var completedStr = commsActive ? "?" : tasksCompleted.ToString();
                    string taskInfo = tasksTotal > 0 ? $"<color=#FAD934FF>({completedStr}/{tasksTotal})</color>" : "";

                    string playerInfoText = "";
                    string meetingInfoText = "";
                    if (p == PlayerControl.LocalPlayer)
                    {
                        playerInfoText = $"{roleNames}";
                        if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
                        {
                            TMPro.TextMeshPro tabText = DestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>();
                            tabText.SetText($"{TranslationController.Instance.GetString(StringNames.Tasks)} {taskInfo}");
                        }
                        meetingInfoText = $"{roleNames} {taskInfo}".Trim();
                    }
                    else if (MapOptions.GhostsSeeRoles && MapOptions.GhostsSeeTasks)
                    {
                        playerInfoText = $"{roleNames} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (MapOptions.GhostsSeeTasks)
                    {
                        playerInfoText = $"{taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }
                    else if (MapOptions.GhostsSeeRoles || (Lawyer.lawyerKnowsRole && PlayerControl.LocalPlayer == Lawyer.lawyer && p == Lawyer.target))
                    {
                        playerInfoText = $"{roleNames}";
                        meetingInfoText = playerInfoText;
                    }
                    else if (p.IsGM() || PlayerControl.LocalPlayer.IsGM())
                    {
                        playerInfoText = $"{roleNames} {taskInfo}".Trim();
                        meetingInfoText = playerInfoText;
                    }

                    playerInfo.text = playerInfoText;
                    playerInfo.gameObject.SetActive(p.Visible && !Helpers.ShouldHidePlayerName(p));
                    if (meetingInfo != null) meetingInfo.text = MeetingHud.Instance.state == MeetingHud.VoteStates.Results ? "" : meetingInfoText;
                }
            }
        }

        public static void securityGuardSetTarget()
        {
            if (SecurityGuard.securityGuard == null || SecurityGuard.securityGuard != PlayerControl.LocalPlayer || ShipStatus.Instance == null || ShipStatus.Instance.AllVents == null) return;

            Vent target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            for (int i = 0; i < ShipStatus.Instance.AllVents.Length; i++)
            {
                Vent vent = ShipStatus.Instance.AllVents[i];
                if (vent.gameObject.name.StartsWith("JackInTheBoxVent_") || vent.gameObject.name.StartsWith("SealedVent_") || vent.gameObject.name.StartsWith("FutureSealedVent_")) continue;
                float distance = Vector2.Distance(vent.transform.position, truePosition);
                if (distance <= vent.UsableDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = vent;
                }
            }
            SecurityGuard.ventTarget = target;
        }

        public static void securityGuardUpdate() {
            if (SecurityGuard.securityGuard == null || PlayerControl.LocalPlayer != SecurityGuard.securityGuard || SecurityGuard.securityGuard.Data.IsDead) return;
            var (playerCompleted, _) = TaskHandler.GetTaskInfo(SecurityGuard.securityGuard.Data);
            if (playerCompleted == SecurityGuard.rechargedTasks) {
                SecurityGuard.rechargedTasks += SecurityGuard.rechargeTasksNumber;
                if (SecurityGuard.maxCharges > SecurityGuard.charges) SecurityGuard.charges++;
            }
        }

        public static void arsonistSetTarget()
        {
            if (Arsonist.arsonist == null || Arsonist.arsonist != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables;
            if (Arsonist.douseTarget != null)
                untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != Arsonist.douseTarget.PlayerId).ToList();
            else
                untargetables = Arsonist.dousedPlayers;
            Arsonist.currentTarget = setTarget(untargetablePlayers: untargetables);
            if (Arsonist.currentTarget != null) setPlayerOutline(Arsonist.currentTarget, Arsonist.color);
        }

        static void snitchUpdate()
        {
            if (Snitch.localArrows == null) return;

            foreach (Arrow arrow in Snitch.localArrows) arrow.ArrowObject.SetActive(false);

            if (Snitch.snitch == null || Snitch.snitch.Data.IsDead) return;

            var (playerCompleted, playerTotal) = TaskHandler.GetTaskInfo(Snitch.snitch.Data);
            int numberOfTasks = playerTotal - playerCompleted;

            if (numberOfTasks <= Snitch.taskCountForReveal && (PlayerControl.LocalPlayer.Data.Role.IsImpostor || (Snitch.includeTeamJackal && (PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Sidekick.sidekick))))
            {
                if (Snitch.localArrows.Count == 0) Snitch.localArrows.Add(new Arrow(Color.blue));
                if (Snitch.localArrows.Count != 0 && Snitch.localArrows[0] != null)
                {
                    Snitch.localArrows[0].ArrowObject.SetActive(true);
                    Snitch.localArrows[0].Image.color = Color.blue;
                    Snitch.localArrows[0].Update(Snitch.snitch.transform.position);
                }
            }
            else if (PlayerControl.LocalPlayer == Snitch.snitch && numberOfTasks == 0)
            {
                int arrowIndex = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    bool arrowForImp = p.Data.Role.IsImpostor;
                    bool arrowForTeamJackal = Snitch.includeTeamJackal && (p == Jackal.jackal || p == Sidekick.sidekick);
                    bool arrowForFox = p.IsRole(RoleType.Fox) || p.IsRole(RoleType.Immoralist);

                    // Update the arrows' color every time bc things go weird when you add a sidekick or someone dies
                    Color c = Palette.ImpostorRed;
                    if(arrowForTeamJackal){
                        c = Jackal.color;
                    }
                    else if(arrowForFox){
                        c = Fox.color;
                    }
                    if (!p.Data.IsDead && (arrowForImp || arrowForTeamJackal || arrowForFox))
                    {
                        if (arrowIndex >= Snitch.localArrows.Count)
                        {
                            Snitch.localArrows.Add(new Arrow(c));
                        }
                        if (arrowIndex < Snitch.localArrows.Count && Snitch.localArrows[arrowIndex] != null)
                        {
                            Snitch.localArrows[arrowIndex].Image.color = c;
                            Snitch.localArrows[arrowIndex].ArrowObject.SetActive(true);
                            Snitch.localArrows[arrowIndex].Update(p.transform.position, c);
                        }
                        arrowIndex++;
                    }
                }
            }
        }

        static void bountyHunterUpdate()
        {
            if (BountyHunter.bountyHunter == null || PlayerControl.LocalPlayer != BountyHunter.bountyHunter) return;

            if (BountyHunter.bountyHunter.Data.IsDead)
            {
                if (BountyHunter.arrow != null || BountyHunter.arrow.ArrowObject != null) UnityEngine.Object.Destroy(BountyHunter.arrow.ArrowObject);
                BountyHunter.arrow = null;
                if (BountyHunter.cooldownText != null && BountyHunter.cooldownText.gameObject != null) UnityEngine.Object.Destroy(BountyHunter.cooldownText.gameObject);
                BountyHunter.cooldownText = null;
                BountyHunter.bounty = null;
                foreach (PoolablePlayer p in MapOptions.PlayerIcons.Values)
                {
                    if (p != null && p.gameObject != null) p.gameObject.SetActive(false);
                }
                return;
            }

            BountyHunter.arrowUpdateTimer -= Time.fixedDeltaTime;
            BountyHunter.bountyUpdateTimer -= Time.fixedDeltaTime;

            if (BountyHunter.bounty == null || BountyHunter.bountyUpdateTimer <= 0f)
            {
                // Set new bounty
                BountyHunter.bounty = null;
                BountyHunter.arrowUpdateTimer = 0f; // Force arrow to update
                BountyHunter.bountyUpdateTimer = BountyHunter.bountyDuration;
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && !p.Data.Role.IsImpostor && p != Spy.spy && (p != Mini.mini || Mini.isGrownUp()) && !p.IsGM() && BountyHunter.bountyHunter.GetPartner() != p) possibleTargets.Add(p);
                }
                BountyHunter.bounty = possibleTargets[TheOtherRoles.rnd.Next(0, possibleTargets.Count)];
                if (BountyHunter.bounty == null) return;

                // Show poolable player
                if (HudManager.Instance != null && HudManager.Instance.UseButton != null)
                {
                    foreach (PoolablePlayer pp in MapOptions.PlayerIcons.Values) pp.gameObject.SetActive(false);
                    if (MapOptions.PlayerIcons.ContainsKey(BountyHunter.bounty.PlayerId) && MapOptions.PlayerIcons[BountyHunter.bounty.PlayerId].gameObject != null)
                        MapOptions.PlayerIcons[BountyHunter.bounty.PlayerId].gameObject.SetActive(true);
                }
            }

            // Update Cooldown Text
            if (BountyHunter.cooldownText != null)
            {
                BountyHunter.cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(BountyHunter.bountyUpdateTimer, 0, BountyHunter.bountyDuration)).ToString();
            }

            // Update Arrow
            if (BountyHunter.showArrow && BountyHunter.bounty != null)
            {
                if (BountyHunter.arrow == null) BountyHunter.arrow = new Arrow(Color.red);
                if (BountyHunter.arrowUpdateTimer <= 0f)
                {
                    BountyHunter.arrow.Update(BountyHunter.bounty.transform.position);
                    BountyHunter.arrowUpdateTimer = BountyHunter.arrowUpdateIntervall;
                }
                BountyHunter.arrow.Update();
            }
        }

        static void baitUpdate()
        {
            if (Bait.bait == null || Bait.bait != PlayerControl.LocalPlayer) return;

            // Bait report
            if (Bait.bait.Data.IsDead && !Bait.reported)
            {
                Bait.reportDelay -= Time.fixedDeltaTime;
                DeadPlayer deadPlayer = DeadPlayers?.Where(x => x.player?.PlayerId == Bait.bait.PlayerId)?.FirstOrDefault();
                if (deadPlayer.killerIfExisting != null && Bait.reportDelay <= 0f)
                {
                    Helpers.HandleVampireBiteOnBodyReport(); // Manually call Vampire handling, since the CmdReportDeadBody Prefix won't be called

                    byte reporter = deadPlayer.killerIfExisting.PlayerId;
                    if (Bait.bait.HasModifier(ModifierType.Madmate))
                    {
                        var candidates = PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsAlive() && !x.IsImpostor() && !x.isDummy).ToList();
                        int i = rnd.Next(0, candidates.Count);
                        reporter = candidates.Count > 0 ? candidates[i].PlayerId : deadPlayer.killerIfExisting.PlayerId;
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.UncheckedCmdReportDeadBody, Hazel.SendOption.Reliable, -1);
                    writer.Write(reporter);
                    writer.Write(Bait.bait.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.UncheckedCmdReportDeadBody(reporter, Bait.bait.PlayerId);
                    Bait.reported = true;
                }
            }

            // Bait Vents
            if (ShipStatus.Instance?.AllVents != null)
            {
                var ventsWithPlayers = new List<int>();
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == null) continue;

                    if (player.inVent)
                    {
                        Vent target = ShipStatus.Instance.AllVents.OrderBy(x => Vector2.Distance(x.transform.position, player.GetTruePosition())).FirstOrDefault();
                        if (target != null) ventsWithPlayers.Add(target.Id);
                    }
                }

                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    if (vent.myRend == null || vent.myRend.material == null) continue;
                    if (ventsWithPlayers.Contains(vent.Id) || (ventsWithPlayers.Count > 0 && Bait.highlightAllVents))
                    {
                        vent.myRend.material.SetFloat("_Outline", 1f);
                        vent.myRend.material.SetColor("_OutlineColor", Color.yellow);
                    }
                    else
                    {
                        vent.myRend.material.SetFloat("_Outline", 0);
                    }
                }
            }
        }

        static void vultureUpdate()
        {
            if (Vulture.vulture == null || PlayerControl.LocalPlayer != Vulture.vulture || Vulture.localArrows == null || !Vulture.showArrows) return;
            if (Vulture.vulture.Data.IsDead)
            {
                foreach (Arrow arrow in Vulture.localArrows) UnityEngine.Object.Destroy(arrow.ArrowObject);
                Vulture.localArrows = new List<Arrow>();
                return;
            }

            DeadBody[] deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            bool arrowUpdate = Vulture.localArrows.Count != deadBodies.Count();
            int index = 0;

            if (arrowUpdate)
            {
                foreach (Arrow arrow in Vulture.localArrows) UnityEngine.Object.Destroy(arrow.ArrowObject);
                Vulture.localArrows = new List<Arrow>();
            }

            foreach (DeadBody db in deadBodies)
            {
                if (arrowUpdate)
                {
                    Vulture.localArrows.Add(new Arrow(Color.blue));
                    Vulture.localArrows[index].ArrowObject.SetActive(true);
                }
                if (Vulture.localArrows[index] != null) Vulture.localArrows[index].Update(db.transform.position);
                index++;
            }
        }

        public static void mediumSetTarget()
        {
            if (Medium.medium == null || Medium.medium != PlayerControl.LocalPlayer || Medium.medium.Data.IsDead || Medium.deadBodies == null || ShipStatus.Instance?.AllVents == null) return;

            DeadPlayer target = null;
            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
            float closestDistance = float.MaxValue;
            float usableDistance = ShipStatus.Instance.AllVents.FirstOrDefault().UsableDistance;
            foreach ((DeadPlayer dp, Vector3 ps) in Medium.deadBodies)
            {
                float distance = Vector2.Distance(ps, truePosition);
                if (distance <= usableDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    target = dp;
                }
            }
            Medium.target = target;
        }


        static void gmUpdate()
        {
            if (GM.gm == null || GM.gm != PlayerControl.LocalPlayer) return;

            bool showIcon = (GM.canWarp || GM.canKill) && MeetingHud.Instance == null;

            foreach (byte playerID in MapOptions.PlayerIcons.Keys)
            {
                PlayerControl pc = Helpers.PlayerById(playerID);
                PoolablePlayer pp = MapOptions.PlayerIcons[playerID];
                if (pc.Data.Disconnected)
                {
                    pp.gameObject.SetActive(false);
                    continue;
                }

                pp.gameObject.SetActive(showIcon);
                if (pc.Data.IsDead)
                {
                    pp.SetSemiTransparent(true);
                }
                else
                {
                    pp.SetSemiTransparent(false);
                }
            }

            if (DestroyableSingleton<TaskPanelBehaviour>.InstanceExists)
            {
                DestroyableSingleton<TaskPanelBehaviour>.Instance.enabled = false;
                DestroyableSingleton<TaskPanelBehaviour>.Instance.background.enabled = false;
                DestroyableSingleton<TaskPanelBehaviour>.Instance.tab.enabled = false;
                DestroyableSingleton<TaskPanelBehaviour>.Instance.taskText.enabled = false;
                DestroyableSingleton<TaskPanelBehaviour>.Instance.tab.transform.FindChild("TabText_TMP").GetComponent<TMPro.TextMeshPro>().SetText("");
                //TaskPanelBehaviour.Instance.transform.localPosition = Vector3.negativeInfinityVector;
            }

        }

        public static void lawyerUpdate()
        {
            if (Lawyer.lawyer == null || Lawyer.lawyer != PlayerControl.LocalPlayer) return;

            // Meeting win
            if (Lawyer.winsAfterMeetings && Lawyer.neededMeetings == Lawyer.meetings && Lawyer.target != null && !Lawyer.target.Data.IsDead)
            {
                Lawyer.winsAfterMeetings = false; // Avoid sending mutliple RPCs until the host finshes the game
                MessageWriter winWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LawyerWin, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(winWriter);
                RpcProcedure.lawyerWin();
                return;
            }

            // Promote to Pursuer
            if (Lawyer.target != null && Lawyer.target.Data.Disconnected)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.lawyerPromotesToPursuer();
                return;
            }
        }

        public static void hackerUpdate() {
            if (Hacker.hacker == null || PlayerControl.LocalPlayer != Hacker.hacker || Hacker.hacker.Data.IsDead) return;
            var (playerCompleted, _) = TaskHandler.GetTaskInfo(Hacker.hacker.Data);
            if (playerCompleted == Hacker.rechargedTasks) {
                Hacker.rechargedTasks += Hacker.rechargeTasksNumber;
                if (Hacker.toolsNumber > Hacker.chargesVitals) Hacker.chargesVitals++;
                if (Hacker.toolsNumber > Hacker.chargesAdminTable) Hacker.chargesAdminTable++;
            }
        }

        static void pursuerSetTarget()
        {
            if (Pursuer.pursuer == null || Pursuer.pursuer != PlayerControl.LocalPlayer) return;
            Pursuer.target = setTarget();
            setPlayerOutline(Pursuer.target, Pursuer.color);
        }

        static void witchSetTarget()
        {
            if (Witch.witch == null || Witch.witch != PlayerControl.LocalPlayer) return;
            List<PlayerControl> untargetables;
            if (Witch.spellCastingTarget != null)
                untargetables = PlayerControl.AllPlayerControls.ToArray().Where(x => x.PlayerId != Witch.spellCastingTarget.PlayerId).ToList(); // Don't switch the target from the the one you're currently casting a spell on
            else
            {
                untargetables = new List<PlayerControl>(); // Also target players that have already been spelled, to hide spells that were blanks/blocked by shields
                if (Spy.spy != null && !Witch.canSpellAnyone) untargetables.Add(Spy.spy);
            }
            Witch.currentTarget = setTarget(onlyCrewmates: !Witch.canSpellAnyone, untargetablePlayers: untargetables);
            setPlayerOutline(Witch.currentTarget, Witch.color);
        }


        public static void Postfix(PlayerControl __instance)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            // Mini and Morphling shrink
            playerSizeUpdate(__instance);

            if (PlayerControl.LocalPlayer == __instance)
            {
                // Update player outlines
                setBasePlayerOutlines();

                // Update Role Description
                Helpers.RefreshRoleDescription(__instance);

                // Update Player Info
                updatePlayerInfo();

                // Time Master
                bendTimeUpdate();
                // Morphling
                morphlingSetTarget();
                // Medic
                medicSetTarget();
                // Shifter
                shifterUpdate();
                // Detective
                detectiveUpdateFootPrints();
                // Tracker
                trackerSetTarget();
                // Impostor
                impostorSetTarget();
                // Vampire
                vampireSetTarget();
                Garlic.UpdateAll();
                // Eraser
                eraserSetTarget();
                // Engineer
                engineerUpdate();
                // Tracker
                trackerUpdate();
                // Jackal
                jackalSetTarget();
                // Sidekick
                sidekickSetTarget();
                // Warlock
                warlockSetTarget();
                // Check for sidekick promotion on Jackal disconnect
                sidekickCheckPromotion();
                // SecurityGuard
                securityGuardSetTarget();
                securityGuardUpdate();
                // Arsonist
                arsonistSetTarget();
                // Snitch
                snitchUpdate();
                // BountyHunter
                bountyHunterUpdate();
                // Bait
                baitUpdate();
                // GM
                gmUpdate();
                // Vulture
                vultureUpdate();
                // Medium
                mediumSetTarget();
                // Lawyer
                lawyerUpdate();
                // Pursuer
                pursuerSetTarget();
                // Witch
                witchSetTarget();
                hackerUpdate();
            }

            TheOtherRolesGM.FixedUpdate(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.WalkPlayerTo))]
    class PlayerPhysicsWalkPlayerToPatch
    {
        private static Vector2 offset = Vector2.zero;
        public static void Prefix(PlayerPhysics __instance)
        {
            bool correctOffset = Camouflager.camouflageTimer <= 0f && (__instance.myPlayer == Mini.mini || (Morphling.morphling != null && __instance.myPlayer == Morphling.morphling && Morphling.morphTarget == Mini.mini && Morphling.morphTimer > 0f));
            if (correctOffset)
            {
                float currentScaling = (Mini.growingProgress() + 1) * 0.5f;
                __instance.myPlayer.Collider.offset = currentScaling * Mini.defaultColliderOffset * Vector2.down;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
    class PlayerControlCmdReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            Helpers.HandleVampireBiteOnBodyReport();

            if (__instance.IsGM())
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.LocalPlayer.CmdReportDeadBody))]
    class BodyReportPatch
    {
        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
        {
            // Medic or Detective report
            bool isMedicReport = Medic.medic != null && Medic.medic == PlayerControl.LocalPlayer && __instance.PlayerId == Medic.medic.PlayerId;
            bool isDetectiveReport = Detective.detective != null && Detective.detective == PlayerControl.LocalPlayer && __instance.PlayerId == Detective.detective.PlayerId;
            if (isMedicReport || isDetectiveReport)
            {
                DeadPlayer deadPlayer = DeadPlayers?.Where(x => x.player?.PlayerId == target?.PlayerId)?.FirstOrDefault();

                if (deadPlayer != null && deadPlayer.killerIfExisting != null)
                {
                    float timeSinceDeath = ((float)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalMilliseconds);
                    string msg = "";

                    if (isMedicReport)
                    {
                        msg = String.Format(ModTranslation.GetString("medicReport"), Math.Round(timeSinceDeath / 1000));
                    }
                    else if (isDetectiveReport)
                    {
                        if (timeSinceDeath < Detective.reportNameDuration * 1000)
                        {
                            msg = String.Format(ModTranslation.GetString("detectiveReportName"), deadPlayer.killerIfExisting.Data.PlayerName);
                        }
                        else if (timeSinceDeath < Detective.reportColorDuration * 1000)
                        {
                            var typeOfColor = Helpers.IsLighterColor(deadPlayer.killerIfExisting.Data.DefaultOutfit.ColorId) ?
                                ModTranslation.GetString("detectiveColorLight") :
                                ModTranslation.GetString("detectiveColorDark");
                            msg = String.Format(ModTranslation.GetString("detectiveReportColor"), typeOfColor);
                        }
                        else
                        {
                            msg = ModTranslation.GetString("detectiveReportNone");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                        {
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                        }
                        if (msg.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            DestroyableSingleton<UnityTelemetry>.Instance.SendWho();
                        }
                    }
                }
            }
        }
    }

    // Code from Among Us v2024.11.26e
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    public static class HandleMurderRequestPatch
    {
        static bool Prefix(PlayerControl __instance, PlayerControl target)
        {
            __instance.logger.Debug(string.Format("Checking if {0} murdered {1}", __instance.PlayerId, (target == null) ? "null player" : target.PlayerId.ToString()), null);
            __instance.isKilling = false;
            if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
                return false;
            if (!target || __instance.Data.Disconnected) // MODIFIED
            {
                int num = target ? target.PlayerId : -1;
                __instance.logger.Warning(string.Format("Bad kill from {0} to {1}", __instance.PlayerId, num), null);
                __instance.RpcMurderPlayer(target, false);
                return false;
            }
            NetworkedPlayerInfo data = target.Data;
            if (data == null || data.IsDead || target.inVent || target.MyPhysics.Animations.IsPlayingEnterVentAnimation() || target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() || target.inMovingPlat)
            {
                __instance.logger.Warning("Invalid target data for kill", null);
                __instance.RpcMurderPlayer(target, false);
                return false;
            }
            if (MeetingHud.Instance)
            {
                __instance.logger.Warning("Tried to kill while a meeting was starting", null);
                __instance.RpcMurderPlayer(target, false);
                return false;
            }
            __instance.isKilling = true;
            __instance.RpcMurderPlayer(target, true);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;

        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Allow everyone to murder players
            resetToCrewmate = !__instance.Data.Role.IsImpostor;
            resetToDead = __instance.Data.IsDead;
            __instance.Data.Role.TeamType = RoleTeamTypes.Impostor;
            __instance.Data.IsDead = false;

            if (Morphling.morphling != null && target == Morphling.morphling)
            {
                Morphling.resetMorph();
            }

            target.ResetMorph();
        }

        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(target, DateTime.UtcNow, DeathReason.Kill, __instance);
            GameHistory.DeadPlayers.Add(deadPlayer);

            // Reset killer to crewmate if resetToCrewmate
            if (resetToCrewmate) __instance.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            if (resetToDead) __instance.Data.IsDead = true;

            // Remove fake tasks when player dies
            if (target.HasFakeTasks())
                target.ClearAllTasks();

            // Sidekick promotion trigger on murder
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && target == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.sidekickPromotes();
            }

            // Pursuer promotion trigger on murder (the host sends the call such that everyone recieves the update before a possible game End)
            if (target == Lawyer.target && AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.lawyerPromotesToPursuer();
            }

            // Cleaner Button Sync
            if (Cleaner.cleaner != null && PlayerControl.LocalPlayer == Cleaner.cleaner && __instance == Cleaner.cleaner && HudManagerStartPatch.cleanerCleanButton != null)
                HudManagerStartPatch.cleanerCleanButton.Timer = Cleaner.cleaner.killTimer;


            // Witch Button Sync
            if (Witch.triggerBothCooldowns && Witch.witch != null && PlayerControl.LocalPlayer == Witch.witch && __instance == Witch.witch && HudManagerStartPatch.witchSpellButton != null)
                HudManagerStartPatch.witchSpellButton.Timer = HudManagerStartPatch.witchSpellButton.MaxTimer;

            // Warlock Button Sync
            if (Warlock.warlock != null && PlayerControl.LocalPlayer == Warlock.warlock && __instance == Warlock.warlock && HudManagerStartPatch.warlockCurseButton != null)
            {
                if (Warlock.warlock.killTimer > HudManagerStartPatch.warlockCurseButton.Timer)
                {
                    HudManagerStartPatch.warlockCurseButton.Timer = Warlock.warlock.killTimer;
                }
            }

            // Seer show flash and add dead player position
            if (Seer.seer != null && PlayerControl.LocalPlayer == Seer.seer && !Seer.seer.Data.IsDead && Seer.seer != target && Seer.mode <= 1)
            {
                HudManager.Instance.FullScreen.enabled = true;
                HudManager.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) =>
                {
                    var renderer = HudManager.Instance.FullScreen;
                    if (p < 0.5)
                    {
                        if (renderer != null)
                            renderer.color = new Color(42f / 255f, 187f / 255f, 245f / 255f, Mathf.Clamp01(p * 2 * 0.75f));
                    }
                    else
                    {
                        if (renderer != null)
                            renderer.color = new Color(42f / 255f, 187f / 255f, 245f / 255f, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                })));
            }
            if (Seer.DeadBodyPositions != null) Seer.DeadBodyPositions.Add(target.transform.position);

            // Tracker store body positions
            if (Tracker.DeadBodyPositions != null) Tracker.DeadBodyPositions.Add(target.transform.position);

            // Medium add body
            if (Medium.deadBodies != null)
            {
                Medium.featureDeadBodies.Add(new Tuple<DeadPlayer, Vector3>(deadPlayer, target.transform.position));
            }

            // Mini set adapted kill cooldown
            if (Mini.mini != null && PlayerControl.LocalPlayer == Mini.mini && Mini.mini.Data.Role.IsImpostor && Mini.mini == __instance)
            {
                var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
                Mini.mini.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().KillCooldown * multiplier);
            }

            // Set bountyHunter cooldown
            if (BountyHunter.bountyHunter != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter && __instance == BountyHunter.bountyHunter)
            {
                if (target == BountyHunter.bounty)
                {
                    BountyHunter.bountyHunter.SetKillTimer(BountyHunter.bountyKillCooldown);
                    BountyHunter.bountyUpdateTimer = 0f; // Force bounty update
                }
                else
                    BountyHunter.bountyHunter.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown() + BountyHunter.punishmentTime);
            }

            // Update arsonist status
            Arsonist.updateStatus();
			
            // Show flash on bait kill to the killer if enabled
            if (Bait.bait != null && target == Bait.bait && Bait.showKillFlash && __instance != Bait.bait && __instance == PlayerControl.LocalPlayer) {
                HudManager.Instance.FullScreen.enabled = true;
                HudManager.Instance.StartCoroutine(Effects.Lerp(1f, new Action<float>((p) => {
                    var renderer = HudManager.Instance.FullScreen;
                    if (p < 0.5) {
                        if (renderer != null)
                            renderer.color = new Color(204f / 255f, 102f / 255f, 0f / 255f, Mathf.Clamp01(p * 2 * 0.75f));
                    }
                    else {
                        if (renderer != null)
                            renderer.color = new Color(204f / 255f, 102f / 255f, 0f / 255f, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                })));
            }

            __instance.OnKill(target);
            target.OnDeath(__instance);
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (GameManager.Instance.LogicOptions.GetKillCooldown() <= 0f) return false;
            float multiplier = 1f;
            float addition = 0f;
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Mini) && PlayerControl.LocalPlayer.IsImpostor()) multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            if (PlayerControl.LocalPlayer.IsRole(RoleType.BountyHunter)) addition = BountyHunter.punishmentTime;
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Ninja) && Ninja.isPenalized(PlayerControl.LocalPlayer)) addition = Ninja.killPenalty;

            float max = Mathf.Max(GameManager.Instance.LogicOptions.GetKillCooldown() * multiplier + addition, __instance.killTimer);
            __instance.SetKillTimerUnchecked(Mathf.Clamp(time, 0f, max), max);
            return false;
        }

        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;

            player.killTimer = time;
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationCoPerformKillPatch {
        public static bool hideNextAnimation = false;

        public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)]ref PlayerControl source, [HarmonyArgument(1)]ref PlayerControl target) {
            if (hideNextAnimation)
                source = target;
            hideNextAnimation = false;
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
    class KillAnimationSetMovementPatch {
        private static int? colorId = null;
        public static void Prefix(PlayerControl source, bool canMove) {
            Color color = source.cosmetics.currentBodySprite.BodySprite.material.GetColor("_BodyColor");
            if (color != null && Morphling.morphling != null && source.Data.PlayerId == Morphling.morphling.PlayerId) {
                var index = Palette.PlayerColors.IndexOf(color);
                if (index != -1) colorId = index;
            }
        }

        public static void Postfix(PlayerControl source, bool canMove) {
            if (colorId.HasValue) source.RawSetColor(colorId.Value);
            colorId = null;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
    public static class ExilePlayerPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // Collect dead player info
            DeadPlayer deadPlayer = new DeadPlayer(__instance, DateTime.UtcNow, DeathReason.Exile, null);
            GameHistory.DeadPlayers.Add(deadPlayer);

            // Remove fake tasks when player dies
            if (__instance.HasFakeTasks())
                __instance.ClearAllTasks();

            __instance.OnDeath(killer: null);

            // Sidekick promotion trigger on exile
            if (Sidekick.promotesToJackal && Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead && __instance == Jackal.jackal && Jackal.jackal == PlayerControl.LocalPlayer)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.sidekickPromotes();
            }

            // Pursuer promotion trigger on exile (the host sends the call such that everyone recieves the update before a possible game End)
            if (__instance == Lawyer.target && AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RpcProcedure.lawyerPromotesToPursuer();
            }
        }
    }

    //[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    //class PlayerControlCanMovePatch
    //{
    //    public static bool Prefix(PlayerControl __instance, ref bool __result)
    //    {
    //        __result = __instance.moveable &&
    //            !Minigame.Instance &&
    //            (!DestroyableSingleton<HudManager>.InstanceExists || (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)) &&
    //            (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) &&
    //            !MeetingHud.Instance &&
    //            !ExileController.Instance &&
    //            !IntroCutscene.Instance;
    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckName))]
    class PlayerControlCheckNamePatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string name)
        {
            if (CustomOptionHolder.UselessOptions.GetBool() && CustomOptionHolder.playerNameDupes.GetBool())
            {
                __instance.RpcSetName(name);
                return false;
            }

            return true;
        }
    }
}
