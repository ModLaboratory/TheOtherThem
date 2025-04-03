using System;
using System.Collections.Generic;
using TheOtherThem.Patches;
using UnityEngine;

namespace TheOtherThem
{
    public class DeadPlayer
    {
        public PlayerControl player;
        public NetworkedPlayerInfo DeadInfo { get; }
        public DateTime timeOfDeath;
        public DeathReason deathReason;
        public PlayerControl killerIfExisting;
        public NetworkedPlayerInfo KillerInfo { get; }

        public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
        {
            this.player = player;
            DeadInfo = player.Data;
            this.timeOfDeath = timeOfDeath;
            this.deathReason = deathReason;
            this.killerIfExisting = killerIfExisting;
            KillerInfo = killerIfExisting.Data;
        }
    }

    static class GameHistory
    {
        public static List<Tuple<Vector3, bool>> localPlayerPositions = new List<Tuple<Vector3, bool>>();
        public static List<DeadPlayer> DeadPlayers = new List<DeadPlayer>();
        public static Dictionary<int, FinalStatus> FinalStatuses = new Dictionary<int, FinalStatus>();

        public static void clearGameHistory()
        {
            localPlayerPositions = new List<Tuple<Vector3, bool>>();
            DeadPlayers = new List<DeadPlayer>();
            FinalStatuses = new Dictionary<int, FinalStatus>();
        }
    }
}