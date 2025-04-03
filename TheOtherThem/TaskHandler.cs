using AmongUs.GameOptions;
using System;
using static TheOtherThem.TheOtherRolesGM;

namespace TheOtherThem
{
    [HarmonyPatch]
    public static class TaskHandler
    {

        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
        public static class NormalPlayerTaskPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                bool showArrows = !MapOptions.HideTaskArrows && !__instance.IsComplete && __instance.TaskStep > 0;
                __instance.Arrow?.gameObject?.SetActive(showArrows);
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.FixedUpdate))]
        public static class AirshipUploadTaskPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                bool showArrows = !MapOptions.HideTaskArrows && !__instance.IsComplete && __instance.TaskStep > 0;
                __instance.Arrows?.DoIf(x => x != null, x => x.gameObject?.SetActive(showArrows));
            }
        }

        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrowAndLocation))]
        public static class NormalPlayerTaskUpdateArrowPatch
        {
            public static void Postfix(NormalPlayerTask __instance)
            {
                if (MapOptions.HideTaskArrows)
                    __instance.Arrow?.gameObject?.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(AirshipUploadTask), nameof(AirshipUploadTask.UpdateArrowAndLocation))]
        public static class AirshipUploadTaskUpdateArrowPatch
        {
            public static void Postfix(AirshipUploadTask __instance)
            {
                if (MapOptions.HideTaskArrows)
                {
                    __instance.Arrows?.DoIf(x => x != null, x => x.gameObject?.SetActive(false));
                }
            }
        }

        public static Tuple<int, int> GetTaskInfo(NetworkedPlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                (GameOptionsManager.Instance.CurrentGameOptions.Cast<NormalGameOptionsV09>().GhostsDoTasks || !playerInfo.IsDead) &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
                !(playerInfo.Object.IsGM() && !GM.hasTasks) &&
                !(playerInfo.Object.IsInLove() && !Lovers.HasTasks) &&
                !playerInfo.Object.HasFakeTasks()
                )
            {

                for (int j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    TotalTasks++;
                    if (playerInfo.Tasks[j].Complete)
                    {
                        CompletedTasks++;
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch
        {
            private static bool Prefix(GameData __instance)
            {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++)
                {
                    NetworkedPlayerInfo playerInfo = __instance.AllPlayers[i];
                    if (playerInfo.Object &&
                        ((playerInfo.Object?.IsInLove() == true && !Lovers.TasksCount) ||
                         (playerInfo.PlayerId == Shifter.shifter?.PlayerId && Shifter.isNeutral) || // Neutral shifter has tasks, but they don't count
                          playerInfo.PlayerId == Lawyer.lawyer?.PlayerId || // Tasks of the Lawyer do not count
                         (playerInfo.PlayerId == Pursuer.pursuer?.PlayerId && Pursuer.pursuer.Data.IsDead) || // Tasks of the Pursuer only count, if he's alive
                          playerInfo.Object?.IsRole(RoleType.Fox) == true ||
                         (Madmate.HasTasks && playerInfo.Object?.HasModifier(ModifierType.Madmate) == true)
                        )
                    )
                        continue;
                    var (playerCompleted, playerTotal) = GetTaskInfo(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
                return false;
            }
        }


    }
}
