﻿sing CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Mappen.Variables;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Mappen.Utils
{
    public static class ServerUtils
    {
        public static Mappen Instance => Mappen.Instance;

        // Helper method to create a new config with updated VoteMapDuration
        private static Config.Config CreateConfigWithVoteMapDuration(Config.Config currentConfig, int newDuration)
        {
            return new Config.Config
            {
                // Copy all properties from the current config
                Version = currentConfig.Version,
                RtvEnable = currentConfig.RtvEnable,
                RtvDelay = currentConfig.RtvDelay,
                RtvPlayerPercentage = currentConfig.RtvPlayerPercentage,
                RtvChangeInstantly = currentConfig.RtvChangeInstantly,
                RtvRespectNextmap = currentConfig.RtvRespectNextmap,
                VoteMapEnable = currentConfig.VoteMapEnable,
                VoteMapDuration = newDuration, // Set the new value
                VoteMapOnFreezeTime = currentConfig.VoteMapOnFreezeTime,
                DependsOnTheRound = currentConfig.DependsOnTheRound,
                EnableRandomNextMap = currentConfig.EnableRandomNextMap,
                EnablePlayerFreezeInMenu = currentConfig.EnablePlayerFreezeInMenu,
                EnablePlayerVotingInChat = currentConfig.EnablePlayerVotingInChat,
                EnableNextMapCommand = currentConfig.EnableNextMapCommand,
                EnableLastMapCommand = currentConfig.EnableLastMapCommand,
                EnableCurrentMapCommand = currentConfig.EnableCurrentMapCommand,
                EnableTimeLeftCommand = currentConfig.EnableTimeLeftCommand,
                EnableCommandAdsInChat = currentConfig.EnableCommandAdsInChat,
                EnableIgnoreVote = currentConfig.EnableIgnoreVote,
                IgnoreVotePosition = currentConfig.IgnoreVotePosition,
                EnableExtendMap = currentConfig.EnableExtendMap,
                ExtendMapTime = currentConfig.ExtendMapTime,
                ExtendMapPosition = currentConfig.ExtendMapPosition,
                DelayToChangeMapInTheEnd = currentConfig.DelayToChangeMapInTheEnd,
                VoteTriggerTimeBeforeMapEnd = currentConfig.VoteTriggerTimeBeforeMapEnd,
                DisplayMapByValue = currentConfig.DisplayMapByValue,
                VoteMapCount = currentConfig.VoteMapCount,
                Sounds = currentConfig.Sounds,
                EnableMapCooldown = currentConfig.EnableMapCooldown,
                Maps = currentConfig.Maps
            };
        }

        // Helper method to create a new config with updated IgnoreVotePosition
        private static Config.Config CreateConfigWithIgnoreVotePosition(Config.Config currentConfig, string newPosition)
        {
            return new Config.Config
            {
                // Copy all properties from the current config
                Version = currentConfig.Version,
                RtvEnable = currentConfig.RtvEnable,
                RtvDelay = currentConfig.RtvDelay,
                RtvPlayerPercentage = currentConfig.RtvPlayerPercentage,
                RtvChangeInstantly = currentConfig.RtvChangeInstantly,
                RtvRespectNextmap = currentConfig.RtvRespectNextmap,
                VoteMapEnable = currentConfig.VoteMapEnable,
                VoteMapDuration = currentConfig.VoteMapDuration,
                VoteMapOnFreezeTime = currentConfig.VoteMapOnFreezeTime,
                DependsOnTheRound = currentConfig.DependsOnTheRound,
                EnableRandomNextMap = currentConfig.EnableRandomNextMap,
                EnablePlayerFreezeInMenu = currentConfig.EnablePlayerFreezeInMenu,
                EnablePlayerVotingInChat = currentConfig.EnablePlayerVotingInChat,
                EnableNextMapCommand = currentConfig.EnableNextMapCommand,
                EnableLastMapCommand = currentConfig.EnableLastMapCommand,
                EnableCurrentMapCommand = currentConfig.EnableCurrentMapCommand,
                EnableTimeLeftCommand = currentConfig.EnableTimeLeftCommand,
                EnableCommandAdsInChat = currentConfig.EnableCommandAdsInChat,
                EnableIgnoreVote = currentConfig.EnableIgnoreVote,
                IgnoreVotePosition = newPosition, // Set the new value
                EnableExtendMap = currentConfig.EnableExtendMap,
                ExtendMapTime = currentConfig.ExtendMapTime,
                ExtendMapPosition = currentConfig.ExtendMapPosition,
                DelayToChangeMapInTheEnd = currentConfig.DelayToChangeMapInTheEnd,
                VoteTriggerTimeBeforeMapEnd = currentConfig.VoteTriggerTimeBeforeMapEnd,
                DisplayMapByValue = currentConfig.DisplayMapByValue,
                VoteMapCount = currentConfig.VoteMapCount,
                Sounds = currentConfig.Sounds,
                EnableMapCooldown = currentConfig.EnableMapCooldown,
                Maps = currentConfig.Maps
            };
        }

        // Helper method to create a new config with updated ExtendMapTime
        private static Config.Config CreateConfigWithExtendMapTime(Config.Config currentConfig, int newTime)
        {
            return new Config.Config
            {
                // Copy all properties from the current config
                Version = currentConfig.Version,
                RtvEnable = currentConfig.RtvEnable,
                RtvDelay = currentConfig.RtvDelay,
                RtvPlayerPercentage = currentConfig.RtvPlayerPercentage,
                RtvChangeInstantly = currentConfig.RtvChangeInstantly,
                RtvRespectNextmap = currentConfig.RtvRespectNextmap,
                VoteMapEnable = currentConfig.VoteMapEnable,
                VoteMapDuration = currentConfig.VoteMapDuration,
                VoteMapOnFreezeTime = currentConfig.VoteMapOnFreezeTime,
                DependsOnTheRound = currentConfig.DependsOnTheRound,
                EnableRandomNextMap = currentConfig.EnableRandomNextMap,
                EnablePlayerFreezeInMenu = currentConfig.EnablePlayerFreezeInMenu,
                EnablePlayerVotingInChat = currentConfig.EnablePlayerVotingInChat,
                EnableNextMapCommand = currentConfig.EnableNextMapCommand,
                EnableLastMapCommand = currentConfig.EnableLastMapCommand,
                EnableCurrentMapCommand = currentConfig.EnableCurrentMapCommand,
                EnableTimeLeftCommand = currentConfig.EnableTimeLeftCommand,
                EnableCommandAdsInChat = currentConfig.EnableCommandAdsInChat,
                EnableIgnoreVote = currentConfig.EnableIgnoreVote,
                IgnoreVotePosition = currentConfig.IgnoreVotePosition,
                EnableExtendMap = currentConfig.EnableExtendMap,
                ExtendMapTime = newTime, // Set the new value
                ExtendMapPosition = currentConfig.ExtendMapPosition,
                DelayToChangeMapInTheEnd = currentConfig.DelayToChangeMapInTheEnd,
                VoteTriggerTimeBeforeMapEnd = currentConfig.VoteTriggerTimeBeforeMapEnd,
                DisplayMapByValue = currentConfig.DisplayMapByValue,
                VoteMapCount = currentConfig.VoteMapCount,
                Sounds = currentConfig.Sounds,
                EnableMapCooldown = currentConfig.EnableMapCooldown,
                Maps = currentConfig.Maps
            };
        }

        // Helper method to create a new config with updated ExtendMapPosition
        private static Config.Config CreateConfigWithExtendMapPosition(Config.Config currentConfig, string newPosition)
        {
            return new Config.Config
            {
                // Copy all properties from the current config
                Version = currentConfig.Version,
                RtvEnable = currentConfig.RtvEnable,
                RtvDelay = currentConfig.RtvDelay,
                RtvPlayerPercentage = currentConfig.RtvPlayerPercentage,
                RtvChangeInstantly = currentConfig.RtvChangeInstantly,
                RtvRespectNextmap = currentConfig.RtvRespectNextmap,
                VoteMapEnable = currentConfig.VoteMapEnable,
                VoteMapDuration = currentConfig.VoteMapDuration,
                VoteMapOnFreezeTime = currentConfig.VoteMapOnFreezeTime,
                DependsOnTheRound = currentConfig.DependsOnTheRound,
                EnableRandomNextMap = currentConfig.EnableRandomNextMap,
                EnablePlayerFreezeInMenu = currentConfig.EnablePlayerFreezeInMenu,
                EnablePlayerVotingInChat = currentConfig.EnablePlayerVotingInChat,
                EnableNextMapCommand = currentConfig.EnableNextMapCommand,
                EnableLastMapCommand = currentConfig.EnableLastMapCommand,
                EnableCurrentMapCommand = currentConfig.EnableCurrentMapCommand,
                EnableTimeLeftCommand = currentConfig.EnableTimeLeftCommand,
                EnableCommandAdsInChat = currentConfig.EnableCommandAdsInChat,
                EnableIgnoreVote = currentConfig.EnableIgnoreVote,
                IgnoreVotePosition = currentConfig.IgnoreVotePosition,
                EnableExtendMap = currentConfig.EnableExtendMap,
                ExtendMapTime = currentConfig.ExtendMapTime,
                ExtendMapPosition = newPosition, // Set the new value
                DelayToChangeMapInTheEnd = currentConfig.DelayToChangeMapInTheEnd,
                VoteTriggerTimeBeforeMapEnd = currentConfig.VoteTriggerTimeBeforeMapEnd,
                DisplayMapByValue = currentConfig.DisplayMapByValue,
                VoteMapCount = currentConfig.VoteMapCount,
                Sounds = currentConfig.Sounds,
                EnableMapCooldown = currentConfig.EnableMapCooldown,
                Maps = currentConfig.Maps
            };
        }

        // Helper method to create a new config with updated DelayToChangeMapInTheEnd
        private static Config.Config CreateConfigWithDelayToChangeMapInTheEnd(Config.Config currentConfig, int newDelay)
        {
            return new Config.Config
            {
                // Copy all properties from the current config
                Version = currentConfig.Version,
                RtvEnable = currentConfig.RtvEnable,
                RtvDelay = currentConfig.RtvDelay,
                RtvPlayerPercentage = currentConfig.RtvPlayerPercentage,
                RtvChangeInstantly = currentConfig.RtvChangeInstantly,
                RtvRespectNextmap = currentConfig.RtvRespectNextmap,
                VoteMapEnable = currentConfig.VoteMapEnable,
                VoteMapDuration = currentConfig.VoteMapDuration,
                VoteMapOnFreezeTime = currentConfig.VoteMapOnFreezeTime,
                DependsOnTheRound = currentConfig.DependsOnTheRound,
                EnableRandomNextMap = currentConfig.EnableRandomNextMap,
                EnablePlayerFreezeInMenu = currentConfig.EnablePlayerFreezeInMenu,
                EnablePlayerVotingInChat = currentConfig.EnablePlayerVotingInChat,
                EnableNextMapCommand = currentConfig.EnableNextMapCommand,
                EnableLastMapCommand = currentConfig.EnableLastMapCommand,
                EnableCurrentMapCommand = currentConfig.EnableCurrentMapCommand,
                EnableTimeLeftCommand = currentConfig.EnableTimeLeftCommand,
                EnableCommandAdsInChat = currentConfig.EnableCommandAdsInChat,
                EnableIgnoreVote = currentConfig.EnableIgnoreVote,
                IgnoreVotePosition = currentConfig.IgnoreVotePosition,
                EnableExtendMap = currentConfig.EnableExtendMap,
                ExtendMapTime = currentConfig.ExtendMapTime,
                ExtendMapPosition = currentConfig.ExtendMapPosition,
                DelayToChangeMapInTheEnd = newDelay, // Set the new value
                VoteTriggerTimeBeforeMapEnd = currentConfig.VoteTriggerTimeBeforeMapEnd,
                DisplayMapByValue = currentConfig.DisplayMapByValue,
                VoteMapCount = currentConfig.VoteMapCount,
                Sounds = currentConfig.Sounds,
                EnableMapCooldown = currentConfig.EnableMapCooldown,
                Maps = currentConfig.Maps
            };
        }

        // Helper method to create a new config with updated VoteTriggerTimeBeforeMapEnd
        private static Config.Config CreateConfigWithVoteTriggerTimeBeforeMapEnd(Config.Config currentConfig, int newTime)
        {
            return new Config.Config
            {
                // Copy all properties from the current config
                Version = currentConfig.Version,
                RtvEnable = currentConfig.RtvEnable,
                RtvDelay = currentConfig.RtvDelay,
                RtvPlayerPercentage = currentConfig.RtvPlayerPercentage,
                RtvChangeInstantly = currentConfig.RtvChangeInstantly,
                RtvRespectNextmap = currentConfig.RtvRespectNextmap,
                VoteMapEnable = currentConfig.VoteMapEnable,
                VoteMapDuration = currentConfig.VoteMapDuration,
                VoteMapOnFreezeTime = currentConfig.VoteMapOnFreezeTime,
                DependsOnTheRound = currentConfig.DependsOnTheRound,
                EnableRandomNextMap = currentConfig.EnableRandomNextMap,
                EnablePlayerFreezeInMenu = currentConfig.EnablePlayerFreezeInMenu,
                EnablePlayerVotingInChat = currentConfig.EnablePlayerVotingInChat,
                EnableNextMapCommand = currentConfig.EnableNextMapCommand,
                EnableLastMapCommand = currentConfig.EnableLastMapCommand,
                EnableCurrentMapCommand = currentConfig.EnableCurrentMapCommand,
                EnableTimeLeftCommand = currentConfig.EnableTimeLeftCommand,
                EnableCommandAdsInChat = currentConfig.EnableCommandAdsInChat,
                EnableIgnoreVote = currentConfig.EnableIgnoreVote,
                IgnoreVotePosition = currentConfig.IgnoreVotePosition,
                EnableExtendMap = currentConfig.EnableExtendMap,
                ExtendMapTime = currentConfig.ExtendMapTime,
                ExtendMapPosition = currentConfig.ExtendMapPosition,
                DelayToChangeMapInTheEnd = currentConfig.DelayToChangeMapInTheEnd,
                VoteTriggerTimeBeforeMapEnd = newTime, // Set the new value
                DisplayMapByValue = currentConfig.DisplayMapByValue,
                VoteMapCount = currentConfig.VoteMapCount,
                Sounds = currentConfig.Sounds,
                EnableMapCooldown = currentConfig.EnableMapCooldown,
                Maps = currentConfig.Maps
            };
        }

        public static void InitializeCvars()
        {
            GlobalVariables.FreezeTime = ConVar.Find("mp_freezetime")?.GetPrimitiveValue<int>() ?? 5;

            if (Instance?.Config?.DependsOnTheRound == true)
            {
                var maxRounds = ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>();

                if (maxRounds <= 4)
                {
                    Server.ExecuteCommand("mp_maxrounds 5");
                    Instance?.Logger.LogInformation("mp_maxrounds are set to a value less than 5. I set it to 5.");
                }

                Server.ExecuteCommand("mp_timelimit 0");
            }
            else
            {
                var timeLimit = ConVar.Find("mp_timelimit")?.GetPrimitiveValue<float>();

                if (timeLimit <= 4.0f)
                {
                    Server.ExecuteCommand("mp_timelimit 5");
                    Instance?.Logger.LogInformation("mp_timelimit are set to a value less than 5. I set it to 5.");
                    GlobalVariables.TimeLeft = 5 * 60; // in seconds
                }
                else
                {
                    GlobalVariables.TimeLeft = (timeLimit ?? 5.0f) * 60; // in seconds
                }

                Server.ExecuteCommand("mp_maxrounds 0");
            }
        }

        public static CCSGameRules? GetGameRules()
        {
            try
            {
                var gameRulesEntities = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
                
                if (!gameRulesEntities.Any())
                {
                    Instance?.Logger.LogWarning("No game rules entities found");
                    return null;
                }
                
                var gameRules = gameRulesEntities.First().GameRules;
        
                if (gameRules == null)
                {
                    Instance?.Logger.LogWarning("Game rules is null");
                    return null;
                }
        
                return gameRules;
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error getting game rules");
                return null;
            }
        }

        public static bool CheckAndValidateConfig()
        {
            bool isValid = true;

            if(Instance?.Config == null)
            {
                Instance?.Logger.LogError("Config fields are null.");
                return false;
            }

            // VoteMapDuration
            if (Instance.Config.VoteMapDuration < 0 || Instance.Config.VoteMapDuration > 45)
            {
                Instance.Logger.LogError("vote_map_duration has bad value. Value must be between 0 and 45. Setting to default value 15.");
                Instance.Config = CreateConfigWithVoteMapDuration(Instance.Config, 15);
                isValid = false;
            }

            // IgnoreVotePosition
            if (Instance.Config.IgnoreVotePosition != "top" && Instance.Config.IgnoreVotePosition != "bottom")
            {
                Instance.Logger.LogError("ignore_vote_position has bad value. Value must be top or bottom. Setting to default value 'top'.");
                Instance.Config = CreateConfigWithIgnoreVotePosition(Instance.Config, "top");
                isValid = false;
            }

            // ExtendMapTime
            if (Instance.Config.ExtendMapTime < 0)
            {
                Instance.Logger.LogError("extend_map_time has bad value. Value must be greater than 0. Setting to default value 8.");
                Instance.Config = CreateConfigWithExtendMapTime(Instance.Config, 8);
                isValid = false;
            }

            // ExtendMapPosition
            if (Instance.Config.ExtendMapPosition != "top" && Instance.Config.ExtendMapPosition != "bottom")
            {
                Instance.Logger.LogError("extend_map_position has bad value. Value must be top or bottom. Setting to default value 'bottom'.");
                Instance.Config = CreateConfigWithExtendMapPosition(Instance.Config, "bottom");
                isValid = false;
            }

            // DelayToChangeMapInTheEnd
            if (Instance.Config.DelayToChangeMapInTheEnd < 5)
            {
                Instance.Logger.LogError("delay_to_change_map_in_the_end has bad value. Value must be greater than or equal to 5. Setting to default value 10.");
                Instance.Config = CreateConfigWithDelayToChangeMapInTheEnd(Instance.Config, 10);
                isValid = false;
            }

            // VoteTriggerTimeBeforeMapEnd
            if (Instance.Config.VoteTriggerTimeBeforeMapEnd < 2)
            {
                Instance.Logger.LogError("vote_trigger_time_before_map_end has bad value. Value must be greater than or equal to 2. Setting to default value 3.");
                Instance.Config = CreateConfigWithVoteTriggerTimeBeforeMapEnd(Instance.Config, 3);
                isValid = false;
            }

            // Maps
            List<string> invalidMaps = new();
            foreach (var map in Instance.Config.Maps)
            {
                if (string.IsNullOrEmpty(map.MapCycleStartTime) || string.IsNullOrEmpty(map.MapCycleEndTime)) continue;

                bool mapValid = true;
                
                // CycleStartTime and CycleEndTime
                if (map.MapCycleStartTime == map.MapCycleEndTime)
                {
                    Instance.Logger.LogWarning("'map_cycle_start_time' and 'map_cycle_end_time' are same for map: {MapName}. This map will be available at all times.", map.MapValue);
                    mapValid = false;
                }
                
                if (!DateTime.TryParseExact(map.MapCycleStartTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
                {
                    Instance.Logger.LogWarning("'map_cycle_start_time' has bad value for map: {MapName}. Value format must be 'HH:mm'.", map.MapValue);
                    mapValid = false;
                }
                
                if (!DateTime.TryParseExact(map.MapCycleEndTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _))
                {
                    Instance.Logger.LogWarning("'map_cycle_end_time' has bad value for map: {MapName}. Value format must be 'HH:mm'.", map.MapValue);
                    mapValid = false;
                }

                if (!mapValid)
                {
                    invalidMaps.Add(map.MapValue);
                    isValid = false;
                }
            }

            return isValid;
        }
    }
}
