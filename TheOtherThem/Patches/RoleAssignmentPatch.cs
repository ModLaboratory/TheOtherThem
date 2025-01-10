using AmongUs.GameOptions;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOtherThem.ToTRole;
using UnityEngine;
using static TheOtherThem.TheOtherRoles;

namespace TheOtherThem.Patches
{
    [HarmonyPatch(typeof(RoleOptionsData), nameof(RoleOptionsData.GetNumPerGame))]
    class RoleOptionsDataGetNumPerGamePatch
    {
        public static void Postfix(ref int __result, ref RoleTypes role)
        {
            if (role == RoleTypes.Crewmate || role == RoleTypes.Impostor) return;

            if (CustomOptionHolder.activateRoles.GetBool()) __result = 0; // Deactivate Vanilla Roles if the mod roles are active
        }
    }


    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class RoleManagerSelectRolesPatch
    {
        private static List<byte> blockLovers = new List<byte>();
        public static int blockedAssignments = 0;
        public static int maxBlocks = 10;

        public static void Postfix()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.ResetVaribles, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.resetVariables();

            if (!DestroyableSingleton<TutorialManager>.InstanceExists && CustomOptionHolder.activateRoles.GetBool()) // Don't assign Roles in Tutorial or if deactivated
                assignRoles();
        }

        private static void assignRoles()
        {
            if (CustomOptionHolder.gmEnabled.GetBool() && CustomOptionHolder.gmIsHost.GetBool())
            {
                PlayerControl host = AmongUsClient.Instance?.GetHost().Character;
                if (host.Data.Role.IsImpostor)
                {
                    Helpers.log("Why are we here");
                    bool hostIsImpostor = host.Data.Role.IsImpostor;
                    if (host.Data.Role.IsImpostor)
                    {
                        int newImpId = 0;
                        PlayerControl newImp;
                        while (true)
                        {
                            newImpId = rnd.Next(0, PlayerControl.AllPlayerControls.Count);
                            newImp = PlayerControl.AllPlayerControls[newImpId];
                            if (newImp == host || newImp.Data.Role.IsImpostor)
                            {
                                continue;
                            }
                            break;
                        }

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.OverrideNativeRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(host.PlayerId);
                        writer.Write((byte)RoleTypes.Crewmate);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.overrideNativeRole(host.PlayerId, (byte)RoleTypes.Crewmate);

                        writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.OverrideNativeRole, Hazel.SendOption.Reliable, -1);
                        writer.Write(newImp.PlayerId);
                        writer.Write((byte)RoleTypes.Impostor);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.overrideNativeRole(newImp.PlayerId, (byte)RoleTypes.Impostor);
                    }
                }
            }

            blockLovers = new List<byte> {
                (byte)RoleType.Bait,
            };

            if (!Lovers.hasTasks)
            {
                blockLovers.Add((byte)RoleType.Snitch);
                blockLovers.Add((byte)RoleType.FortuneTeller);
                blockLovers.Add((byte)RoleType.Fox);
            }

            if (!CustomOptionHolder.arsonistCanBeLovers.GetBool())
            {
                blockLovers.Add((byte)RoleType.Arsonist);
            }

            var data = getRoleAssignmentData();
            assignSpecialRoles(data); // Assign special roles like mafia and lovers first as they assign a role to multiple players and the chances are independent of the ticket system
            selectFactionForFactionIndependentRoles(data);
            assignEnsuredRoles(data); // Assign roles that should always be in the game next
            assignChanceRoles(data); // Assign roles that may or may not be in the game last
            assignRoleTargets(data);
            assignRoleModifiers(data);
        }

        private static RoleAssignmentData getRoleAssignmentData()
        {
            // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
            List<PlayerControl> crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
            List<PlayerControl> impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
            impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

            var crewmateMin = CustomOptionHolder.crewmateRolesCountMin.GetSelection();
            var crewmateMax = CustomOptionHolder.crewmateRolesCountMax.GetSelection();
            var neutralMin = CustomOptionHolder.neutralRolesCountMin.GetSelection();
            var neutralMax = CustomOptionHolder.neutralRolesCountMax.GetSelection();
            var impostorMin = CustomOptionHolder.impostorRolesCountMin.GetSelection();
            var impostorMax = CustomOptionHolder.impostorRolesCountMax.GetSelection();

            // Make sure min is less or equal to max
            if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
            if (neutralMin > neutralMax) neutralMin = neutralMax;
            if (impostorMin > impostorMax) impostorMin = impostorMax;

            // Get the maximum allowed count of each role type based on the minimum and maximum option
            int crewCountSettings = rnd.Next(crewmateMin, crewmateMax + 1);
            int neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
            int impCountSettings = rnd.Next(impostorMin, impostorMax + 1);

            // Potentially lower the actual maximum to the assignable players
            int maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
            int maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
            int maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

            // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
            Dictionary<byte, (int rate, int count)> impSettings = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int rate, int count)> neutralSettings = new Dictionary<byte, (int, int)>();
            Dictionary<byte, (int rate, int count)> crewSettings = new Dictionary<byte, (int, int)>();

            impSettings.Add((byte)RoleType.Morphling, CustomOptionHolder.morphlingSpawnRate.Data);
            impSettings.Add((byte)RoleType.Camouflager, CustomOptionHolder.camouflagerSpawnRate.Data);
            impSettings.Add((byte)RoleType.Vampire, CustomOptionHolder.vampireSpawnRate.Data);
            impSettings.Add((byte)RoleType.Eraser, CustomOptionHolder.eraserSpawnRate.Data);
            impSettings.Add((byte)RoleType.Trickster, CustomOptionHolder.tricksterSpawnRate.Data);
            impSettings.Add((byte)RoleType.Cleaner, CustomOptionHolder.cleanerSpawnRate.Data);
            impSettings.Add((byte)RoleType.Warlock, CustomOptionHolder.warlockSpawnRate.Data);
            impSettings.Add((byte)RoleType.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.Data);
            impSettings.Add((byte)RoleType.Witch, CustomOptionHolder.witchSpawnRate.Data);
            impSettings.Add((byte)RoleType.Ninja, CustomOptionHolder.ninjaSpawnRate.Data);
            impSettings.Add((byte)RoleType.NekoKabocha, CustomOptionHolder.nekoKabochaSpawnRate.Data);
            impSettings.Add((byte)RoleType.SerialKiller, CustomOptionHolder.serialKillerSpawnRate.Data);

            neutralSettings.Add((byte)RoleType.Jester, CustomOptionHolder.jesterSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.Arsonist, CustomOptionHolder.arsonistSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.Jackal, CustomOptionHolder.jackalSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.Opportunist, CustomOptionHolder.opportunistSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.Vulture, CustomOptionHolder.vultureSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.Lawyer, CustomOptionHolder.lawyerSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.PlagueDoctor, CustomOptionHolder.plagueDoctorSpawnRate.Data);
            neutralSettings.Add((byte)RoleType.Fox, CustomOptionHolder.foxSpawnRate.Data);

            crewSettings.Add((byte)RoleType.FortuneTeller, CustomOptionHolder.fortuneTellerSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Mayor, CustomOptionHolder.mayorSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Engineer, CustomOptionHolder.engineerSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Sheriff, CustomOptionHolder.sheriffSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Lighter, CustomOptionHolder.lighterSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Detective, CustomOptionHolder.detectiveSpawnRate.Data);
            crewSettings.Add((byte)RoleType.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Medic, CustomOptionHolder.medicSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Seer, CustomOptionHolder.seerSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Hacker, CustomOptionHolder.hackerSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Tracker, CustomOptionHolder.trackerSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Snitch, CustomOptionHolder.snitchSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Bait, CustomOptionHolder.baitSpawnRate.Data);
            crewSettings.Add((byte)RoleType.SecurityGuard, CustomOptionHolder.securityGuardSpawnRate.Data);
            crewSettings.Add((byte)RoleType.Medium, CustomOptionHolder.mediumSpawnRate.Data);

            foreach (var role in CustomRole.AllRoles)
            {
                var roleList = role.MyTeamType switch
                {
                    TeamTypeTOT.Crewmate => crewSettings,
                    TeamTypeTOT.Impostor => impSettings,
                    TeamTypeTOT.Neutral or _ => neutralSettings
                };

                try
                {

                    roleList.Add((byte)role.MyRoleType, ((CustomRoleOption)role.MyRoleInfo.BaseOption).Data);
                }
                catch
                {
                    Main.Logger.LogInfo(role.MyRoleInfo.BaseOption == null);
                }
            }

            if (impostors.Count > 1)
            {
                // Only add Spy if more than 1 impostor as the spy role is otherwise useless
                crewSettings.Add((byte)RoleType.Spy, CustomOptionHolder.spySpawnRate.Data);
            }

            return new RoleAssignmentData
            {
                crewmates = crewmates,
                impostors = impostors,
                crewSettings = crewSettings,
                neutralSettings = neutralSettings,
                impSettings = impSettings,
                maxCrewmateRoles = maxCrewmateRoles,
                maxNeutralRoles = maxNeutralRoles,
                maxImpostorRoles = maxImpostorRoles
            };
        }

        private static void assignSpecialRoles(RoleAssignmentData data)
        {
            // Assign GM
            if (CustomOptionHolder.gmEnabled.GetBool() == true)
            {
                byte gmID = 0;

                if (CustomOptionHolder.gmIsHost.GetBool() == true)
                {
                    PlayerControl host = AmongUsClient.Instance?.GetHost().Character;
                    gmID = setRoleToHost((byte)RoleType.GM, host);

                    // First, remove the GM from role selection.
                    data.crewmates.RemoveAll(x => x.PlayerId == host.PlayerId);
                    data.impostors.RemoveAll(x => x.PlayerId == host.PlayerId);

                }
                else
                {
                    gmID = SetRoleToRandomPlayer((byte)RoleType.GM, data.crewmates);
                }

                PlayerControl p = PlayerControl.AllPlayerControls.ToArray().ToList().Find(x => x.PlayerId == gmID);

                if (p != null && CustomOptionHolder.gmDiesAtStart.GetBool())
                {
                    p.Exiled();
                }
            }

            // Assign Lovers
            for (int i = 0; i < CustomOptionHolder.loversNumCouples.GetFloat(); i++)
            {
                var singleCrew = data.crewmates.FindAll(x => !x.isLovers());
                var singleImps = data.impostors.FindAll(x => !x.isLovers());

                bool isOnlyRole = !CustomOptionHolder.loversCanHaveAnotherRole.GetBool();
                if (rnd.Next(1, 101) <= CustomOptionHolder.loversSpawnRate.GetSelection() * 10)
                {
                    int lover1 = -1;
                    int lover2 = -1;
                    int lover1Index = -1;
                    int lover2Index = -1;
                    if (singleImps.Count > 0 && singleCrew.Count > 0 && (!isOnlyRole || (data.maxCrewmateRoles > 0 && data.maxImpostorRoles > 0)) && rnd.Next(1, 101) <= CustomOptionHolder.loversImpLoverRate.GetSelection() * 10)
                    {
                        lover1Index = rnd.Next(0, singleImps.Count);
                        lover1 = singleImps[lover1Index].PlayerId;

                        lover2Index = rnd.Next(0, singleCrew.Count);
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.maxImpostorRoles--;
                            data.maxCrewmateRoles--;

                            data.impostors.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    else if (singleCrew.Count >= 2 && (isOnlyRole || data.maxCrewmateRoles >= 2))
                    {
                        lover1Index = rnd.Next(0, singleCrew.Count);
                        while (lover2Index == lover1Index || lover2Index < 0) lover2Index = rnd.Next(0, singleCrew.Count);

                        lover1 = singleCrew[lover1Index].PlayerId;
                        lover2 = singleCrew[lover2Index].PlayerId;

                        if (isOnlyRole)
                        {
                            data.maxCrewmateRoles -= 2;
                            data.crewmates.RemoveAll(x => x.PlayerId == lover1);
                            data.crewmates.RemoveAll(x => x.PlayerId == lover2);
                        }
                    }

                    if (lover1 >= 0 && lover2 >= 0)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetLovers, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)lover1);
                        writer.Write((byte)lover2);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.setLovers((byte)lover1, (byte)lover2);
                    }
                }
            }

            // Assign Mafia
            if (data.impostors.Count >= 3 && data.maxImpostorRoles >= 3 && (rnd.Next(1, 101) <= CustomOptionHolder.mafiaSpawnRate.GetSelection() * 10))
            {
                SetRoleToRandomPlayer((byte)RoleType.Godfather, data.impostors);
                SetRoleToRandomPlayer((byte)RoleType.Janitor, data.impostors);
                SetRoleToRandomPlayer((byte)RoleType.Mafioso, data.impostors);
                data.maxImpostorRoles -= 3;
            }
        }

        private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
        {
            // Assign Mini (33% chance impostor / 67% chance crewmate)
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && rnd.Next(1, 101) <= CustomOptionHolder.miniIsImpRate.GetSelection() * 10)
            {
                data.impSettings.Add((byte)RoleType.Mini, (CustomOptionHolder.miniSpawnRate.GetSelection(), 1));
            }
            else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
            {
                data.crewSettings.Add((byte)RoleType.Mini, (CustomOptionHolder.miniSpawnRate.GetSelection(), 1));
            }

            // Assign Guesser (chance to be impostor based on setting)
            bool isEvilGuesser = (rnd.Next(1, 101) <= CustomOptionHolder.guesserIsImpGuesserRate.GetSelection() * 10);
            if (CustomOptionHolder.guesserSpawnBothRate.GetSelection() > 0)
            {
                if (rnd.Next(1, 101) <= CustomOptionHolder.guesserSpawnRate.GetSelection() * 10)
                {
                    if (isEvilGuesser)
                    {
                        if (data.impostors.Count > 0 && data.maxImpostorRoles > 0)
                        {
                            byte evilGuesser = SetRoleToRandomPlayer((byte)RoleType.EvilGuesser, data.impostors);
                            data.impostors.ToList().RemoveAll(x => x.PlayerId == evilGuesser);
                            data.maxImpostorRoles--;
                            data.crewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionHolder.guesserSpawnBothRate.GetSelection(), 1));
                        }
                    }
                    else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
                    {
                        byte niceGuesser = SetRoleToRandomPlayer((byte)RoleType.NiceGuesser, data.crewmates);
                        data.crewmates.ToList().RemoveAll(x => x.PlayerId == niceGuesser);
                        data.maxCrewmateRoles--;
                        data.impSettings.Add((byte)RoleType.EvilGuesser, (CustomOptionHolder.guesserSpawnBothRate.GetSelection(), 1));
                    }
                }
            }
            else
            {
                if (isEvilGuesser) data.impSettings.Add((byte)RoleType.EvilGuesser, (CustomOptionHolder.guesserSpawnRate.GetSelection(), 1));
                else data.crewSettings.Add((byte)RoleType.NiceGuesser, (CustomOptionHolder.guesserSpawnRate.GetSelection(), 1));
            }

            // Assign Swapper (chance to be impostor based on setting)
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && rnd.Next(1, 101) <= CustomOptionHolder.swapperIsImpRate.GetSelection() * 10)
            {
                data.impSettings.Add((byte)RoleType.Swapper, (CustomOptionHolder.swapperSpawnRate.GetSelection(), 1));
            }
            else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
            {
                data.crewSettings.Add((byte)RoleType.Swapper, (CustomOptionHolder.swapperSpawnRate.GetSelection(), 1));
            }

            // Assign Shifter (chance to be neutral based on setting)
            bool shifterIsNeutral = false;
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && rnd.Next(1, 101) <= CustomOptionHolder.shifterIsNeutralRate.GetSelection() * 10)
            {
                data.neutralSettings.Add((byte)RoleType.Shifter, (CustomOptionHolder.shifterSpawnRate.GetSelection(), 1));
                shifterIsNeutral = true;
            }
            else if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0)
            {
                data.crewSettings.Add((byte)RoleType.Shifter, (CustomOptionHolder.shifterSpawnRate.GetSelection(), 1));
                shifterIsNeutral = false;
            }

            // Assign any dual role types
            foreach (var option in CustomDualRoleOption.dualRoles)
            {
                if (option.count <= 0 || !option.roleEnabled) continue;

                int niceCount = 0;
                int evilCount = 0;
                while (niceCount + evilCount < option.count)
                {
                    if (option.assignEqually)
                    {
                        niceCount++;
                        evilCount++;
                    }
                    else
                    {
                        bool isEvil = rnd.Next(1, 101) <= option.impChance * 10;
                        if (isEvil) evilCount++;
                        else niceCount++;
                    }
                }

                if (niceCount > 0)
                    data.crewSettings.Add((byte)option.roleType, (option.rate, niceCount));

                if (evilCount > 0)
                    data.impSettings.Add((byte)option.roleType, (option.rate, evilCount));
            }

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetShifterType, Hazel.SendOption.Reliable, -1);
            writer.Write(shifterIsNeutral);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setShifterType(shifterIsNeutral);
        }

        private static void assignEnsuredRoles(RoleAssignmentData data)
        {
            blockedAssignments = 0;

            // Get all roles where the chance to occur is set to 100%
            List<byte> ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
            List<byte> ensuredImpostorRoles = data.impSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) ||
                    (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
                )))
            {
                Dictionary<TeamType, List<byte>> rolesToAssign = new Dictionary<TeamType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(TeamType.Crewmate, ensuredCrewmateRoles);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(TeamType.Neutral, ensuredNeutralRoles);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(TeamType.Impostor, ensuredImpostorRoles);

                // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove the role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == TeamType.Crewmate || roleType == TeamType.Neutral ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                var player = SetRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                if (player == byte.MaxValue && blockedAssignments < maxBlocks)
                {
                    blockedAssignments++;
                    continue;
                }
                blockedAssignments = 0;

                rolesToAssign[roleType].RemoveAt(index);

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                    {
                        // Set chance for the blocked roles to 0 for chances less than 100%
                        if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = (0, 0);
                        if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = (0, 0);
                        if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = (0, 0);
                        // Remove blocked roles even if the chance was 100%
                        foreach (var ensuredRolesList in rolesToAssign.Values)
                        {
                            ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                        }
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.maxCrewmateRoles--; break;
                    case TeamType.Neutral: data.maxNeutralRoles--; break;
                    case TeamType.Impostor: data.maxImpostorRoles--; break;
                }
            }
        }


        private static void assignChanceRoles(RoleAssignmentData data)
        {
            blockedAssignments = 0;

            // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
            List<byte> crewmateTickets = data.crewSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> neutralTickets = data.neutralSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();
            List<byte> impostorTickets = data.impSettings.Where(x => x.Value.rate > 0 && x.Value.rate < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.rate * x.Value.count)).SelectMany(x => x).ToList();

            // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
            while (
                (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) ||
                (data.crewmates.Count > 0 && (
                    (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                    (data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
                )))
            {

                Dictionary<TeamType, List<byte>> rolesToAssign = new Dictionary<TeamType, List<byte>>();
                if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(TeamType.Crewmate, crewmateTickets);
                if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(TeamType.Neutral, neutralTickets);
                if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(TeamType.Impostor, impostorTickets);

                // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role) 
                // then select one of the roles from the selected pool to a player 
                // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
                var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count()));
                var players = roleType == TeamType.Crewmate || roleType == TeamType.Neutral ? data.crewmates : data.impostors;
                var index = rnd.Next(0, rolesToAssign[roleType].Count);
                var roleId = rolesToAssign[roleType][index];
                var player = SetRoleToRandomPlayer(rolesToAssign[roleType][index], players);
                if (player == byte.MaxValue && blockedAssignments < maxBlocks)
                {
                    blockedAssignments++;
                    continue;
                }
                blockedAssignments = 0;

                rolesToAssign[roleType].RemoveAll(x => x == roleId);

                if (CustomOptionHolder.blockedRolePairings.ContainsKey(roleId))
                {
                    foreach (var blockedRoleId in CustomOptionHolder.blockedRolePairings[roleId])
                    {
                        // Remove tickets of blocked roles from all pools
                        crewmateTickets.RemoveAll(x => x == blockedRoleId);
                        neutralTickets.RemoveAll(x => x == blockedRoleId);
                        impostorTickets.RemoveAll(x => x == blockedRoleId);
                    }
                }

                // Adjust the role limit
                switch (roleType)
                {
                    case TeamType.Crewmate: data.maxCrewmateRoles--; break;
                    case TeamType.Neutral: data.maxNeutralRoles--; break;
                    case TeamType.Impostor: data.maxImpostorRoles--; break;
                }
            }
        }

        private static byte setRoleToHost(byte roleId, PlayerControl host, byte flag = 0)
        {
            byte playerId = host.PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(roleId, playerId, flag);
            return playerId;
        }

        private static void assignRoleTargets(RoleAssignmentData data)
        {
            // Set Lawyer Target
            if (Lawyer.lawyer != null)
            {
                var possibleTargets = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && !p.Data.Disconnected && !p.isLovers() && (p.Data.Role.IsImpostor || p == Jackal.jackal))
                        possibleTargets.Add(p);
                }
                if (possibleTargets.Count == 0)
                {
                    MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LawyerPromotesToPursuer, Hazel.SendOption.Reliable, -1);
                    AmongUsClient.Instance.FinishRpcImmediately(w);
                    RPCProcedure.lawyerPromotesToPursuer();
                }
                else
                {
                    var target = possibleTargets[TheOtherRoles.rnd.Next(0, possibleTargets.Count)];
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.LawyerSetTarget, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.lawyerSetTarget(target.PlayerId);
                }
            }
        }

        private static void assignRoleModifiers(RoleAssignmentData data)
        {
            // Madmate
            for (int i = 0; i < CustomOptionHolder.madmateSpawnRate.count; i++)
            {
                if (rnd.Next(1, 100) <= CustomOptionHolder.madmateSpawnRate.rate * 10)
                {
                    var candidates = Madmate.candidates;
                    if (candidates.Count <= 0)
                    {
                        break;
                    }

                    if (Madmate.madmateType == Madmate.MadmateType.Simple)
                    {
                        if (data.maxCrewmateRoles <= 0) break;
                        setModifierToRandomPlayer((byte)ModifierType.Madmate, Madmate.candidates);
                        data.maxCrewmateRoles--;
                    }
                    else
                    {
                        setModifierToRandomPlayer((byte)ModifierType.Madmate, Madmate.candidates);
                    }
                }
            }
        }

        private static byte SetRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, byte flag = 0, bool removePlayer = true)
        {
            Main.Logger.LogInfo("set role to random player " + (RoleType)roleId);

            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;
            if (RoleInfo.lovers.Enabled &&
                Helpers.playerById(playerId)?.isLovers() == true &&
                blockLovers.Contains(roleId))
            {
                return byte.MaxValue;
            }

            if (removePlayer) playerList.RemoveAt(index);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.SetRole, Hazel.SendOption.Reliable, -1);
            writer.Write(roleId);
            writer.Write(playerId);
            writer.Write(flag);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.SetRole(roleId, playerId, flag);
            return playerId;
        }

        private static byte setModifierToRandomPlayer(byte modId, List<PlayerControl> playerList)
        {
            if (playerList.Count <= 0)
            {
                return byte.MaxValue;
            }

            var index = rnd.Next(0, playerList.Count);
            byte playerId = playerList[index].PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRpc.AddModifier, Hazel.SendOption.Reliable, -1);
            writer.Write(modId);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.addModifier(modId, playerId);
            return playerId;
        }

        private class RoleAssignmentData
        {
            public List<PlayerControl> crewmates { get; set; }
            public List<PlayerControl> impostors { get; set; }
            public Dictionary<byte, (int rate, int count)> impSettings = new Dictionary<byte, (int, int)>();
            public Dictionary<byte, (int rate, int count)> neutralSettings = new Dictionary<byte, (int, int)>();
            public Dictionary<byte, (int rate, int count)> crewSettings = new Dictionary<byte, (int, int)>();
            public int maxCrewmateRoles { get; set; }
            public int maxNeutralRoles { get; set; }
            public int maxImpostorRoles { get; set; }
            public PlayerControl host { get; set; }
        }

        private enum TeamType
        {
            Crewmate = 0,
            Neutral = 1,
            Impostor = 2
        }

    }
}