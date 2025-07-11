using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using static TheOtherThem.MapOptions;
using static TheOtherThem.GameHistory;
using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TheOtherThem.Patches {
    [HarmonyPatch]
    class MeetingHudPatch {
        static bool[] selections;
        static SpriteRenderer[] renderers;
        private static NetworkedPlayerInfo target = null;
        private const float scale = 0.65f;
        private static Sprite blankNameplate = null;
        public static bool nameplatesChanged = true;
        public static bool animateSwap = false;

        static TMPro.TextMeshPro meetingInfoText;

        public static void updateNameplate(PlayerVoteArea pva, byte playerId = Byte.MaxValue)
        {
            blankNameplate = blankNameplate ?? ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;

            var nameplate = blankNameplate;
            if (!HideNameplates)
            {
                var p = Helpers.PlayerById(playerId != Byte.MaxValue ? playerId : pva.TargetPlayerId);
                var nameplateId = p?.CurrentOutfit?.NamePlateId;
                nameplate = ShipStatus.Instance.CosmeticsCache.GetNameplate(nameplateId).Image;
            }
            pva.Background.sprite = nameplate;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
        class PlayerVoteAreaCosmetics
        {
            static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
            {
                updateNameplate(__instance, playerInfo.PlayerId);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingHudUpdatePatch
        {
            static void Postfix(MeetingHud __instance)
            {
                if (nameplatesChanged)
                {
                    foreach (var pva in __instance.playerStates)
                    {
                        updateNameplate(pva);
                    }
                    nameplatesChanged = false;
                }

                if (__instance.state == MeetingHud.VoteStates.Animating)
                    return;

                // Deactivate skip Button if skipping on emergency meetings is disabled
                if (BlockSkippingInEmergencyMeetings)
                    __instance.SkipVoteButton?.gameObject?.SetActive(false);

                updateMeetingText(__instance);

                // This fixes a bug with the original game where pressing the button and a kill happens simultaneously
                // results in bodies sometimes being created *after* the meeting starts, marking them as dead and
                // removing the corpses so there's no random corpses leftover afterwards
                foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                {
                    if (b == null) continue;

                    foreach (PlayerVoteArea pva in __instance.playerStates)
                    {
                        if (pva == null) continue;

                        if (pva.TargetPlayerId == b?.ParentId && !pva.AmDead)
                        {
                            pva?.SetDead(pva.DidReport, true);
                            pva?.Overlay?.gameObject?.SetActive(true);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
        class MeetingCalculateVotesPatch {
            private static Dictionary<byte, int> CalculateVotes(MeetingHud __instance) {
                Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte votedFor = playerVoteArea.VotedFor;
                    if (votedFor != 252 && votedFor != 255 && votedFor != 254) {
                        PlayerControl player = Helpers.PlayerById((byte)playerVoteArea.TargetPlayerId);
                        if (player == null || player.Data == null || player.Data.IsDead || player.Data.Disconnected || player.IsGM()) continue;
                        
                        // don't try to vote for the GM
                        if (GM.gm != null && votedFor == GM.gm.PlayerId) continue;

                        int currentVotes;
                        int additionalVotes = (Mayor.mayor != null && Mayor.mayor.PlayerId == playerVoteArea.TargetPlayerId) ? Mayor.numVotes : 1; // Mayor vote
                        if (dictionary.TryGetValue(votedFor, out currentVotes))
                            dictionary[votedFor] = currentVotes + additionalVotes;
                        else
                            dictionary[votedFor] = additionalVotes;
                    }
                }

                // Swapper swap votes
                if (Swapper.swapper != null && !Swapper.swapper.Data.IsDead) {
                    PlayerVoteArea swapped1 = null;
                    PlayerVoteArea swapped2 = null;
                    foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                        if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                    }

                    if (swapped1 != null && swapped2 != null) {
                        if (!dictionary.ContainsKey(swapped1.TargetPlayerId)) dictionary[swapped1.TargetPlayerId] = 0;
                        if (!dictionary.ContainsKey(swapped2.TargetPlayerId)) dictionary[swapped2.TargetPlayerId] = 0;
                        int tmp = dictionary[swapped1.TargetPlayerId];
                        dictionary[swapped1.TargetPlayerId] = dictionary[swapped2.TargetPlayerId];
                        dictionary[swapped2.TargetPlayerId] = tmp;

                        if (AmongUsClient.Instance.AmHost)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SwapperAnimate, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RpcProcedure.swapperAnimate();
                        }
                    }
                }

                return dictionary;
            }


            static bool Prefix(MeetingHud __instance) {
                if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote)) {

			        Dictionary<byte, int> self = CalculateVotes(__instance);
                    bool tie;
			        KeyValuePair<byte, int> max = self.MaxPair(out tie);
                    NetworkedPlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key && !v.IsDead);

                    MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
                    for (int i = 0; i < __instance.playerStates.Length; i++)
                    {
                        PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                        array[i] = new MeetingHud.VoterState {
                            VoterId = playerVoteArea.TargetPlayerId,
                            VotedForId = playerVoteArea.VotedFor
                        };
                    }

                    // RPCVotingComplete
                    __instance.RpcVotingComplete(array, exiled, tie);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Select))]
        class MeetingHudSelectPatch
        {
            public static bool Prefix(ref bool __result, MeetingHud __instance, [HarmonyArgument(0)] int suspectStateIdx)
            {
                __result = false;
                if (GM.gm != null && GM.gm.PlayerId == suspectStateIdx) return false;
                if (NoVotingIsSelfVoting && PlayerControl.LocalPlayer.PlayerId == suspectStateIdx) return false;
                if (BlockSkippingInEmergencyMeetings && suspectStateIdx == -1) return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
        class MeetingHudBloopAVoteIconPatch {
            public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)]NetworkedPlayerInfo voterPlayer, [HarmonyArgument(1)]int index, [HarmonyArgument(2)]Transform parent) {
                SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(__instance.PlayerVotePrefab);
                if (!GameManager.Instance.LogicOptions.GetAnonymousVotes() || (PlayerControl.LocalPlayer.Data.IsDead && MapOptions.GhostsSeeVotes) || PlayerControl.LocalPlayer.IsRole(RoleType.Watcher))
                    PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
                else
                    PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
                spriteRenderer.transform.SetParent(parent);
                spriteRenderer.transform.localScale = Vector3.zero;
                __instance.StartCoroutine(Effects.Bloop((float)index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
                parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
                return false;
            }
        } 

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
        class MeetingHudPopulateVotesPatch {
            
            static bool Prefix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states) {
                // Swapper swap
                PlayerVoteArea swapped1 = null;
                PlayerVoteArea swapped2 = null;

                foreach (PlayerVoteArea playerVoteArea in __instance.playerStates) {
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId1) swapped1 = playerVoteArea;
                    if (playerVoteArea.TargetPlayerId == Swapper.playerId2) swapped2 = playerVoteArea;
                }
                bool doSwap = animateSwap && swapped1 != null && swapped2 != null && Swapper.swapper != null && !Swapper.swapper.Data.IsDead;
                if (doSwap) {
                    __instance.StartCoroutine(Effects.Slide3D(swapped1.transform, swapped1.transform.localPosition, swapped2.transform.localPosition, 1.5f));
                    __instance.StartCoroutine(Effects.Slide3D(swapped2.transform, swapped2.transform.localPosition, swapped1.transform.localPosition, 1.5f));

                    Swapper.numSwaps--;
                }


                __instance.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
                int num = 0;
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    byte targetPlayerId = playerVoteArea.TargetPlayerId;
                    // Swapper change playerVoteArea that gets the votes
                    if (doSwap && playerVoteArea.TargetPlayerId == swapped1.TargetPlayerId) playerVoteArea = swapped2;
                    else if (doSwap && playerVoteArea.TargetPlayerId == swapped2.TargetPlayerId) playerVoteArea = swapped1;

                    playerVoteArea.ClearForResults();
                    int num2 = 0;
                    //bool mayorFirstVoteDisplayed = false;
                    Dictionary<int, int> votesApplied = new Dictionary<int, int>();
                    for (int j = 0; j < states.Length; j++) {
                        MeetingHud.VoterState voterState = states[j];
                        PlayerControl voter = Helpers.PlayerById(voterState.VoterId);
                        if (voter == null) continue;

                        NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
                        if (playerById == null)
                        {
                            UnityEngine.Debug.LogError(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
                        }
                        else if (GM.gm != null && (voterState.VoterId == GM.gm.PlayerId || voterState.VotedForId == GM.gm.PlayerId))
                        {
                            continue;
                        }
                        else if (i == 0 && voterState.SkippedVote && !playerById.IsDead) {
                            __instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
                            num++;
                        }
                        else if (voterState.VotedForId == targetPlayerId && !playerById.IsDead) {
                            __instance.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
                            num2++;
                        }

                        if (!votesApplied.ContainsKey(voter.PlayerId))
                            votesApplied[voter.PlayerId] = 0;

                        votesApplied[voter.PlayerId]++;

                        // Major vote, redo this iteration to place a second vote
                        if (Mayor.mayor != null && voter.PlayerId == Mayor.mayor.PlayerId && votesApplied[voter.PlayerId] < Mayor.numVotes) {
                            j--;    
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class MeetingHudVotingCompletedPatch {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)]byte[] states, [HarmonyArgument(1)]NetworkedPlayerInfo exiled, [HarmonyArgument(2)]bool tie)
            {
                // Reset swapper values
                Swapper.playerId1 = Byte.MaxValue;
                Swapper.playerId2 = Byte.MaxValue;

                if (meetingInfoText != null)
                    meetingInfoText.gameObject.SetActive(false);

                foreach (DeadBody b in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                {
                    UnityEngine.Object.Destroy(b.gameObject);
                }

                if (exiled != null)
                {
                    FinalStatuses[exiled.PlayerId] = FinalStatus.Exiled;
                    bool isLovers = exiled.Object.IsInLove();

                    if (isLovers)
                        FinalStatuses[exiled.Object.GetPartner().PlayerId] = FinalStatus.Suicide;
                }
            }
        }

        static void gmKillOnClick(int i, MeetingHud __instance)
        {
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            SpriteRenderer renderer = renderers[i];
            var target = __instance.playerStates[i];

            if (target != null)
            {
                if (target.AmDead)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.GMRevive, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)target.TargetPlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.GMRevive(target.TargetPlayerId);

                    renderer.sprite = Guesser.getTargetSprite();
                    renderer.color = Color.red;
                }
                else
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.GMKill, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)target.TargetPlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RpcProcedure.GMKill(target.TargetPlayerId);

                    renderer.sprite = Swapper.getCheckSprite();
                    renderer.color = Color.green;
                }
            }
        }

        static void swapperOnClick(int i, MeetingHud __instance) {
            if (Swapper.numSwaps <= 0) return;
            if (__instance.state == MeetingHud.VoteStates.Results) return;
            if (__instance.playerStates[i].AmDead) return;

            int selectedCount = selections.Where(b => b).Count();
            SpriteRenderer renderer = renderers[i];

            if (selectedCount == 0) {
                renderer.color = Color.green;
                selections[i] = true;
            } else if (selectedCount == 1) {
                if (selections[i]) {
                    renderer.color = Color.red;
                    selections[i] = false;
                } else {
                    selections[i] = true;
                    renderer.color = Color.green;   
                    
                    PlayerVoteArea firstPlayer = null;
                    PlayerVoteArea secondPlayer = null;
                    for (int A = 0; A < selections.Length; A++) {
                        if (selections[A]) {
                            if (firstPlayer != null) {
                                secondPlayer = __instance.playerStates[A];
                                break;
                            } else {
                                firstPlayer = __instance.playerStates[A];
                            }
                        }
                    }

                    if (firstPlayer != null && secondPlayer != null) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SwapperSwap, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)firstPlayer.TargetPlayerId);
                        writer.Write((byte)secondPlayer.TargetPlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);

                        RpcProcedure.swapperSwap((byte)firstPlayer.TargetPlayerId, (byte)secondPlayer.TargetPlayerId);
                    }
                }
            }
        }

        private static GameObject guesserUI;
        static void guesserOnClick(int buttonTarget, MeetingHud __instance) {
            if (guesserUI != null || !(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted)) return;
            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

            Transform container = UnityEngine.Object.Instantiate(__instance.transform.Find("MeetingContents/PhoneUI"), __instance.transform);
            container.transform.localPosition = new Vector3(0, 0, -200f);
            guesserUI = container.gameObject;

            int i = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;

            Transform exitButtonParent = (new GameObject()).transform;
            exitButtonParent.SetParent(container);
            Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(2.725f, 2.1f, -200f);
            exitButtonParent.transform.localScale = new Vector3(0.25f, 0.9f, 1f);
            exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                UnityEngine.Object.Destroy(container.gameObject);
            }));

            List<Transform> buttons = new List<Transform>();
            Transform selectedButton = null;

            foreach (RoleInfo roleInfo in RoleInfo.AllRoleInfos)
            {
                RoleType guesserRole = (Guesser.niceGuesser != null && PlayerControl.LocalPlayer.PlayerId == Guesser.niceGuesser.PlayerId) ? RoleType.NiceGuesser : RoleType.EvilGuesser;
                if (roleInfo == null || 
                    roleInfo.MyRoleType == RoleType.Lovers || 
                    roleInfo.MyRoleType == guesserRole || 
                    roleInfo == RoleInfo.niceMini || 
					(!Guesser.evilGuesserCanGuessSpy && guesserRole == RoleType.EvilGuesser && roleInfo.MyRoleType == RoleType.Spy) ||
                    roleInfo == RoleInfo.gm ||
                    (Guesser.onlyAvailableRoles && !roleInfo.Enabled && !MapOptions.HideSettings))
                    continue; // Not guessable roles
				if (Guesser.guesserCantGuessSnitch && Snitch.snitch != null) {
                    var (playerCompleted, playerTotal) = TaskHandler.GetTaskInfo(Snitch.snitch.Data);
                    int numberOfLeftTasks = playerTotal - playerCompleted;
                    if (numberOfLeftTasks <= 0 && roleInfo.MyRoleType == RoleType.Snitch) continue;
                }
                Transform buttonParent = (new GameObject()).transform;
                buttonParent.SetParent(container);
                Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
                Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
                TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
                button.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
                buttons.Add(button);
                int row = i/5, col = i%5;
                buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -200f);
                buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                label.text = Helpers.ColorString(roleInfo.RoleColor, roleInfo.Name);
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                label.transform.localScale *= 1.6f;
                label.autoSizeTextContainer = true;
                int copiedIndex = i;

                button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                if (PlayerControl.LocalPlayer.IsAlive()) button.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                    if (selectedButton != button) {
                        selectedButton = button;
                        buttons.ForEach(x => x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                    } else {
                        PlayerControl focusedTarget = Helpers.PlayerById((byte)__instance.playerStates[buttonTarget].TargetPlayerId);
                        if (!(__instance.state == MeetingHud.VoteStates.Voted || __instance.state == MeetingHud.VoteStates.NotVoted) || focusedTarget == null || Guesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) <= 0 ) return;

                        if (!Guesser.killsThroughShield && focusedTarget == Medic.shielded) { // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true)); 
                            UnityEngine.Object.Destroy(container.gameObject);

                            MessageWriter murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                            RpcProcedure.shieldedMurderAttempt();
                            return;
                        }

                        var mainRoleInfo = RoleInfo.GetRoleInfoForPlayer(focusedTarget).FirstOrDefault();
                        if (mainRoleInfo == null) return;

                        PlayerControl dyingTarget = (mainRoleInfo == roleInfo) ? focusedTarget : PlayerControl.LocalPlayer;

                        // Reset the GUI
                        __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                        UnityEngine.Object.Destroy(container.gameObject);
                        if (Guesser.hasMultipleShotsPerMeeting && Guesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 1 && dyingTarget != PlayerControl.LocalPlayer)
                        {
                            __instance.playerStates.ToList().ForEach(x => { if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                        }
                        else
                        {
                            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                        }

                        // Shoot player and send chat info if activated
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.GuesserShoot, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        writer.Write(dyingTarget.PlayerId);
                        writer.Write(focusedTarget.PlayerId);
                        writer.Write((byte)roleInfo.MyRoleType);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RpcProcedure.guesserShoot(PlayerControl.LocalPlayer.PlayerId, dyingTarget.PlayerId, focusedTarget.PlayerId, (byte)roleInfo.MyRoleType);
                    }
                }));

                i++;
            }
            container.transform.localScale *= 0.75f;
        }

        [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
        class PlayerVoteAreaSelectPatch {
            static bool Prefix(MeetingHud __instance) {
                return !(PlayerControl.LocalPlayer != null && Guesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && guesserUI != null);
            }
        }


        static void populateButtonsPostfix(MeetingHud __instance) {
            nameplatesChanged = true;

            if (PlayerControl.LocalPlayer.IsRole(RoleType.GM) && GM.canKill)
            {
                renderers = new SpriteRenderer[__instance.playerStates.Length];

                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject checkbox = UnityEngine.Object.Instantiate(template);
                    checkbox.transform.SetParent(playerVoteArea.transform);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(-0.95f, 0.03f, -20f);
                    SpriteRenderer renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = playerVoteArea.AmDead ? Swapper.getCheckSprite() : Guesser.getTargetSprite();
                    renderer.color = playerVoteArea.AmDead ? Color.green : Color.red;

                    PassiveButton button = checkbox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => gmKillOnClick(copiedIndex, __instance)));

                    renderers[i] = renderer;
                }
            }

            // Add Swapper Buttons
            if (PlayerControl.LocalPlayer.IsRole(RoleType.Swapper) && Swapper.numSwaps > 0 && !Swapper.swapper.Data.IsDead) {
                selections = new bool[__instance.playerStates.Length];
                renderers = new SpriteRenderer[__instance.playerStates.Length];

                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || (playerVoteArea.TargetPlayerId == Swapper.swapper.PlayerId && Swapper.canOnlySwapOthers) || (playerVoteArea.TargetPlayerId == GM.gm?.PlayerId)) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject checkbox = UnityEngine.Object.Instantiate(template);
                    checkbox.transform.SetParent(playerVoteArea.transform);
                    checkbox.transform.position = template.transform.position;
                    checkbox.transform.localPosition = new Vector3(-0.95f, 0.03f, -20f);
                    SpriteRenderer renderer = checkbox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Swapper.getCheckSprite();
                    renderer.color = Color.red;

                    PassiveButton button = checkbox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => swapperOnClick(copiedIndex, __instance)));
                    
                    selections[i] = false;
                    renderers[i] = renderer;
                }

            }

            // Add overlay for spelled players
            if (Witch.witch != null && Witch.futureSpelled != null) {
                foreach (PlayerVoteArea pva in __instance.playerStates) {
                    if (Witch.futureSpelled.Any(x => x.PlayerId == pva.TargetPlayerId)) {
                        SpriteRenderer rend = (new GameObject()).AddComponent<SpriteRenderer>();
                        rend.transform.SetParent(pva.transform);
                        rend.gameObject.layer = pva.Megaphone.gameObject.layer;
                        rend.transform.localPosition = new Vector3(-0.5f, -0.03f, -1f);
                        rend.sprite = Witch.getSpelledOverlaySprite();
                    }
                }
            }

            // Add Guesser Buttons
            if (Guesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsAlive() && Guesser.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0) {
                for (int i = 0; i < __instance.playerStates.Length; i++) {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId || playerVoteArea.TargetPlayerId == GM.gm?.PlayerId) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = Guesser.getTargetSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => guesserOnClick(copiedIndex, __instance)));
                }
            }
        }

        public static void updateMeetingText(MeetingHud __instance)
        {
            // Uses remaining text for guesser/swapper
            if (meetingInfoText == null)
            {
                meetingInfoText = UnityEngine.Object.Instantiate(HudManager.Instance.TaskPanel.taskText, __instance.transform);
                meetingInfoText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                meetingInfoText.transform.position = Vector3.zero;
                meetingInfoText.transform.localPosition = new Vector3(-3.07f, 3.33f, -20f);
                meetingInfoText.transform.localScale *= 1.1f;
                meetingInfoText.color = Palette.White;
                meetingInfoText.gameObject.SetActive(false);
            }

            meetingInfoText.text = "";
            meetingInfoText.gameObject.SetActive(false);

            if (MeetingHud.Instance.state != MeetingHud.VoteStates.Voted &&
                MeetingHud.Instance.state != MeetingHud.VoteStates.NotVoted &&
                MeetingHud.Instance.state != MeetingHud.VoteStates.Discussion)
                return;

            if (PlayerControl.LocalPlayer.IsRole(RoleType.Swapper) && Swapper.numSwaps > 0 && !Swapper.swapper.Data.IsDead)
            {
                meetingInfoText.text = String.Format(ModTranslation.GetString("swapperSwapsLeft"), Swapper.numSwaps);
                meetingInfoText.gameObject.SetActive(true);
            }

            var numGuesses = Guesser.remainingShots(PlayerControl.LocalPlayer.PlayerId);
            if (Guesser.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer.IsAlive() && numGuesses > 0)
            {
                meetingInfoText.text = String.Format(ModTranslation.GetString("guesserGuessesLeft"), numGuesses);
                meetingInfoText.gameObject.SetActive(true);
            }

            if (PlayerControl.LocalPlayer.IsRole(RoleType.Shifter) && Shifter.futureShift != null)
            {
                meetingInfoText.text = String.Format(ModTranslation.GetString("shifterTargetInfo"), Shifter.futureShift.Data.PlayerName);
                meetingInfoText.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.ServerStart))]
        class MeetingServerStartPatch {
            static void Postfix(MeetingHud __instance)
            {
                populateButtonsPostfix(__instance);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class MeetingDeserializePatch {
            static void Postfix(MeetingHud __instance, [HarmonyArgument(0)]MessageReader reader, [HarmonyArgument(1)]bool initialState)
            {
                // Add swapper buttons
                if (initialState) {
                    populateButtonsPostfix(__instance);
                }
            }
        }

        public static void startMeeting()
        {
            animateSwap = false;
            CustomOverlays.ShowBlackBackground();
            CustomOverlays.HideInfoOverlay();
            TheOtherRolesGM.OnMeetingStart();
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.OpenMeetingRoom))]
        class OpenMeetingPatch
        {
            public static void Prefix(HudManager __instance)
            {
                startMeeting();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcStartMeeting))]
        class StartMeetingPatch {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)]NetworkedPlayerInfo meetingTarget)
            {
                startMeeting();
                // Medium meeting start time
                Medium.meetingStartTime = DateTime.UtcNow;
                // Reset vampire bitten
                Vampire.bitten = null;
                // Count meetings
                if (meetingTarget == null) MeetingsCount++;
                // Save the meeting target
                target = meetingTarget;
            }
        }
    }
}
