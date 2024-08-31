using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using static TheOtherThem.TheOtherRoles;

namespace TheOtherThem{
    static class MapOptions {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool hideSettings = false;
        public static bool hideOutOfSightNametags = false;

        public static bool randomizeColors = false;
        public static bool allowDupeNames = false;

        public static int restrictDevices = 0;
        public static float restrictAdminTime = 600f;
        public static float restrictAdminTimeMax = 600f;
        public static float restrictCamerasTime = 600f;
        public static float restrictCamerasTimeMax = 600f;
        public static float restrictVitalsTime = 600f;
        public static float restrictVitalsTimeMax = 600f;
        public static bool disableVents = false;

        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool hideNameplates = false;
        public static bool allowParallelMedBayScans = false;
        public static bool showLighterDarker = false;
        public static bool hideTaskArrows = false;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();

        public static void clearAndReloadMapOptions() {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.GetSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.GetBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.GetBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.GetBool();

            hideOutOfSightNametags = CustomOptionHolder.hideOutOfSightNametags.GetBool();

            hideSettings = CustomOptionHolder.hideSettings.GetBool();

            randomizeColors = CustomOptionHolder.uselessOptions.GetBool() && CustomOptionHolder.playerColorRandom.GetBool();
            allowDupeNames = CustomOptionHolder.uselessOptions.GetBool() && CustomOptionHolder.playerNameDupes.GetBool();

            restrictDevices = CustomOptionHolder.restrictDevices.GetSelection();
            restrictAdminTime = restrictAdminTimeMax = CustomOptionHolder.restrictAdmin.GetFloat();
            restrictCamerasTime = restrictCamerasTimeMax = CustomOptionHolder.restrictCameras.GetFloat();
            restrictVitalsTime = restrictVitalsTimeMax = CustomOptionHolder.restrictVents.GetFloat();
            disableVents = CustomOptionHolder.disableVents.GetBool();

            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.GetBool();
            ghostsSeeRoles = Main.GhostsSeeRoles.Value;
            ghostsSeeTasks = Main.GhostsSeeTasks.Value;
            ghostsSeeVotes = Main.GhostsSeeVotes.Value;
            showRoleSummary = Main.ShowRoleSummary.Value;
            hideNameplates = Main.HideNameplates.Value;
            showLighterDarker = Main.ShowLighterDarker.Value;
            hideTaskArrows = Main.HideTaskArrows.Value;
        }

        public static void resetDeviceTimes()
        {
            restrictAdminTime = restrictAdminTimeMax;
            restrictCamerasTime = restrictCamerasTimeMax;
            restrictVitalsTime = restrictVitalsTimeMax;
        }

        public static bool canUseAdmin
        {
            get
            {
                return restrictDevices == 0 || restrictAdminTime > 0f;
            }
        }

        public static bool couldUseAdmin
        {
            get
            {
                return restrictDevices == 0 || restrictAdminTimeMax > 0f;
            }
        }

        public static bool canUseCameras
        {
            get
            {
                return restrictDevices == 0 || restrictCamerasTime > 0f;
            }
        }

        public static bool couldUseCameras
        {
            get
            {
                return restrictDevices == 0 || restrictCamerasTimeMax > 0f;
            }
        }

        public static bool canUseVitals
        {
            get
            {
                return restrictDevices == 0 || restrictVitalsTime > 0f;
            }
        }

        public static bool couldUseVitals
        {
            get
            {
                return restrictDevices == 0 || restrictVitalsTimeMax > 0f;
            }
        }
    }
}