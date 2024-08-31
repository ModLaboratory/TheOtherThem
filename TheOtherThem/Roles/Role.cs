using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheOtherThem.TOTRole;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;

namespace TheOtherThem
{
    public enum RoleType
    {
        Crewmate = 0,
        Shifter,
        Mayor,
        Engineer,
        Sheriff,
        Lighter,
        Detective,
        TimeMaster,
        Medic,
        Swapper,
        Seer,
        Hacker,
        Tracker,
        Snitch,
        Spy,
        SecurityGuard,
        Bait,
        Medium,
        FortuneTeller,


        Impostor = 100,
        Godfather,
        Mafioso,
        Janitor,
        Morphling,
        Camouflager,
        Vampire,
        Eraser,
        Trickster,
        Cleaner,
        Warlock,
        BountyHunter,
        Witch,
        Ninja,
        NekoKabocha,
        Madmate,
        SerialKiller,
        Innersloth,


        Mini = 150,
        Lovers,
        EvilGuesser,
        NiceGuesser,
        Jester,
        Arsonist,
        Jackal,
        Sidekick,
        Opportunist,
        Vulture,
        Lawyer,
        Pursuer,
        PlagueDoctor,
        Watcher,
        Fox,
        Immoralist,


        GM = 200,


        // don't put anything below this
        NoRole = int.MaxValue
    }

    [HarmonyPatch]
    public static class RoleData
    {
        public static Dictionary<RoleType, Type> AllRoleTypes = new Dictionary<RoleType, Type>
        {
            // Crew
            { RoleType.Sheriff, typeof(RoleBase<Sheriff>) },
            { RoleType.Lighter, typeof(RoleBase<Lighter>) },
            { RoleType.FortuneTeller, typeof(RoleBase<FortuneTeller>) },

            // Impostor
            { RoleType.Ninja, typeof(RoleBase<Ninja>) },
            { RoleType.NekoKabocha, typeof(RoleBase<NekoKabocha>) },
            { RoleType.SerialKiller, typeof(RoleBase<SerialKiller>) },

            // Neutral
            { RoleType.Opportunist, typeof(RoleBase<Opportunist>) },
            { RoleType.PlagueDoctor, typeof(RoleBase<PlagueDoctor>) },
            { RoleType.Fox, typeof(RoleBase<Fox>) },
            { RoleType.Immoralist, typeof(RoleBase<Immoralist>) },

            // Other
            { RoleType.Watcher, typeof(RoleBase<Watcher>) },
        };
    }

    public abstract class Role
    {
        public static List<Role> AllRoles = new List<Role>();
        public PlayerControl Player;
        public RoleType RoleId;

        public abstract void OnMeetingStart();
        public abstract void OnMeetingEnd();
        public abstract void FixedUpdate();
        public abstract void OnKill(PlayerControl target);
        public abstract void OnDeath(PlayerControl killer = null);
        public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
        public virtual void ResetRole() { }

        public static void ClearAll()
        {
            AllRoles = new List<Role>();
        }
    }

    [HarmonyPatch]
    public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
    {
        public static List<T> Players = new List<T>();
        public static RoleType RoleType;

        public void Init(PlayerControl player)
        {
            this.Player = player;
            Players.Add((T)this);
            AllRoles.Add(this);
        }

        public static void Remove(PlayerControl player)
        {
            EraseRole(player);
        }

        public static T Local
        {
            get
            {
                return Players.FirstOrDefault(x => x.Player == PlayerControl.LocalPlayer);
            }
        }

        public static List<PlayerControl> AllPlayers
        {
            get
            {
                return Players.Select(x => x.Player).ToList();
            }
        }

        public static List<PlayerControl> LivingPlayers
        {
            get
            {
                return Players.Select(x => x.Player).Where(x => x.isAlive()).ToList();
            }
        }

        public static List<PlayerControl> DeadPlayers
        {
            get
            {
                return Players.Select(x => x.Player).Where(x => !x.isAlive()).ToList();
            }
        }

        public static bool Exists
        {
            get { return Helpers.RolesEnabled && Players.Count > 0; }
        }

        public static T GetRole(PlayerControl player = null)
        {
            player ??= PlayerControl.LocalPlayer;
            return Players.FirstOrDefault(x => x.Player == player);
        }

        public static bool IsRole(PlayerControl player)
        {
            return Players.Any(x => x.Player == player);
        }

        public static void SetRole(PlayerControl player)
        {
            if (!IsRole(player))
            {
                T role = new T();
                role.Init(player);
            }
        }

        public static void EraseRole(PlayerControl player)
        {
            Players.DoIf(x => x.Player == player, x => x.ResetRole());
            Players.RemoveAll(x => x.Player == player && x.RoleId == RoleType);
            AllRoles.RemoveAll(x => x.Player == player && x.RoleId == RoleType);
        }

        public static void SwapRole(PlayerControl p1, PlayerControl p2)
        {
            var index = Players.FindIndex(x => x.Player == p1);
            if (index >= 0)
            {
                Players[index].Player = p2;
            }
        }
    }

    public static class RoleHelpers
    {
        public static bool IsRole(this PlayerControl player, RoleType role)
        {
            foreach (var t in RoleData.AllRoleTypes)
            {
                if (role == t.Key)
                {
                    return (bool)t.Value.GetMethod("isRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                }
            }

            switch (role)
            {
                case RoleType.Jester:
                    return Jester.jester == player;
                case RoleType.Mayor:
                    return Mayor.mayor == player;
                case RoleType.Engineer:
                    return Engineer.engineer == player;
                case RoleType.Godfather:
                    return Godfather.godfather == player;
                case RoleType.Mafioso:
                    return Mafioso.mafioso == player;
                case RoleType.Janitor:
                    return Janitor.janitor == player;
                case RoleType.Detective:
                    return Detective.detective == player;
                case RoleType.TimeMaster:
                    return TimeMaster.timeMaster == player;
                case RoleType.Medic:
                    return Medic.medic == player;
                case RoleType.Shifter:
                    return Shifter.shifter == player;
                case RoleType.Swapper:
                    return Swapper.swapper == player;
                case RoleType.Seer:
                    return Seer.seer == player;
                case RoleType.Morphling:
                    return Morphling.morphling == player;
                case RoleType.Camouflager:
                    return Camouflager.camouflager == player;
                case RoleType.Hacker:
                    return Hacker.hacker == player;
                case RoleType.Mini:
                    return Mini.mini == player;
                case RoleType.Tracker:
                    return Tracker.tracker == player;
                case RoleType.Vampire:
                    return Vampire.vampire == player;
                case RoleType.Snitch:
                    return Snitch.snitch == player;
                case RoleType.Jackal:
                    return Jackal.jackal == player;
                case RoleType.Sidekick:
                    return Sidekick.sidekick == player;
                case RoleType.Eraser:
                    return Eraser.eraser == player;
                case RoleType.Spy:
                    return Spy.spy == player;
                case RoleType.Trickster:
                    return Trickster.trickster == player;
                case RoleType.Cleaner:
                    return Cleaner.cleaner == player;
                case RoleType.Warlock:
                    return Warlock.warlock == player;
                case RoleType.SecurityGuard:
                    return SecurityGuard.securityGuard == player;
                case RoleType.Arsonist:
                    return Arsonist.arsonist == player;
                case RoleType.EvilGuesser:
                    return Guesser.evilGuesser == player;
                case RoleType.NiceGuesser:
                    return Guesser.niceGuesser == player;
                case RoleType.BountyHunter:
                    return BountyHunter.bountyHunter == player;
                case RoleType.Bait:
                    return Bait.bait == player;
                case RoleType.GM:
                    return GM.gm == player;
                case RoleType.Vulture:
                    return Vulture.vulture == player;
                case RoleType.Medium:
                    return Medium.medium == player;
                case RoleType.Witch:
                    return Witch.witch == player;
                case RoleType.Lawyer:
                    return Lawyer.lawyer == player;
                case RoleType.Pursuer:
                    return Pursuer.pursuer == player;
                default:
                    Main.Logger.LogError($"isRole: no method found for role type {role}");
                    break;
            }

            return false;
        }

        public static void ClearRole(this PlayerControl player)
        {
            // TOR Roles
            if (Jester.jester == player)
            {
                Jester.jester = null;
                return;
            }
            if (Mayor.mayor == player)
            {
                Mayor.mayor = null;
                return;
            }
            if (Engineer.engineer == player)
            {
                Engineer.engineer = null;
                return;
            }
            if (Godfather.godfather == player)
            {
                Godfather.godfather = null;
                return;
            }
            if (Mafioso.mafioso == player)
            {
                Mafioso.mafioso = null;
                return;
            }
            if (Janitor.janitor == player)
            {
                Janitor.janitor = null;
                return;
            }
            if (Detective.detective == player)
            {
                Detective.detective = null;
                return;
            }
            if (TimeMaster.timeMaster == player)
            {
                TimeMaster.timeMaster = null;
                return;
            }
            if (Medic.medic == player)
            {
                Medic.medic = null;
                return;
            }
            if (Shifter.shifter == player)
            {
                Shifter.shifter = null;
                return;
            }
            if (Swapper.swapper == player)
            {
                Swapper.swapper = null;
                return;
            }
            if (Seer.seer == player)
            {
                Seer.seer = null;
                return;
            }
            if (Morphling.morphling == player)
            {
                Morphling.morphling = null;
                return;
            }
            if (Camouflager.camouflager == player)
            {
                Camouflager.camouflager = null;
                return;
            }
            if (Hacker.hacker == player)
            {
                Hacker.hacker = null;
                return;
            }
            if (Mini.mini == player)
            {
                Mini.mini = null;
                return;
            }
            if (Tracker.tracker == player)
            {
                Tracker.tracker = null;
                return;
            }
            if (Vampire.vampire == player)
            {
                Vampire.vampire = null;
                return;
            }
            if (Snitch.snitch == player)
            {
                Snitch.snitch = null;
                return;
            }
            if (Jackal.jackal == player)
            {
                Jackal.jackal = null;
                return;
            }
            if (Sidekick.sidekick == player)
            {
                Sidekick.sidekick = null;
                return;
            }
            if (Eraser.eraser == player)
            {
                Eraser.eraser = null;
                return;
            }
            if (Spy.spy == player)
            {
                Spy.spy = null;
                return;
            }
            if (Trickster.trickster == player)
            {
                Trickster.trickster = null;
                return;
            }
            if (Cleaner.cleaner == player)
            {
                Cleaner.cleaner = null;
                return;
            }
            if (Warlock.warlock == player)
            {
                Warlock.warlock = null;
                return;
            }
            if (SecurityGuard.securityGuard == player)
            {
                SecurityGuard.securityGuard = null;
                return;
            }
            if (Arsonist.arsonist == player)
            {
                Arsonist.arsonist = null;
                return;
            }
            if (Guesser.evilGuesser == player)
            {
                Guesser.evilGuesser = null;
                return;
            }
            if (Guesser.niceGuesser == player)
            {
                Guesser.niceGuesser = null;
                return;
            }
            if (BountyHunter.bountyHunter == player)
            {
                BountyHunter.bountyHunter = null;
                return;
            }
            if (Bait.bait == player)
            {
                Bait.bait = null;
                return;
            }
            if (GM.gm == player)
            {
                GM.gm = null;
                return;
            }
            if (Vulture.vulture == player)
            {
                Vulture.vulture = null;
                return;
            }
            if (Medium.medium == player)
            {
                Medium.medium = null;
                return;
            }
            if (Witch.witch == player)
            {
                Witch.witch = null;
                return;
            }
            if (Lawyer.lawyer == player)
            {
                Lawyer.lawyer = null;
                return;
            }
            if (Pursuer.pursuer == player)
            {
                Pursuer.pursuer = null;
                return;
            }

            // GM Edition Roles
            foreach (var t in RoleData.AllRoleTypes.Values)
                t.GetMethod("Remove").Invoke(null, new[] { player });

            // TOT Roles
            CustomRole.AllRoles.ForEach(cr => cr.Players.Remove(player.Data));
        }

        public static void SetRole(this PlayerControl player, RoleType role)
        {
            player.ClearRole();

            foreach (var t in RoleData.AllRoleTypes)
            {
                if (role == t.Key)
                {
                    t.Value.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                    return;
                }
            }

            CustomRole.AllRoles.FirstOrDefault(r => r.MyRoleType == role)?.Players.Add(player.Data);

            switch (role)
            {
                case RoleType.Jester:
                    Jester.jester = player;
                    break;
                case RoleType.Mayor:
                    Mayor.mayor = player;
                    break;
                case RoleType.Engineer:
                    Engineer.engineer = player;
                    break;
                case RoleType.Godfather:
                    Godfather.godfather = player;
                    break;
                case RoleType.Mafioso:
                    Mafioso.mafioso = player;
                    break;
                case RoleType.Janitor:
                    Janitor.janitor = player;
                    break;
                case RoleType.Detective:
                    Detective.detective = player;
                    break;
                case RoleType.TimeMaster:
                    TimeMaster.timeMaster = player;
                    break;
                case RoleType.Medic:
                    Medic.medic = player;
                    break;
                case RoleType.Shifter:
                    Shifter.shifter = player;
                    break;
                case RoleType.Swapper:
                    Swapper.swapper = player;
                    break;
                case RoleType.Seer:
                    Seer.seer = player;
                    break;
                case RoleType.Morphling:
                    Morphling.morphling = player;
                    break;
                case RoleType.Camouflager:
                    Camouflager.camouflager = player;
                    break;
                case RoleType.Hacker:
                    Hacker.hacker = player;
                    break;
                case RoleType.Mini:
                    Mini.mini = player;
                    break;
                case RoleType.Tracker:
                    Tracker.tracker = player;
                    break;
                case RoleType.Vampire:
                    Vampire.vampire = player;
                    break;
                case RoleType.Snitch:
                    Snitch.snitch = player;
                    break;
                case RoleType.Jackal:
                    Jackal.jackal = player;
                    break;
                case RoleType.Sidekick:
                    Sidekick.sidekick = player;
                    break;
                case RoleType.Eraser:
                    Eraser.eraser = player;
                    break;
                case RoleType.Spy:
                    Spy.spy = player;
                    break;
                case RoleType.Trickster:
                    Trickster.trickster = player;
                    break;
                case RoleType.Cleaner:
                    Cleaner.cleaner = player;
                    break;
                case RoleType.Warlock:
                    Warlock.warlock = player;
                    break;
                case RoleType.SecurityGuard:
                    SecurityGuard.securityGuard = player;
                    break;
                case RoleType.Arsonist:
                    Arsonist.arsonist = player;
                    break;
                case RoleType.EvilGuesser:
                    Guesser.evilGuesser = player;
                    break;
                case RoleType.NiceGuesser:
                    Guesser.niceGuesser = player;
                    break;
                case RoleType.BountyHunter:
                    BountyHunter.bountyHunter = player;
                    break;
                case RoleType.Bait:
                    Bait.bait = player;
                    break;
                case RoleType.GM:
                    GM.gm = player;
                    break;
                case RoleType.Vulture:
                    Vulture.vulture = player;
                    break;
                case RoleType.Medium:
                    Medium.medium = player;
                    break;
                case RoleType.Witch:
                    Witch.witch = player;
                    break;
                case RoleType.Lawyer:
                    Lawyer.lawyer = player;
                    break;
                case RoleType.Pursuer:
                    Pursuer.pursuer = player;
                    break;
                default:
                    Main.Logger.LogError($"setRole: no method found for role type {role}");
                    return;
            }
        }

        public static void EraseRole(this PlayerControl player, RoleType role)
        {
            if (IsRole(player, role))
            {
                foreach (var t in RoleData.AllRoleTypes)
                {
                    if (role == t.Key)
                    {
                        t.Value.GetMethod("EraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                        return;
                    }
                }
                Main.Logger.LogError($"eraseRole: no method found for role type {role}");
            }
        }

        public static void EraseAllRoles(this PlayerControl player)
        {
            foreach (var t in RoleData.AllRoleTypes)
            {
                t.Value.GetMethod("EraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
            }

            CustomRole.AllRoles.ForEach(cr => cr.ClearData());

            // Crewmate roles
            if (player.IsRole(RoleType.Mayor)) Mayor.clearAndReload();
            if (player.IsRole(RoleType.Engineer)) Engineer.clearAndReload();
            if (player.IsRole(RoleType.Detective)) Detective.clearAndReload();
            if (player.IsRole(RoleType.TimeMaster)) TimeMaster.clearAndReload();
            if (player.IsRole(RoleType.Medic)) Medic.clearAndReload();
            if (player.IsRole(RoleType.Shifter)) Shifter.clearAndReload();
            if (player.IsRole(RoleType.Seer)) Seer.clearAndReload();
            if (player.IsRole(RoleType.Hacker)) Hacker.clearAndReload();
            if (player.IsRole(RoleType.Mini)) Mini.clearAndReload();
            if (player.IsRole(RoleType.Tracker)) Tracker.clearAndReload();
            if (player.IsRole(RoleType.Snitch)) Snitch.clearAndReload();
            if (player.IsRole(RoleType.Swapper)) Swapper.clearAndReload();
            if (player.IsRole(RoleType.Spy)) Spy.clearAndReload();
            if (player.IsRole(RoleType.SecurityGuard)) SecurityGuard.clearAndReload();
            if (player.IsRole(RoleType.Bait)) Bait.clearAndReload();
            if (player.IsRole(RoleType.Medium)) Medium.clearAndReload();

            // Impostor roles
            if (player.IsRole(RoleType.Morphling)) Morphling.clearAndReload();
            if (player.IsRole(RoleType.Camouflager)) Camouflager.clearAndReload();
            if (player.IsRole(RoleType.Godfather)) Godfather.clearAndReload();
            if (player.IsRole(RoleType.Mafioso)) Mafioso.clearAndReload();
            if (player.IsRole(RoleType.Janitor)) Janitor.clearAndReload();
            if (player.IsRole(RoleType.Vampire)) Vampire.clearAndReload();
            if (player.IsRole(RoleType.Eraser)) Eraser.clearAndReload();
            if (player.IsRole(RoleType.Trickster)) Trickster.clearAndReload();
            if (player.IsRole(RoleType.Cleaner)) Cleaner.clearAndReload();
            if (player.IsRole(RoleType.Warlock)) Warlock.clearAndReload();
            if (player.IsRole(RoleType.Witch)) Witch.clearAndReload();

            // Other roles
            if (player.IsRole(RoleType.Jester)) Jester.clearAndReload();
            if (player.IsRole(RoleType.Arsonist)) Arsonist.clearAndReload();
            if (player.IsRole(RoleType.Sidekick)) Sidekick.clearAndReload();
            if (player.IsRole(RoleType.BountyHunter)) BountyHunter.clearAndReload();
            if (player.IsRole(RoleType.Vulture)) Vulture.clearAndReload();
            if (player.IsRole(RoleType.Lawyer)) Lawyer.clearAndReload();
            if (player.IsRole(RoleType.Pursuer)) Pursuer.clearAndReload();
            if (Guesser.isGuesser(player.PlayerId)) Guesser.clear(player.PlayerId);


            if (player.IsRole(RoleType.Jackal))
            { // Promote Sidekick and hence override the the Jackal or erase Jackal
                if (Sidekick.promotesToJackal && Sidekick.sidekick != null && Sidekick.sidekick.isAlive())
                {
                    RPCProcedure.sidekickPromotes();
                }
                else
                {
                    Jackal.clearAndReload();
                }
            }
        }


        // TODO
        public static void SwapRoles(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in RoleData.AllRoleTypes)
            {
                if (player.IsRole(t.Key))
                {
                    t.Value.GetMethod("SwapRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
                }
            }

            if (player.IsRole(RoleType.Mayor)) Mayor.mayor = target;
            if (player.IsRole(RoleType.Engineer)) Engineer.engineer = target;
            if (player.IsRole(RoleType.Detective)) Detective.detective = target;
            if (player.IsRole(RoleType.TimeMaster)) TimeMaster.timeMaster = target;
            if (player.IsRole(RoleType.Medic)) Medic.medic = target;
            if (player.IsRole(RoleType.Swapper)) Swapper.swapper = target;
            if (player.IsRole(RoleType.Seer)) Seer.seer = target;
            if (player.IsRole(RoleType.Hacker)) Hacker.hacker = target;
            if (player.IsRole(RoleType.Tracker)) Tracker.tracker = target;
            if (player.IsRole(RoleType.Snitch)) Snitch.snitch = target;
            if (player.IsRole(RoleType.Spy)) Spy.spy = target;
            if (player.IsRole(RoleType.SecurityGuard)) SecurityGuard.securityGuard = target;
            if (player.IsRole(RoleType.Bait))
            {
                Bait.bait = target;
                if (Bait.bait.Data.IsDead) Bait.reported = true;
            }
            if (player.IsRole(RoleType.Medium)) Medium.medium = target;
            if (player.IsRole(RoleType.Godfather)) Godfather.godfather = target;
            if (player.IsRole(RoleType.Mafioso)) Mafioso.mafioso = target;
            if (player.IsRole(RoleType.Janitor)) Janitor.janitor = target;
            if (player.IsRole(RoleType.Morphling)) Morphling.morphling = target;
            if (player.IsRole(RoleType.Camouflager)) Camouflager.camouflager = target;
            if (player.IsRole(RoleType.Vampire)) Vampire.vampire = target;
            if (player.IsRole(RoleType.Eraser)) Eraser.eraser = target;
            if (player.IsRole(RoleType.Trickster)) Trickster.trickster = target;
            if (player.IsRole(RoleType.Cleaner)) Cleaner.cleaner = target;
            if (player.IsRole(RoleType.Warlock)) Warlock.warlock = target;
            if (player.IsRole(RoleType.BountyHunter)) BountyHunter.bountyHunter = target;
            if (player.IsRole(RoleType.Witch)) Witch.witch = target;
            if (player.IsRole(RoleType.Mini)) Mini.mini = target;
            if (player.IsRole(RoleType.EvilGuesser)) Guesser.evilGuesser = target;
            if (player.IsRole(RoleType.NiceGuesser)) Guesser.niceGuesser = target;
            if (player.IsRole(RoleType.Jester)) Jester.jester = target;
            if (player.IsRole(RoleType.Arsonist)) Arsonist.arsonist = target;
            if (player.IsRole(RoleType.Jackal)) Jackal.jackal = target;
            if (player.IsRole(RoleType.Sidekick)) Sidekick.sidekick = target;
            if (player.IsRole(RoleType.Vulture)) Vulture.vulture = target;
            if (player.IsRole(RoleType.Lawyer)) Lawyer.lawyer = target;
            if (player.IsRole(RoleType.Pursuer)) Pursuer.pursuer = target;
        }

        public static void OnKill(this PlayerControl player, PlayerControl target)
        {
            Role.AllRoles.DoIf(x => x.Player == player, x => x.OnKill(target));
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnKill(target));
        }

        public static void OnDeath(this PlayerControl player, PlayerControl killer)
        {
            Role.AllRoles.DoIf(x => x.Player == player, x => x.OnDeath(killer));
            Modifier.allModifiers.DoIf(x => x.player == player, x => x.OnDeath(killer));

            // Lover suicide trigger on exile/death
            if (player.isLovers())
                Lovers.killLovers(player, killer);

            RPCProcedure.updateMeeting(player.PlayerId, true);
        }

        public static void InitTOTRoles()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            currentAssembly.GetTypes().Where(t => t.IsDefined(typeof(RoleAutoInitializeAttribute), true) && typeof(CustomRole).IsAssignableFrom(t)).Do(t => Activator.CreateInstance(t, true));
        }
    }
}