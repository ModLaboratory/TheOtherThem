using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace TheOtherThem{
    static class MapOptions {
        // Set values
        public static int MaxNumberOfMeetings { get; set; } = 10;
        public static bool BlockSkippingInEmergencyMeetings { get; set; } = false;
        public static bool NoVotingIsSelfVoting { get; set; } = false;
        public static bool HidePlayerNames { get; set; } = false;
        public static bool HideSettings { get; set; } = false;
        public static bool HideOutOfSightNametags { get; set; } = false;

        public static bool RandomizeColors { get; set; } = false;
        public static bool AllowDupeNames { get; set; } = false;

        public static int RestrictDevices { get; set; } = 0;
        public static float RestrictAdminTime { get; set; } = 600f;
        public static float RestrictAdminTimeMax { get; set; } = 600f;
        public static float RestrictCamerasTime { get; set; } = 600f;
        public static float RestrictCamerasTimeMax { get; set; } = 600f;
        public static float RestrictVitalsTime { get; set; } = 600f;
        public static float RestrictVitalsTimeMax { get; set; } = 600f;
        public static bool DisableVents { get; set; } = false;

        public static bool GhostsSeeRoles { get; set; } = true;
        public static bool GhostsSeeTasks { get; set; } = true;
        public static bool GhostsSeeVotes { get; set; } = true;
        public static bool ShowRoleSummary { get; set; } = true;
        public static bool HideNameplates { get; set; } = false;
        public static bool AllowParallelMedBayScans { get; set; } = false;
        public static bool ShowLighterOrDarker { get; set; } = false;
        public static bool HideTaskArrows { get; set; } = false;

        // Updating values
        public static int MeetingsCount { get; set; } = 0;
        public static List<SurvCamera> CamerasToAdd { get; set; } = new();
        public static List<Vent> VentsToSeal { get; set; } = new();
        public static Dictionary<byte, PoolablePlayer> PlayerIcons { get; set; } = new();

        public static void ClearAndReloadMapOptions() {
            MeetingsCount = 0;
            CamerasToAdd = new List<SurvCamera>();
            VentsToSeal = new List<Vent>();
            PlayerIcons = new Dictionary<byte, PoolablePlayer>();

            MaxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.GetSelection());
            BlockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.GetBool();
            NoVotingIsSelfVoting = CustomOptionHolder.noVoteIsSelfVote.GetBool();
            HidePlayerNames = CustomOptionHolder.hidePlayerNames.GetBool();

            HideOutOfSightNametags = CustomOptionHolder.hideOutOfSightNametags.GetBool();

            HideSettings = CustomOptionHolder.hideSettings.GetBool();

            RandomizeColors = CustomOptionHolder.UselessOptions.GetBool() && CustomOptionHolder.playerColorRandom.GetBool();
            AllowDupeNames = CustomOptionHolder.UselessOptions.GetBool() && CustomOptionHolder.playerNameDupes.GetBool();

            RestrictDevices = CustomOptionHolder.restrictDevices.GetSelection();
            RestrictAdminTime = RestrictAdminTimeMax = CustomOptionHolder.restrictAdmin.GetFloat();
            RestrictCamerasTime = RestrictCamerasTimeMax = CustomOptionHolder.restrictCameras.GetFloat();
            RestrictVitalsTime = RestrictVitalsTimeMax = CustomOptionHolder.restrictVents.GetFloat();
            DisableVents = CustomOptionHolder.disableVents.GetBool();

            AllowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.GetBool();
            GhostsSeeRoles = Main.GhostsSeeRoles.Value;
            GhostsSeeTasks = Main.GhostsSeeTasks.Value;
            GhostsSeeVotes = Main.GhostsSeeVotes.Value;
            ShowRoleSummary = Main.ShowRoleSummary.Value;
            HideNameplates = Main.HideNameplates.Value;
            ShowLighterOrDarker = Main.ShowLighterDarker.Value;
            HideTaskArrows = Main.HideTaskArrows.Value;
        }

        public static void resetDeviceTimes()
        {
            RestrictAdminTime = RestrictAdminTimeMax;
            RestrictCamerasTime = RestrictCamerasTimeMax;
            RestrictVitalsTime = RestrictVitalsTimeMax;
        }

        public static bool canUseAdmin
        {
            get
            {
                return RestrictDevices == 0 || RestrictAdminTime > 0f;
            }
        }

        public static bool couldUseAdmin
        {
            get
            {
                return RestrictDevices == 0 || RestrictAdminTimeMax > 0f;
            }
        }

        public static bool canUseCameras
        {
            get
            {
                return RestrictDevices == 0 || RestrictCamerasTime > 0f;
            }
        }

        public static bool couldUseCameras
        {
            get
            {
                return RestrictDevices == 0 || RestrictCamerasTimeMax > 0f;
            }
        }

        public static bool canUseVitals
        {
            get
            {
                return RestrictDevices == 0 || RestrictVitalsTime > 0f;
            }
        }

        public static bool couldUseVitals
        {
            get
            {
                return RestrictDevices == 0 || RestrictVitalsTimeMax > 0f;
            }
        }
    }
}