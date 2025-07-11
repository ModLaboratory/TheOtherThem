  
using HarmonyLib;
using static TheOtherThem.TheOtherRoles;
using static TheOtherThem.TheOtherRolesGM;
using static TheOtherThem.GameHistory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hazel;
using System;
using System.Text;
using TMPro;

namespace TheOtherThem.Patches
{
    public enum CustomGameOverReason
    {
        LoversWin = 10,
        TeamJackalWin = 11,
        MiniLose = 12,
        JesterWin = 13,
        ArsonistWin = 14,
        VultureWin = 15,
        LawyerSoloWin = 16,
        PlagueDoctorWin = 17,
        FoxWin = 18,
        IdealistWin = 19,
    }

    enum WinCondition
    {
        Default,
        LoversTeamWin,
        LoversSoloWin,
        JesterWin,
        JackalWin,
        MiniLose,
        ArsonistWin,
        OpportunistWin,
        VultureWin,
        LawyerSoloWin,
        AdditionalLawyerBonusWin,
        AdditionalLawyerStolenWin,
        AdditionalAlivePursuerWin,
        PlagueDoctorWin,
        FoxWin,

        EveryoneDied,
    }

    enum FinalStatus
    {
        Alive,
        Torched,
        Spelled,
        Sabotage,
        Exiled,
        Dead,
        Suicide,
        Misfire,
        Revenge,
        Diseased,
        Divined,
        GMExecuted,
        Disconnected
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
    static class EndGamePatch
    {
        static bool Prefix() => !Main.DebugMode.Value; 
    }

    static class AdditionalTempData
    {
        // Should be implemented using a proper GameOverReason in the future
        public static WinCondition WinCondition { get; set; } = WinCondition.Default;
        public static List<WinCondition> AdditionalWinConditions { get; set; } = new();
        public static List<PlayerRoleInfo> PlayerRoles { get; set; } = new();
        public static bool IsGM { get; set; } = false;
        public static GameOverReason GameOverReason { get; set; }

        public static Dictionary<int, PlayerControl> PlagueDoctorInfected { get; set; } = new();
        public static Dictionary<int, float> PlagueDoctorProgress { get; set; } = new();

        public static void Clear()
        {
            PlayerRoles.Clear();
            AdditionalWinConditions.Clear();
            WinCondition = WinCondition.Default;
            IsGM = false;
        }

        internal class PlayerRoleInfo
        {
            public string PlayerName { get; set; }
            public string NameSuffix { get; set; }
            public List<RoleInfo> Roles { get; set; }
            public string RoleString { get; set; }
            public int TasksCompleted { get; set; }
            public int TasksTotal { get; set; }
            public FinalStatus Status { get; set; }
            public int PlayerId { get; set; }
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
    public class OnGameEndPatch
    {
        public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            Camouflager.resetCamouflage();
            Morphling.resetMorph();

            AdditionalTempData.GameOverReason = endGameResult.GameOverReason;
            if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorsByKill;
        }

        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
        {
            var gameOverReason = AdditionalTempData.GameOverReason;

            // 狐の勝利条件を満たしたか確認する
            bool isFoxAlive = Fox.isFoxAlive();
            bool isFoxCompletedTasks = Fox.isFoxCompletedTasks(); // 生存中の狐が1匹でもタスクを全て終えていること
            if (isFoxAlive && isFoxCompletedTasks) {
                // タスク勝利の場合はオプションの設定次第
                if (gameOverReason == GameOverReason.CrewmatesByTask && !Fox.crewWinsByTasks)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }

                // 第三陣営の勝利以外の場合に狐が生存していたら狐の勝ち
                else if (gameOverReason != (GameOverReason)CustomGameOverReason.PlagueDoctorWin &&
                    gameOverReason != (GameOverReason)CustomGameOverReason.ArsonistWin &&
                    gameOverReason != (GameOverReason)CustomGameOverReason.JesterWin &&
                    gameOverReason != (GameOverReason)CustomGameOverReason.VultureWin &&
                    gameOverReason != (GameOverReason)GameOverReason.CrewmatesByTask)
                {
                    gameOverReason = (GameOverReason)CustomGameOverReason.FoxWin;
                }
            }
            AdditionalTempData.Clear();

            //foreach (var pc in PlayerControl.AllPlayerControls)
            var excludeRoles = new RoleType[] { RoleType.Lovers };
            foreach (var p in GameData.Instance.AllPlayers)
            {
                //var p = pc.Data;
                var roles = RoleInfo.GetRoleInfoForPlayer(p.Object, excludeRoles, includeHidden: true);
                var (tasksCompleted, tasksTotal) = TaskHandler.GetTaskInfo(p);
                var finalStatus = FinalStatuses[p.PlayerId] =
                    p.Disconnected == true ? FinalStatus.Disconnected :
                    FinalStatuses.ContainsKey(p.PlayerId) ? FinalStatuses[p.PlayerId] :
                    p.IsDead == true ? FinalStatus.Dead :
                    gameOverReason == GameOverReason.ImpostorsBySabotage && !p.Role.IsImpostor ? FinalStatus.Sabotage :
                    FinalStatus.Alive;

                if (gameOverReason == GameOverReason.CrewmatesByTask && p.Object.IsCrewmate()) tasksCompleted = tasksTotal;

                AdditionalTempData.PlayerRoles.Add(new AdditionalTempData.PlayerRoleInfo()
                {
                    PlayerName = p.PlayerName,
                    PlayerId = p.PlayerId,
                    NameSuffix = Lovers.GetIcon(p.Object),
                    Roles = roles,
                    RoleString = RoleInfo.GetRolesString(p.Object, true, excludeRoles, true),
                    TasksTotal = tasksTotal,
                    TasksCompleted = tasksCompleted,
                    Status = finalStatus,
                });
            }

            AdditionalTempData.IsGM = CustomOptionHolder.gmEnabled.GetBool() && PlayerControl.LocalPlayer.IsGM();
            AdditionalTempData.PlagueDoctorInfected = PlagueDoctor.infected;
            AdditionalTempData.PlagueDoctorProgress = PlagueDoctor.progress;


            // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
            List<PlayerControl> notWinners = new List<PlayerControl>();
            if (Jester.jester != null) notWinners.Add(Jester.jester);
            if (Sidekick.sidekick != null) notWinners.Add(Sidekick.sidekick);
            if (Jackal.jackal != null) notWinners.Add(Jackal.jackal);
            if (Arsonist.arsonist != null) notWinners.Add(Arsonist.arsonist);
            if (Vulture.vulture != null) notWinners.Add(Vulture.vulture);
            if (Lawyer.lawyer != null) notWinners.Add(Lawyer.lawyer);
            if (Pursuer.pursuer != null) notWinners.Add(Pursuer.pursuer);

            notWinners.AddRange(Jackal.formerJackals);
            notWinners.AddRange(Madmate.allPlayers);
            notWinners.AddRange(Opportunist.AllPlayers);
            notWinners.AddRange(PlagueDoctor.AllPlayers);
            notWinners.AddRange(Fox.AllPlayers);
            notWinners.AddRange(Immoralist.AllPlayers);

            // Neutral shifter can't win
            if (Shifter.shifter != null && Shifter.isNeutral) notWinners.Add(Shifter.shifter);

            // GM can't win at all, and we're treating lovers as a separate class
            if (GM.gm != null) notWinners.Add(GM.gm);

            if (Lovers.SeparateTeam)
            {
                foreach (var couple in Lovers.Couples)
                {
                    notWinners.Add(couple.Lover1);
                    notWinners.Add(couple.Lover2);
                }
            }

            List<CachedPlayerData> winnersToRemove = new List<CachedPlayerData>();
            foreach (CachedPlayerData winner in EndGameResult.CachedWinners)
            {
                if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName)) winnersToRemove.Add(winner);
            }
            foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);

            bool saboWin = gameOverReason == GameOverReason.ImpostorsBySabotage;

            bool jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
            bool arsonistWin = Arsonist.arsonist != null && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
            bool miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
            bool loversWin = Lovers.AnyAlive && !(Lovers.SeparateTeam && gameOverReason == GameOverReason.CrewmatesByTask);
            bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin && ((Jackal.jackal != null && Jackal.jackal.IsAlive()) || (Sidekick.sidekick != null && !Sidekick.sidekick.IsAlive()));
            bool vultureWin = Vulture.vulture != null && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
            bool lawyerSoloWin = Lawyer.lawyer != null && gameOverReason == (GameOverReason)CustomGameOverReason.LawyerSoloWin;
            bool plagueDoctorWin = PlagueDoctor.Exists && gameOverReason == (GameOverReason)CustomGameOverReason.PlagueDoctorWin;
            bool foxWin = Fox.Exists && gameOverReason == (GameOverReason)CustomGameOverReason.FoxWin;
            bool everyoneDead = AdditionalTempData.PlayerRoles.All(x => x.Status != FinalStatus.Alive);

            
            // Mini lose
            if (miniLose)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Mini.mini.Data);
                wpd.IsYou = false; // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.WinCondition = WinCondition.MiniLose;
            }

            // Jester win
            else if (jesterWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Jester.jester.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.WinCondition = WinCondition.JesterWin;
            }

            // Arsonist win
            else if (arsonistWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Arsonist.arsonist.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.WinCondition = WinCondition.ArsonistWin;
            }

            else if (plagueDoctorWin)
            {
                foreach (var pd in PlagueDoctor.Players)
                {
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    CachedPlayerData wpd = new CachedPlayerData(pd.Player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                    AdditionalTempData.WinCondition = WinCondition.PlagueDoctorWin;
                }
            }

            else if (everyoneDead)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                AdditionalTempData.WinCondition = WinCondition.EveryoneDied;
            }

            // Vulture win
            else if (vultureWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Vulture.vulture.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.WinCondition = WinCondition.VultureWin;
            }

            // Lovers win conditions
            else if (loversWin)
            {
                // Double win for lovers, crewmates also win
                if (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.SeparateTeam && Lovers.AnyNonKillingCouples)
                {
                    AdditionalTempData.WinCondition = WinCondition.LoversTeamWin;
                    AdditionalTempData.AdditionalWinConditions.Add(WinCondition.LoversTeamWin);
                }
                // Lovers solo win
                else
                {
                    AdditionalTempData.WinCondition = WinCondition.LoversSoloWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();

                    foreach (var couple in Lovers.Couples)
                    {
                        if (couple.ExistingAndAlive)
                        {
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(couple.Lover1.Data));
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(couple.Lover2.Data));
                        }
                    }
                }
            }

            // Jackal win condition (should be implemented using a proper GameOverReason in the future)
            else if (teamJackalWin)
            {
                // Jackal wins if nobody except jackal is alive
                AdditionalTempData.WinCondition = WinCondition.JackalWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Jackal.jackal.Data);
                wpd.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpd);
                // If there is a sidekick. The sidekick also wins
                if (Sidekick.sidekick != null)
                {
                    CachedPlayerData wpdSidekick = new CachedPlayerData(Sidekick.sidekick.Data);
                    wpdSidekick.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdSidekick);
                }
                foreach (var player in Jackal.formerJackals)
                {
                    CachedPlayerData wpdFormerJackal = new CachedPlayerData(player.Data);
                    wpdFormerJackal.IsImpostor = false;
                    EndGameResult.CachedWinners.Add(wpdFormerJackal);
                }
            }
            // Lawyer solo win 
            else if (lawyerSoloWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                CachedPlayerData wpd = new CachedPlayerData(Lawyer.lawyer.Data);
                EndGameResult.CachedWinners.Add(wpd);
                AdditionalTempData.WinCondition = WinCondition.LawyerSoloWin;
            }
            else if (foxWin)
            {
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (var fox in Fox.Players)
                {
                    CachedPlayerData wpd = new CachedPlayerData(fox.Player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                foreach (var immoralist in Immoralist.Players)
                {
                    CachedPlayerData wpd = new CachedPlayerData(immoralist.Player.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
                AdditionalTempData.WinCondition = WinCondition.FoxWin;
            }

            // Madmate win with impostors
            if (Madmate.exists && EndGameResult.CachedWinners.ToArray().Any(x => x.IsImpostor))
            {
                foreach (var p in Madmate.allPlayers)
                {
                    CachedPlayerData wpd = new CachedPlayerData(p.Data);
                    EndGameResult.CachedWinners.Add(wpd);
                }
            }

            // Possible Additional winner: Lawyer
            if (!lawyerSoloWin && Lawyer.lawyer != null && Lawyer.target != null && Lawyer.target.IsAlive())
            {
                CachedPlayerData winningClient = null;
                foreach (CachedPlayerData winner in EndGameResult.CachedWinners)
                {
                    if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                        winningClient = winner;
                }

                if (winningClient != null)
                { // The Lawyer wins if the client is winning (and alive, but if he wasn't the Lawyer shouldn't exist anymore)
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                    if (Lawyer.lawyer.IsAlive())
                    { // The Lawyer steals the clients win
                        EndGameResult.CachedWinners.Remove(winningClient);
                        AdditionalTempData.AdditionalWinConditions.Add(WinCondition.AdditionalLawyerStolenWin);
                    }
                    else
                    { // The Lawyer wins together with the client
                        AdditionalTempData.AdditionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin);
                    }
                }
            }

            // Extra win conditions for non-impostor roles
            if (!saboWin)
            {
                bool oppWin = false;
                foreach (var p in Opportunist.LivingPlayers)
                {
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == p.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    oppWin = true;
                }
                if (oppWin)
                    AdditionalTempData.AdditionalWinConditions.Add(WinCondition.OpportunistWin);

                // Possible Additional winner: Pursuer
                if (Pursuer.pursuer != null && Pursuer.pursuer.IsAlive())
                {
                    if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Pursuer.pursuer.Data.PlayerName))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Pursuer.pursuer.Data));
                    AdditionalTempData.AdditionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
                }
            }

            foreach (CachedPlayerData wpd in EndGameResult.CachedWinners)
            {
                wpd.IsDead = wpd.IsDead || AdditionalTempData.PlayerRoles.Any(x => x.PlayerName == wpd.PlayerName && x.Status != FinalStatus.Alive);
            }

            // Reset Settings
            RpcProcedure.ResetVariables();
        }

        public static class EndGameNavigationPatch
        {
            public static TMPro.TMP_Text textRenderer;

            [HarmonyPatch(typeof(EndGameNavigation), nameof(EndGameNavigation.ShowProgression))]
            public class ShowProgressionPatch
            {
                public static void Prefix()
                {
                    if (textRenderer != null)
                    {
                        textRenderer.gameObject.SetActive(false);
                    }
                }
            }

            [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
            public class EndGameManagerSetUpPatch
            {
                public static void Postfix(EndGameManager __instance)
                {
                    // Delete and readd PoolablePlayers always showing the name and role of the player
                    foreach (PoolablePlayer pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
                    {
                        UnityEngine.Object.Destroy(pb.gameObject);
                    }
                    int num = Mathf.CeilToInt(7.5f);
                    List<CachedPlayerData> list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate (CachedPlayerData b)
                    {
                        if (!b.IsYou)
                        {
                            return 0;
                        }
                        return -1;
                    }).ToList<CachedPlayerData>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        CachedPlayerData CachedPlayerData2 = list[i];
                        int num2 = (i % 2 == 0) ? -1 : 1;
                        int num3 = (i + 1) / 2;
                        float num4 = (float)num3 / (float)num;
                        float num5 = Mathf.Lerp(1f, 0.75f, num4);
                        float num6 = (float)((i == 0) ? -8 : -1);
                        PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, __instance.transform);
                        poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
                        float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
                        Vector3 vector = new Vector3(num7, num7, 1f);
                        poolablePlayer.transform.localScale = vector;
                        poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);
                        if (CachedPlayerData2.IsDead)
                        {
                            poolablePlayer.SetBodyAsGhost();
                            poolablePlayer.SetDeadFlipX(i % 2 == 0);
                        }
                        else
                        {
                            poolablePlayer.SetFlipX(i % 2 == 0);
                        }

                        var nameText = poolablePlayer.transform.FindChild("Names").FindChild("NameText_TMP").GetComponent<TextMeshPro>();
                        nameText.color = Color.white;
                        nameText.lineSpacing *= 0.7f;
                        nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
                        nameText.transform.localPosition = new Vector3(nameText.transform.localPosition.x, nameText.transform.localPosition.y, -15f);
                        nameText.text = CachedPlayerData2.PlayerName;

                        foreach (var data in AdditionalTempData.PlayerRoles)
                        {
                            if (data.PlayerName != CachedPlayerData2.PlayerName) continue;
                            nameText.text += data.NameSuffix + $"\n<size=80%>{data.RoleString}</size>";
                        }
                    }

                    // Additional code
                    GameObject bonusTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                    bonusTextObject.transform.position = new Vector3(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.8f, __instance.WinText.transform.position.z);
                    bonusTextObject.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                    textRenderer = bonusTextObject.GetComponent<TMPro.TMP_Text>();
                    textRenderer.text = "";

                    if (AdditionalTempData.IsGM)
                    {
                        __instance.WinText.text = ModTranslation.GetString("gmGameOver");
                        __instance.WinText.color = GM.color;
                    }

                    string bonusText = "";

                    if (AdditionalTempData.WinCondition == WinCondition.JesterWin)
                    {
                        bonusText = "jesterWin";
                        textRenderer.color = Jester.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Jester.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.ArsonistWin)
                    {
                        bonusText = "arsonistWin";
                        textRenderer.color = Arsonist.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Arsonist.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.VultureWin)
                    {
                        bonusText = "vultureWin";
                        textRenderer.color = Vulture.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Vulture.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.LawyerSoloWin)
                    {
                        bonusText = "lawyerWin";
                        textRenderer.color = Lawyer.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Lawyer.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.PlagueDoctorWin)
                    {
                        bonusText = "plagueDoctorWin";
                        textRenderer.color = PlagueDoctor.color;
                        __instance.BackgroundBar.material.SetColor("_Color", PlagueDoctor.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.FoxWin)
                    {
                        bonusText = "foxWin";
                        textRenderer.color = Fox.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Fox.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.LoversTeamWin)
                    {
                        bonusText = "crewWin";
                        textRenderer.color = Lovers.Color;
                        __instance.BackgroundBar.material.SetColor("_Color", Lovers.Color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.LoversSoloWin)
                    {
                        bonusText = "loversWin";
                        textRenderer.color = Lovers.Color;
                        __instance.BackgroundBar.material.SetColor("_Color", Lovers.Color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.JackalWin)
                    {
                        bonusText = "jackalWin";
                        textRenderer.color = Jackal.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Jackal.color);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.EveryoneDied)
                    {
                        bonusText = "everyoneDied";
                        textRenderer.color = Palette.DisabledGrey;
                        __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
                    }
                    else if (AdditionalTempData.WinCondition == WinCondition.MiniLose)
                    {
                        bonusText = "miniDied";
                        textRenderer.color = Mini.color;
                        __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
                    }
                    else if (AdditionalTempData.GameOverReason == GameOverReason.CrewmatesByTask || AdditionalTempData.GameOverReason == GameOverReason.CrewmatesByVote)
                    {
                        bonusText = "crewWin";
                        textRenderer.color = Palette.White;
                    }
                    else if (AdditionalTempData.GameOverReason == GameOverReason.ImpostorsByKill || AdditionalTempData.GameOverReason == GameOverReason.ImpostorsBySabotage || AdditionalTempData.GameOverReason == GameOverReason.ImpostorsByVote)
                    {
                        bonusText = "impostorWin";
                        textRenderer.color = Palette.ImpostorRed;
                    }

                    string extraText = "";
                    foreach (WinCondition w in AdditionalTempData.AdditionalWinConditions)
                    {
                        switch (w)
                        {
                            case WinCondition.OpportunistWin:
                                extraText += ModTranslation.GetString("opportunistExtra");
                                break;
                            case WinCondition.LoversTeamWin:
                                extraText += ModTranslation.GetString("loversExtra");
                                break;
                            case WinCondition.AdditionalAlivePursuerWin:
                                extraText += ModTranslation.GetString("pursuerExtra");
                                break;
                            default:
                                break;
                        }
                    }

                    if (extraText.Length > 0)
                    {
                        textRenderer.text = string.Format(ModTranslation.GetString(bonusText + "Extra"), extraText);
                    }
                    else
                    {
                        textRenderer.text = ModTranslation.GetString(bonusText);
                    }

                    foreach (WinCondition cond in AdditionalTempData.AdditionalWinConditions)
                    {
                        switch (cond)
                        {
                            case WinCondition.AdditionalLawyerStolenWin:
                                textRenderer.text += $"\n{Helpers.ColorString(Lawyer.color, ModTranslation.GetString("lawyerExtraStolen"))}";
                                break;
                            case WinCondition.AdditionalLawyerBonusWin:
                                textRenderer.text += $"\n{Helpers.ColorString(Lawyer.color, ModTranslation.GetString("lawyerExtraBonus"))}";
                                break;
                        }
                    }

                    if (MapOptions.ShowRoleSummary)
                    {
                        var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                        GameObject roleSummary = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
                        roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f, position.y - 0.1f, -14f);
                        roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                        var roleSummaryText = new StringBuilder();
                        roleSummaryText.AppendLine(ModTranslation.GetString("roleSummaryText"));
                        AdditionalTempData.PlayerRoles.Sort((x, y) =>
                        {
                            RoleInfo roleX = x.Roles.FirstOrDefault();
                            RoleInfo roleY = y.Roles.FirstOrDefault();
                            RoleType idX = roleX == null ? RoleType.NoRole : roleX.MyRoleType;
                            RoleType idY = roleY == null ? RoleType.NoRole : roleY.MyRoleType;

                            if (x.Status == y.Status)
                            {
                                if (idX == idY)
                                {
                                    return x.PlayerName.CompareTo(y.PlayerName);
                                }
                                return idX.CompareTo(idY);
                            }
                            return x.Status.CompareTo(y.Status);

                        });

                        bool plagueExists = AdditionalTempData.PlayerRoles.Any(x => x.Roles.Contains(RoleInfo.plagueDoctor));
                        foreach (var data in AdditionalTempData.PlayerRoles)
                        {
                            var taskInfo = data.TasksTotal > 0 ? $"<color=#FAD934FF>{data.TasksCompleted}/{data.TasksTotal}</color>" : "";
                            string aliveDead = ModTranslation.GetString("roleSummary" + data.Status.ToString(), def: "-");
                            string result = $"{data.PlayerName + data.NameSuffix}<pos=18.5%>{taskInfo}<pos=25%>{aliveDead}<pos=34%>{data.RoleString}";
                            if (plagueExists && !data.Roles.Contains(RoleInfo.plagueDoctor))
                            {
                                result += "<pos=52.5%>";
                                if (AdditionalTempData.PlagueDoctorInfected.ContainsKey(data.PlayerId))
                                {
                                    result += Helpers.ColorString(Color.red, ModTranslation.GetString("plagueDoctorInfectedText"));
                                }
                                else
                                {
                                    float progress = AdditionalTempData.PlagueDoctorProgress.ContainsKey(data.PlayerId) ? AdditionalTempData.PlagueDoctorProgress[data.PlayerId] : 0f;
                                    result += PlagueDoctor.getProgressString(progress);
                                }
                            }
                            roleSummaryText.AppendLine(result);
                        }

                        TMPro.TMP_Text roleSummaryTextMesh = roleSummary.GetComponent<TMPro.TMP_Text>();
                        roleSummaryTextMesh.alignment = TMPro.TextAlignmentOptions.TopLeft;
                        roleSummaryTextMesh.color = Color.white;
                        roleSummaryTextMesh.outlineWidth *= 1.2f;
                        roleSummaryTextMesh.fontSizeMin = 1.25f;
                        roleSummaryTextMesh.fontSizeMax = 1.25f;
                        roleSummaryTextMesh.fontSize = 1.25f;

                        var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                        roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                        roleSummaryTextMesh.text = roleSummaryText.ToString();
                    }
                    AdditionalTempData.Clear();
                }
            }

            [HarmonyPatch(typeof(LogicGameFlow), nameof(LogicGameFlow.CheckEndCriteria))]
            public static class CheckEndCriteriaPatch
            {
                public static List<Func<ShipStatus, bool>> GeneralEndCheckingHandler { get; } = new()
                {
                    CheckAndEndGameForMiniLose,
                    CheckAndEndGameForJesterWin,
                    CheckAndEndGameForLawyerMeetingWin,
                    CheckAndEndGameForArsonistWin,
                    CheckAndEndGameForVultureWin,
                    CheckAndEndGameForPlagueDoctorWin,
                    CheckAndEndGameForSabotageWin,
                    CheckAndEndGameForTaskWin,
                };

                public static List<Func<ShipStatus, PlayerStatistics, bool>> StatisticalEndCheckingHandler { get; } = new()
                {
                    CheckAndEndGameForLoverWin,
                    CheckAndEndGameForJackalWin,
                    CheckAndEndGameForImpostorWin,
                    CheckAndEndGameForCrewmateWin
                };

                public static bool Prefix()
                {
                    var ship = ShipStatus.Instance;

                    if (Main.DebugMode.Value) return false;
                    if (!GameData.Instance) return false;
                    if (DestroyableSingleton<TutorialManager>.InstanceExists) return true; // InstanceExists | Don't check Custom Criteria when in Tutorial
                    if (HudManager.Instance.IsIntroDisplayed) return false;

                    var statistics = new PlayerStatistics(ship);

                    foreach (var func in GeneralEndCheckingHandler)
                        if (func(ship)) 
                            return false;

                    foreach (var func in StatisticalEndCheckingHandler)
                        if (func(ship, statistics))
                            return false;

                    return false;
                }

                private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
                {
                    if (Mini.triggerMiniLose)
                    {
                        UncheckedEndGame(CustomGameOverReason.MiniLose);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
                {
                    if (Jester.triggerJesterWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.JesterWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
                {
                    if (Arsonist.triggerArsonistWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.ArsonistWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForVultureWin(ShipStatus __instance)
                {
                    if (Vulture.triggerVultureWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.VultureWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForLawyerMeetingWin(ShipStatus __instance)
                {
                    if (Lawyer.triggerLawyerWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.LawyerSoloWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForPlagueDoctorWin(ShipStatus __instance)
                {
                    if (PlagueDoctor.triggerPlagueDoctorWin)
                    {
                        UncheckedEndGame(CustomGameOverReason.PlagueDoctorWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
                {
                    if (__instance.Systems == null) return false;
                    ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
                    if (systemType != null)
                    {
                        LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                        if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                        {
                            EndGameForSabotage(__instance);
                            lifeSuppSystemType.Countdown = 10000f;
                            return true;
                        }
                    }
                    ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
                    if (systemType2 == null)
                    {
                        systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
                    }
                    if (systemType2 != null)
                    {
                        ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                        if (criticalSystem != null && criticalSystem.Countdown < 0f)
                        {
                            EndGameForSabotage(__instance);
                            criticalSystem.ClearSabotage();
                            return true;
                        }
                    }
                    return false;
                }

                private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
                {
                    if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
                    {
                        UncheckedEndGame(GameOverReason.CrewmatesByTask);
                        return true;
                    }

                    if (Fox.Exists && !Fox.crewWinsByTasks)
                    {
                        // 狐生存かつタスク完了時に生存中のクルーがタスクを全て終わらせたら勝ち
                        // 死んだプレイヤーが意図的にタスクを終了させないのを防止するため
                        bool isFoxAlive = Fox.isFoxAlive();
                        bool isFoxCompletedtasks = Fox.isFoxCompletedTasks();
                        int numDeadPlayerUncompletedTasks = 0;
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            foreach (var task in player.Data.Tasks)
                            {
                                if (player.Data.IsDead && player.IsCrewmate())
                                {
                                    if (!task.Complete)
                                    {
                                        numDeadPlayerUncompletedTasks++;
                                    }
                                }
                            }
                        }

                        if (isFoxCompletedtasks && isFoxAlive && GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks + numDeadPlayerUncompletedTasks)
                        {
                            UncheckedEndGame(GameOverReason.CrewmatesByTask);
                            return true;
                        }
                    }

                    return false;
                }

                private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.CouplesAlive == 1 && statistics.TotalAlive <= 3)
                    {
                        UncheckedEndGame(CustomGameOverReason.LoversWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive - statistics.FoxAlive &&
                        statistics.TeamImpostorsAlive == 0 && 
                        (statistics.TeamJackalLovers == 0 || statistics.TeamJackalLovers >= statistics.CouplesAlive * 2)
                       )
                    {
                        UncheckedEndGame(CustomGameOverReason.TeamJackalWin);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive - statistics.FoxAlive &&
                        statistics.TeamJackalAlive == 0 &&
                        (statistics.TeamImpostorLovers == 0 || statistics.TeamImpostorLovers >= statistics.CouplesAlive * 2)
                       )
                    {
                        GameOverReason endReason;
                        switch (GameData.LastDeathReason)
                        {
                            case DeathReason.Exile:
                                endReason = GameOverReason.ImpostorsByVote;
                                break;
                            case DeathReason.Kill:
                                endReason = GameOverReason.ImpostorsByKill;
                                break;
                            default:
                                endReason = GameOverReason.ImpostorsByVote;
                                break;
                        }
                        UncheckedEndGame(endReason);
                        return true;
                    }
                    return false;
                }

                private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
                {
                    if (statistics.TeamCrew > 0 && statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0)
                    {
                        UncheckedEndGame(GameOverReason.CrewmatesByVote);
                        return true;
                    }
                    return false;
                }

                private static void EndGameForSabotage(ShipStatus __instance)
                {
                    UncheckedEndGame(GameOverReason.ImpostorsBySabotage);
                    return;
                }

                private static void UncheckedEndGame(GameOverReason reason)
                {
                    if (Main.DebugMode.Value) return;
                    GameManager.Instance.RpcEndGame(reason, false);
                    /*MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedEndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)reason);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedEndGame((byte)reason);*/
                }

                private static void UncheckedEndGame(CustomGameOverReason reason)
                {
                    UncheckedEndGame((GameOverReason)reason);
                }
            }

            public class PlayerStatistics
            {
                public int TeamImpostorsAlive { get; set; }
                public int TeamJackalAlive { get; set; }
                public int TeamLoversAlive { get; set; }
                public int CouplesAlive { get; set; }
                public int TeamCrew { get; set; }
                public int NeutralAlive { get; set; }
                public int TotalAlive { get; set; }
                public int TeamImpostorLovers { get; set; }
                public int TeamJackalLovers { get; set; }
                public int FoxAlive { get; set; }

                public PlayerStatistics(ShipStatus __instance)
                {
                    GetPlayerCounts();
                }

                private bool IsLover(NetworkedPlayerInfo p)
                {
                    foreach (var couple in Lovers.Couples)
                    {
                        if (p.PlayerId == couple.Lover1.PlayerId || p.PlayerId == couple.Lover2.PlayerId) return true;
                    }
                    return false;
                }

                private void GetPlayerCounts()
                {
                    int numJackalAlive = 0;
                    int numImpostorsAlive = 0;
                    int numTotalAlive = 0;
                    int numNeutralAlive = 0;
                    int numCrew = 0;

                    int numLoversAlive = 0;
                    int numCouplesAlive = 0;
                    int impLovers = 0;
                    int jackalLovers = 0;

                    for (int i = 0; i < GameData.Instance.PlayerCount; i++)
                    {
                        NetworkedPlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                        if (!playerInfo.Disconnected)
                        {
                            if (playerInfo.Object.IsCrewmate()) numCrew++;
                            if (!playerInfo.IsDead && !playerInfo.Object.IsGM())
                            {
                                numTotalAlive++;

                                bool lover = IsLover(playerInfo);
                                if (lover) numLoversAlive++;

                                if (playerInfo.Role.IsImpostor)
                                {
                                    numImpostorsAlive++;
                                    if (lover) impLovers++;
                                }
                                if (Jackal.jackal != null && Jackal.jackal.PlayerId == playerInfo.PlayerId)
                                {
                                    numJackalAlive++;
                                    if (lover) jackalLovers++;
                                }
                                if (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == playerInfo.PlayerId)
                                {
                                    numJackalAlive++;
                                    if (lover) jackalLovers++;
                                }

                                if (playerInfo.Object.IsNeutral()) numNeutralAlive++;
                            }
                        }
                    }

                    foreach (var couple in Lovers.Couples)
                    {
                        if (couple.Alive) numCouplesAlive++;
                    }

                    // In the special case of Mafia being enabled, but only the janitor's left alive,
                    // count it as zero impostors alive bc they can't actually do anything.
                    if (Godfather.godfather?.IsDead() == true && Mafioso.mafioso?.IsDead() == true && Janitor.janitor?.IsDead() == false)
                    {
                        numImpostorsAlive = 0;
                    }

                    TeamCrew = numCrew;
                    TeamJackalAlive = numJackalAlive;
                    TeamImpostorsAlive = numImpostorsAlive;
                    TeamLoversAlive = numLoversAlive;
                    NeutralAlive = numNeutralAlive;
                    TotalAlive = numTotalAlive;
                    CouplesAlive = numCouplesAlive;
                    TeamImpostorLovers = impLovers;
                    TeamJackalLovers = jackalLovers;
                    FoxAlive = Fox.LivingPlayers.Count;
                }
            }
        }
    }
}