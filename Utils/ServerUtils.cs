﻿﻿﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Mappen.Variables;
using Mappen.Classes;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;

namespace Mappen.Utils
{
    public static class ServerUtils
    {
        public static Mappen Instance => Mappen.Instance;

        // Generic helper method to create a new config with a single updated property
        private static Config.Config UpdateConfig<T>(Config.Config currentConfig, string propertyName, T newValue)
        {
            // Create a new config object
            var newConfig = new Config.Config();
            
            // Get all properties of the Config class
            var properties = typeof(Config.Config).GetProperties();
            
            // Copy all properties from the current config to the new one
            foreach (var prop in properties)
            {
                if (prop.CanWrite)
                {
                    if (prop.Name == propertyName)
                    {
                        // Set the new value for the specified property
                        prop.SetValue(newConfig, newValue);
                    }
                    else
                    {
                        // Copy the value from the current config
                        prop.SetValue(newConfig, prop.GetValue(currentConfig));
                    }
                }
            }
            
            return newConfig;
        }

        // Create a new config with multiple updated properties
        private static Config.Config UpdateConfigMultiple(Config.Config currentConfig, Dictionary<string, object> propertyUpdates)
        {
            // Create a new config object
            var newConfig = new Config.Config();
            
            // Get all properties of the Config class
            var properties = typeof(Config.Config).GetProperties();
            
            // Copy all properties from the current config to the new one
            foreach (var prop in properties)
            {
                if (prop.CanWrite)
                {
                    if (propertyUpdates.TryGetValue(prop.Name, out var newValue))
                    {
                        // Set the new value for the specified property
                        prop.SetValue(newConfig, newValue);
                    }
                    else
                    {
                        // Copy the value from the current config
                        prop.SetValue(newConfig, prop.GetValue(currentConfig));
                    }
                }
            }
            
            return newConfig;
        }

        public static void InitializeCvars()
        {
            GlobalVariables.FreezeTime = ConVar.Find("mp_freezetime")?.GetPrimitiveValue<int>() ?? 5;

            // Get current map config if available
            Map? currentMap = null;
            if (!string.IsNullOrWhiteSpace(Server.MapName) && Server.MapName != "<empty>" && Server.MapName != "\u003Cempty\u003E")
            {
                currentMap = GlobalVariables.Maps.FirstOrDefault(m => m.MapValue == Server.MapName);
            }

            if (currentMap != null && (currentMap.MapMaxRounds.HasValue || currentMap.MapTimeLimit.HasValue))
            {
                // Use map-specific settings if available
                if (currentMap.MapMaxRounds.HasValue && currentMap.MapTimeLimit.HasValue)
                {
                    // Both settings are available, use the one based on PrioritizeRounds
                    if (Instance?.Config?.PrioritizeRounds == true)
                    {
                        Server.ExecuteCommand($"mp_maxrounds {currentMap.MapMaxRounds.Value}");
                        Server.ExecuteCommand("mp_timelimit 0");
                        Instance?.Logger.LogInformation("Using map-specific maxrounds: {MaxRounds}", currentMap.MapMaxRounds.Value);
                    }
                    else
                    {
                        Server.ExecuteCommand($"mp_timelimit {currentMap.MapTimeLimit.Value}");
                        Server.ExecuteCommand("mp_maxrounds 0");
                        GlobalVariables.TimeLeft = currentMap.MapTimeLimit.Value * 60; // in seconds
                        Instance?.Logger.LogInformation("Using map-specific timelimit: {TimeLimit}", currentMap.MapTimeLimit.Value);
                    }
                }
                else if (currentMap.MapMaxRounds.HasValue)
                {
                    // Only maxrounds is set
                    Server.ExecuteCommand($"mp_maxrounds {currentMap.MapMaxRounds.Value}");
                    Server.ExecuteCommand("mp_timelimit 0");
                    Instance?.Logger.LogInformation("Using map-specific maxrounds: {MaxRounds}", currentMap.MapMaxRounds.Value);
                }
                else if (currentMap.MapTimeLimit.HasValue)
                {
                    // Only timelimit is set
                    Server.ExecuteCommand($"mp_timelimit {currentMap.MapTimeLimit.Value}");
                    Server.ExecuteCommand("mp_maxrounds 0");
                    GlobalVariables.TimeLeft = currentMap.MapTimeLimit.Value * 60; // in seconds
                    Instance?.Logger.LogInformation("Using map-specific timelimit: {TimeLimit}", currentMap.MapTimeLimit.Value);
                }
            }
            else
            {
                // Use global settings
                if (Instance?.Config?.PrioritizeRounds == true)
                {
                    var maxRounds = ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>();

                    if (maxRounds <= 4)
                    {
                        Server.ExecuteCommand("mp_maxrounds 5");
                        Instance?.Logger.LogInformation("mp_maxrounds are set to a value less than 5. I set it to 5.");
                    }

                    Server.ExecuteCommand("mp_timelimit 0");
                    Instance?.Logger.LogInformation("Using global maxrounds setting");
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
                    Instance?.Logger.LogInformation("Using global timelimit setting");
                }
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

            // Track properties that need to be updated
            Dictionary<string, object> propertyUpdates = new Dictionary<string, object>();

            // VoteMapDuration
            if (Instance.Config.VoteMapDuration < 0 || Instance.Config.VoteMapDuration > 45)
            {
                Instance.Logger.LogError("vote_map_duration has bad value. Value must be between 0 and 45. Setting to default value 15.");
                propertyUpdates["VoteMapDuration"] = 15;
                isValid = false;
            }

            // IgnoreVotePosition
            if (Instance.Config.IgnoreVotePosition != "top" && Instance.Config.IgnoreVotePosition != "bottom")
            {
                Instance.Logger.LogError("ignore_vote_position has bad value. Value must be top or bottom. Setting to default value 'top'.");
                propertyUpdates["IgnoreVotePosition"] = "top";
                isValid = false;
            }

            // ExtendMapTime
            if (Instance.Config.ExtendMapTime < 0)
            {
                Instance.Logger.LogError("extend_map_time has bad value. Value must be greater than 0. Setting to default value 8.");
                propertyUpdates["ExtendMapTime"] = 8;
                isValid = false;
            }

            // ExtendMapPosition
            if (Instance.Config.ExtendMapPosition != "top" && Instance.Config.ExtendMapPosition != "bottom")
            {
                Instance.Logger.LogError("extend_map_position has bad value. Value must be top or bottom. Setting to default value 'bottom'.");
                propertyUpdates["ExtendMapPosition"] = "bottom";
                isValid = false;
            }

            // DelayToChangeMapInTheEnd
            if (Instance.Config.DelayToChangeMapInTheEnd < 5)
            {
                Instance.Logger.LogError("delay_to_change_map_in_the_end has bad value. Value must be greater than or equal to 5. Setting to default value 10.");
                propertyUpdates["DelayToChangeMapInTheEnd"] = 10;
                isValid = false;
            }

            // VoteTriggerTimeBeforeMapEnd
            if (Instance.Config.VoteTriggerTimeBeforeMapEnd < 2)
            {
                Instance.Logger.LogError("vote_trigger_time_before_map_end has bad value. Value must be greater than or equal to 2. Setting to default value 3.");
                propertyUpdates["VoteTriggerTimeBeforeMapEnd"] = 3;
                isValid = false;
            }

            // Apply all updates at once if needed
            if (propertyUpdates.Count > 0)
            {
                Instance.Config = UpdateConfigMultiple(Instance.Config, propertyUpdates);
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
