using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheOtherThem.Objects;
using TheOtherThem.Patches;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.GameHistory;

namespace TheOtherThem
{
    [HarmonyPatch]
    public class Madmate : ModifierBase<Madmate>
    {
        public static Color color = Palette.ImpostorRed;

        public enum MadmateType
        {
            Simple = 0,
            WithRole = 1,
            Random = 2,
        }

        public enum MadmateAbility
        {
            None = 0,
            Fanatic = 1,
        }

        public static bool canEnterVents { get { return CustomOptionHolder.madmateCanEnterVents.GetBool(); } }
        public static bool hasImpostorVision { get { return CustomOptionHolder.madmateHasImpostorVision.GetBool(); } }
        public static bool canSabotage { get { return CustomOptionHolder.madmateCanSabotage.GetBool(); } }
        public static bool canFixComm { get { return CustomOptionHolder.madmateCanFixComm.GetBool(); } }

        public static MadmateType madmateType { get { return (MadmateType)CustomOptionHolder.madmateType.GetSelection(); } }
        public static MadmateAbility madmateAbility { get { return (MadmateAbility)CustomOptionHolder.madmateAbility.GetSelection(); } }
        public static RoleType fixedRole { get { return CustomOptionHolder.madmateFixedRole.role; } }

        public static int numCommonTasks { get { return CustomOptionHolder.madmateTasks.commonTasks; } }
        public static int numLongTasks { get { return CustomOptionHolder.madmateTasks.longTasks; } }
        public static int numShortTasks { get { return CustomOptionHolder.madmateTasks.shortTasks; } }

        public static bool HasTasks { get { return madmateAbility == MadmateAbility.Fanatic; } }

        public static string prefix
        {
            get
            {
                return ModTranslation.GetString("madmatePrefix");
            }
        }

        public static string fullName
        {
            get
            {
                return ModTranslation.GetString("madmate");
            }
        }

        public static List<RoleType> validRoles = new List<RoleType>
        {
            RoleType.NoRole, // NoRole = off
            RoleType.Shifter,
            RoleType.Mayor,
            RoleType.Engineer,
            RoleType.Sheriff,
            RoleType.Lighter,
            RoleType.Detective,
            RoleType.TimeMaster,
            RoleType.Medic,
            RoleType.Swapper,
            RoleType.Seer,
            RoleType.Hacker,
            RoleType.Tracker,
            RoleType.SecurityGuard,
            RoleType.Bait,
            RoleType.Medium,
            RoleType.Mini,
            RoleType.NiceGuesser,
            RoleType.Watcher,
        };

        public static List<PlayerControl> candidates
        {
            get
            {
                List<PlayerControl> crewHasRole = new List<PlayerControl>();
                List<PlayerControl> crewNoRole = new List<PlayerControl>();
                List<PlayerControl> validCrewmates = new List<PlayerControl>();

                foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsCrewmate() && !hasModifier(x)).ToList())
                {
                    var info = RoleInfo.GetRoleInfoForPlayer(player, includeHidden: true);
                    if (info.Contains(RoleInfo.crewmate))
                    {
                        crewNoRole.Add(player);
                        validCrewmates.Add(player);
                    }
                    else if (info.Any(x => validRoles.Contains(x.MyRoleType)))
                    {
                        if (fixedRole == RoleType.NoRole || info.Any(x => x.MyRoleType == fixedRole))
                            crewHasRole.Add(player);

                        validCrewmates.Add(player);
                    }
                }

                if (madmateType == MadmateType.Simple) return crewNoRole;
                else if (madmateType == MadmateType.WithRole && crewHasRole.Count > 0) return crewHasRole;
                else if (madmateType == MadmateType.Random) return validCrewmates;
                return validCrewmates;
            }
        }

        public Madmate()
        {
            ModType = ModId = ModifierType.Madmate;
        }

        public override void OnDeath(PlayerControl killer = null)
        {
            Player.ClearAllTasks();
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch
        {
            public static void Postfix()
            {
                if (HasTasks && hasModifier(PlayerControl.LocalPlayer))
                {
                    local.AssignTasks();
                }
            }
        }

        public void AssignTasks()
        {
            Player.GenerateAndAssignTasks(numCommonTasks, numShortTasks, numLongTasks);
        }

        public static bool knowsImpostors(PlayerControl player)
        {
            return HasTasks && hasModifier(player) && tasksComplete(player);
        }

        public static bool tasksComplete(PlayerControl player)
        {
            if (!HasTasks) return false;

            int counter = 0;
            int totalTasks = numCommonTasks + numLongTasks + numShortTasks;
            if (totalTasks == 0) return true;
            foreach (var task in player.Data.Tasks)
            {
                if (task.Complete)
                {
                    counter++;
                }
            }
            return counter == totalTasks;
        }

        public static void Clear()
        {
            players = new List<Madmate>();
        }
    }
}