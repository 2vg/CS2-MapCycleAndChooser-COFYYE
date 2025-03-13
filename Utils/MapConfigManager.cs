using System.Text.Json;
using CounterStrikeSharp.API.Core;
using MapCycleAndChooser_COFYYE.Classes;
using Microsoft.Extensions.Logging;

namespace MapCycleAndChooser_COFYYE.Utils
{
    public static class MapConfigManager
    {
        private static readonly string PluginConfigPath = Path.Combine(Application.RootDirectory, "configs/plugins/MapCycleAndChooser-COFYYE");
        private static readonly string MapsDirectoryPath = Path.Combine(PluginConfigPath, "maps");
        private static readonly string DefaultMapConfigPath = Path.Combine(PluginConfigPath, "default_map_config.json");
        private static readonly string WorkshopMappingPath = Path.Combine(PluginConfigPath, "workshop_map_mapping.json");
        private static readonly object FileLock = new();
        private static MapCycleAndChooser Instance => MapCycleAndChooser.Instance;
        private static Map? DefaultMapConfig = null;
        private static Dictionary<string, string> WorkshopIdToMapName = new Dictionary<string, string>();

        static MapConfigManager()
        {
            EnsureDirectoryExists();
            LoadDefaultMapConfig();
            LoadWorkshopMapping();
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(PluginConfigPath))
            {
                Directory.CreateDirectory(PluginConfigPath);
            }
            
            if (!Directory.Exists(MapsDirectoryPath))
            {
                Directory.CreateDirectory(MapsDirectoryPath);
            }
        }
        
        private static void LoadWorkshopMapping()
        {
            try
            {
                if (File.Exists(WorkshopMappingPath))
                {
                    string json = File.ReadAllText(WorkshopMappingPath);
                    WorkshopIdToMapName = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                    Instance?.Logger.LogInformation("Loaded workshop mapping from file. {Count} mappings found.", WorkshopIdToMapName.Count);
                }
                else
                {
                    WorkshopIdToMapName = new Dictionary<string, string>();
                    SaveWorkshopMapping();
                    Instance?.Logger.LogInformation("Created new workshop mapping file.");
                }
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error loading workshop mapping.");
                WorkshopIdToMapName = new Dictionary<string, string>();
            }
        }
        
        private static void SaveWorkshopMapping()
        {
            try
            {
                string json = JsonSerializer.Serialize(WorkshopIdToMapName, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(WorkshopMappingPath, json);
                Instance?.Logger.LogInformation("Saved workshop mapping to file.");
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error saving workshop mapping.");
            }
        }
        
        private static void LoadDefaultMapConfig()
        {
            try
            {
                if (File.Exists(DefaultMapConfigPath))
                {
                    string json = File.ReadAllText(DefaultMapConfigPath);
                    DefaultMapConfig = JsonSerializer.Deserialize<Map>(json);
                    Instance?.Logger.LogInformation("Loaded default map configuration from file.");
                }
                else
                {
                    // Create a default configuration file if it doesn't exist
                    SaveDefaultMapConfig();
                }
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error loading default map configuration.");
            }
        }
        
        private static void SaveDefaultMapConfig()
        {
            try
            {
                // Create a default map configuration
                Map defaultMap = new Map(
                    mapValue: "default",
                    mapDisplay: "Default Map",
                    mapIsWorkshop: false,
                    mapWorkshopId: "",
                    mapCycleEnabled: true,
                    mapCanVote: true,
                    mapMinPlayers: 0,
                    mapMaxPlayers: 64,
                    mapCycleStartTime: "",
                    mapCycleEndTime: "",
                    mapCooldownCycles: 10
                );
                
                DefaultMapConfig = defaultMap;
                
                string json = JsonSerializer.Serialize(defaultMap, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(DefaultMapConfigPath, json);
                Instance?.Logger.LogInformation("Created default map configuration file.");
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error saving default map configuration.");
            }
        }

        public static void LoadMapConfigs()
        {
            lock (FileLock)
            {
                try
                {
                    if (!Directory.Exists(MapsDirectoryPath))
                    {
                        Instance?.Logger.LogInformation("Maps directory not found. Creating a new one.");
                        Directory.CreateDirectory(MapsDirectoryPath);
                        return;
                    }

                    var mapFiles = Directory.GetFiles(MapsDirectoryPath, "*.json");
                    if (mapFiles.Length == 0)
                    {
                        Instance?.Logger.LogInformation("No map configuration files found. Using default maps from config.");
                        return;
                    }

                    List<Map> loadedMaps = new();

                    foreach (var mapFile in mapFiles)
                    {
                        try
                        {
                            string json = File.ReadAllText(mapFile);
                            if (string.IsNullOrWhiteSpace(json))
                            {
                                Instance?.Logger.LogWarning("Map config file is empty: {MapFile}", mapFile);
                                continue;
                            }

                            Map? mapConfig = JsonSerializer.Deserialize<Map>(json);
                            if (mapConfig == null)
                            {
                                Instance?.Logger.LogWarning("Failed to deserialize map config: {MapFile}", mapFile);
                                continue;
                            }

                            loadedMaps.Add(mapConfig);
                        }
                        catch (Exception ex)
                        {
                            Instance?.Logger.LogError(ex, "Error loading map config file: {MapFile}", mapFile);
                        }
                    }

                    if (loadedMaps.Count > 0)
                    {
                        Variables.GlobalVariables.Maps = loadedMaps;
                        Variables.GlobalVariables.CycleMaps = loadedMaps.Where(map => map.MapCycleEnabled).ToList();
                        Instance?.Logger.LogInformation("Loaded {Count} maps from individual config files.", loadedMaps.Count);
                    }
                }
                catch (Exception ex)
                {
                    Instance?.Logger.LogError(ex, "Error loading map configurations.");
                }
            }
        }

        public static void SaveMapConfig(Map map)
        {
            lock (FileLock)
            {
                try
                {
                    // Skip empty map names
                    if (string.IsNullOrWhiteSpace(map.MapValue) || map.MapValue == "<empty>" || map.MapValue == "\u003Cempty\u003E")
                    {
                        Instance?.Logger.LogWarning("Attempted to save map config for empty or <empty> map name. Skipping.");
                        return;
                    }
                    
                    string mapFilePath = Path.Combine(MapsDirectoryPath, $"{map.MapValue}.json");
                    
                    string json = JsonSerializer.Serialize(map, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    File.WriteAllText(mapFilePath, json);
                    Instance?.Logger.LogInformation("Saved map configuration for {MapName}.", map.MapValue);
                }
                catch (Exception ex)
                {
                    Instance?.Logger.LogError(ex, "Error saving map configuration for {MapName}.", map.MapValue);
                }
            }
        }

        public static void SaveAllMapConfigs()
        {
            foreach (var map in Variables.GlobalVariables.Maps)
            {
                SaveMapConfig(map);
            }
            Instance?.Logger.LogInformation("Saved all map configurations to individual files.");
        }

        public static void MigrateFromGlobalConfig()
        {
            if (Variables.GlobalVariables.Maps.Count > 0)
            {
                SaveAllMapConfigs();
                Instance?.Logger.LogInformation("Migrated maps from global config to individual map config files.");
            }
        }

        public static Map? GetMapConfig(string mapName)
        {
            // Skip empty map names
            if (string.IsNullOrWhiteSpace(mapName) || mapName == "<empty>" || mapName == "\u003Cempty\u003E")
            {
                Instance?.Logger.LogWarning("Attempted to get map config for empty or <empty> map name. Skipping.");
                return null;
            }
            
            string mapFilePath = Path.Combine(MapsDirectoryPath, $"{mapName}.json");
            
            if (!File.Exists(mapFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(mapFilePath);
                return JsonSerializer.Deserialize<Map>(json);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error reading map configuration for {MapName}.", mapName);
                return null;
            }
        }

        public static Map? CreateDefaultMapConfig(string mapName, bool isWorkshop = false, string workshopId = "")
        {
            // Skip empty map names
            if (string.IsNullOrWhiteSpace(mapName) || mapName == "<empty>" || mapName == "\u003Cempty\u003E")
            {
                Instance?.Logger.LogWarning("Attempted to create map config for empty or <empty> map name. Skipping.");
                return null;
            }
            
            // Use the default map configuration as a template if available
            Map defaultMap;
            
            if (DefaultMapConfig != null)
            {
                // Create a new map based on the default configuration
                defaultMap = new Map(
                    mapValue: mapName,
                    mapDisplay: mapName,  // Always use the actual map name for display
                    mapIsWorkshop: isWorkshop, // Use the provided isWorkshop flag
                    mapWorkshopId: isWorkshop ? workshopId : "", // Use the provided workshopId if it's a workshop map
                    mapCycleEnabled: DefaultMapConfig.MapCycleEnabled,
                    mapCanVote: DefaultMapConfig.MapCanVote,
                    mapMinPlayers: DefaultMapConfig.MapMinPlayers,
                    mapMaxPlayers: DefaultMapConfig.MapMaxPlayers,
                    mapCycleStartTime: DefaultMapConfig.MapCycleStartTime,
                    mapCycleEndTime: DefaultMapConfig.MapCycleEndTime,
                    mapCooldownCycles: DefaultMapConfig.MapCooldownCycles
                );
            }
            else
            {
                // Fallback to hardcoded defaults if no default configuration is available
                defaultMap = new Map(
                    mapValue: mapName,
                    mapDisplay: mapName,
                    mapIsWorkshop: isWorkshop, // Use the provided isWorkshop flag
                    mapWorkshopId: isWorkshop ? workshopId : "", // Use the provided workshopId if it's a workshop map
                    mapCycleEnabled: true,
                    mapCanVote: true,
                    mapMinPlayers: 0,
                    mapMaxPlayers: 64,
                    mapCycleStartTime: "",
                    mapCycleEndTime: "",
                    mapCooldownCycles: 2
                );
            }

            // Save the configuration for this specific map
            SaveMapConfig(defaultMap);
            
            if (isWorkshop)
            {
                Instance?.Logger.LogInformation("Created configuration for workshop map: {MapName} with ID {WorkshopId} based on default template", mapName, workshopId);
            }
            else
            {
                Instance?.Logger.LogInformation("Created configuration for map: {MapName} based on default template", mapName);
            }

            return defaultMap;
        }

        public static Map? GetOrCreateMapConfig(string mapName, bool isWorkshop = false, string workshopId = "")
        {
            // Skip empty map names
            if (string.IsNullOrWhiteSpace(mapName) || mapName == "<empty>" || mapName == "\u003Cempty\u003E")
            {
                Instance?.Logger.LogWarning("Attempted to get or create map config for empty or <empty> map name. Skipping.");
                return null;
            }
            
            Map? mapConfig = GetMapConfig(mapName);
            
            if (mapConfig == null)
            {
                mapConfig = CreateDefaultMapConfig(mapName, isWorkshop, workshopId);
            }
            else if (isWorkshop && !mapConfig.MapIsWorkshop)
            {
                // If the map exists but is not marked as a workshop map and we have a workshop ID,
                // update the map to mark it as a workshop map
                Map updatedMap = new Map(
                    mapValue: mapConfig.MapValue,
                    mapDisplay: mapConfig.MapDisplay,
                    mapIsWorkshop: true,
                    mapWorkshopId: workshopId,
                    mapCycleEnabled: mapConfig.MapCycleEnabled,
                    mapCanVote: mapConfig.MapCanVote,
                    mapMinPlayers: mapConfig.MapMinPlayers,
                    mapMaxPlayers: mapConfig.MapMaxPlayers,
                    mapCycleStartTime: mapConfig.MapCycleStartTime,
                    mapCycleEndTime: mapConfig.MapCycleEndTime,
                    mapCooldownCycles: mapConfig.MapCooldownCycles
                );
                
                // Save the updated config
                SaveMapConfig(updatedMap);
                Instance?.Logger.LogInformation("Updated existing map to mark as workshop map: {MapName} with ID {WorkshopId}", mapName, workshopId);
                
                mapConfig = updatedMap;
            }

            return mapConfig;
        }
        
        public static string? GetOfficialMapName(string workshopId)
        {
            if (WorkshopIdToMapName.TryGetValue(workshopId, out string? mapName))
            {
                return mapName;
            }
            return null;
        }
        
        public static void UpdateWorkshopMapping(string workshopId, string mapName)
        {
            if (string.IsNullOrWhiteSpace(workshopId) || string.IsNullOrWhiteSpace(mapName))
            {
                Instance?.Logger.LogWarning("Attempted to update workshop mapping with empty workshopId or mapName. Skipping.");
                return;
            }
            
            WorkshopIdToMapName[workshopId] = mapName;
            SaveWorkshopMapping();
            Instance?.Logger.LogInformation("Updated workshop mapping: {WorkshopId} -> {MapName}", workshopId, mapName);
        }
        
        public static void MergeWorkshopConfigs(string workshopId, string officialMapName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workshopId) || string.IsNullOrWhiteSpace(officialMapName))
                {
                    Instance?.Logger.LogWarning("Attempted to merge workshop configs with empty workshopId or officialMapName. Skipping.");
                    return;
                }
                
                // Check if this is the first time we've seen this workshop ID with an official map name
                bool isFirstMapping = false;
                if (!WorkshopIdToMapName.ContainsKey(workshopId))
                {
                    isFirstMapping = true;
                    Instance?.Logger.LogInformation("First time mapping workshop ID {WorkshopId} to official map name {OfficialMapName}",
                        workshopId, officialMapName);
                }
                else if (WorkshopIdToMapName[workshopId] != officialMapName)
                {
                    Instance?.Logger.LogInformation("Updating workshop mapping from {OldMapName} to {NewMapName} for ID {WorkshopId}",
                        WorkshopIdToMapName[workshopId], officialMapName, workshopId);
                }
                
                // Find all maps with the same workshop ID but different names
                var maps = Variables.GlobalVariables.Maps
                    .Where(m => m.MapIsWorkshop && m.MapWorkshopId == workshopId && m.MapValue != officialMapName)
                    .ToList();
                
                if (maps.Count == 0)
                {
                    // No duplicate configs found
                    if (isFirstMapping)
                    {
                        Instance?.Logger.LogInformation("No duplicate configs found for new workshop mapping {WorkshopId} -> {OfficialMapName}",
                            workshopId, officialMapName);
                    }
                    return;
                }
                
                Instance?.Logger.LogInformation("Found {Count} duplicate workshop map configs for ID {WorkshopId}", maps.Count, workshopId);
                
                // Check if the official map config already exists
                var officialConfig = GetMapConfig(officialMapName);
                
                // Find the most recently modified config file to use as the source for settings
                Map mostRecentConfig = null;
                DateTime mostRecentTime = DateTime.MinValue;
                
                foreach (var map in maps)
                {
                    string mapFilePath = Path.Combine(MapsDirectoryPath, $"{map.MapValue}.json");
                    if (File.Exists(mapFilePath))
                    {
                        DateTime lastModified = File.GetLastWriteTime(mapFilePath);
                        if (lastModified > mostRecentTime)
                        {
                            mostRecentTime = lastModified;
                            mostRecentConfig = map;
                        }
                    }
                }
                
                // If we couldn't determine the most recent config, use the first one
                if (mostRecentConfig == null && maps.Count > 0)
                {
                    mostRecentConfig = maps.First();
                    Instance?.Logger.LogInformation("Could not determine most recent config, using first available: {MapName}", mostRecentConfig.MapValue);
                }
                else if (mostRecentConfig != null)
                {
                    Instance?.Logger.LogInformation("Using most recent config as source: {MapName} (Last modified: {LastModified})",
                        mostRecentConfig.MapValue, mostRecentTime);
                }
                
                // If we have a config to use as source
                if (mostRecentConfig != null)
                {
                    if (officialConfig == null)
                    {
                        // Create a new config with the official map name using the most recent duplicate as a template
                        Map newMap = new Map(
                            mapValue: officialMapName,
                            mapDisplay: mostRecentConfig.MapDisplay,
                            mapIsWorkshop: true,
                            mapWorkshopId: workshopId,
                            mapCycleEnabled: mostRecentConfig.MapCycleEnabled,
                            mapCanVote: mostRecentConfig.MapCanVote,
                            mapMinPlayers: mostRecentConfig.MapMinPlayers,
                            mapMaxPlayers: mostRecentConfig.MapMaxPlayers,
                            mapCycleStartTime: mostRecentConfig.MapCycleStartTime,
                            mapCycleEndTime: mostRecentConfig.MapCycleEndTime,
                            mapCooldownCycles: mostRecentConfig.MapCooldownCycles
                        );
                        
                        // Save the new config
                        SaveMapConfig(newMap);
                        
                        // Add to global maps list
                        Variables.GlobalVariables.Maps.Add(newMap);
                        if (newMap.MapCycleEnabled)
                        {
                            Variables.GlobalVariables.CycleMaps.Add(newMap);
                        }
                        
                        Instance?.Logger.LogInformation("Created new map config with official name: {OfficialMapName}", officialMapName);
                    }
                    else
                    {
                        // Check if the official config is older than the most recent duplicate
                        string officialFilePath = Path.Combine(MapsDirectoryPath, $"{officialMapName}.json");
                        bool shouldUpdate = true;
                        string updateReason = "Default decision";
                        
                        if (File.Exists(officialFilePath))
                        {
                            DateTime officialLastModified = File.GetLastWriteTime(officialFilePath);
                            
                            // If this is the first time we're mapping this workshop ID
                            if (isFirstMapping)
                            {
                                // The user has been using a non-official name, so we should prefer their settings
                                shouldUpdate = true;
                                updateReason = "First mapping of workshop ID, preferring user settings from non-official name";
                            }
                            // If the official config is newer than the duplicate
                            else if (officialLastModified > mostRecentTime)
                            {
                                // Official config is newer, keep it as is
                                shouldUpdate = false;
                                updateReason = $"Official config is more recent ({officialLastModified}) than duplicate ({mostRecentTime})";
                            }
                            // If the duplicate is newer than the official config
                            else
                            {
                                // Duplicate is newer, update the official config
                                shouldUpdate = true;
                                updateReason = $"Duplicate config is more recent ({mostRecentTime}) than official ({officialLastModified})";
                            }
                        }
                        else
                        {
                            updateReason = "Official config file doesn't exist on disk";
                        }
                        
                        Instance?.Logger.LogInformation("Decision for {OfficialMapName}: {ShouldUpdate} - {Reason}",
                            officialMapName, shouldUpdate ? "Update" : "Keep", updateReason);
                        
                        if (shouldUpdate)
                        {
                            // Update the existing official config with settings from the most recent duplicate
                            Map updatedMap = new Map(
                                mapValue: officialMapName,
                                mapDisplay: officialConfig.MapDisplay,
                                mapIsWorkshop: true, // Ensure this is set to true for workshop maps
                                mapWorkshopId: workshopId,
                                mapCycleEnabled: mostRecentConfig.MapCycleEnabled,
                                mapCanVote: mostRecentConfig.MapCanVote,
                                mapMinPlayers: mostRecentConfig.MapMinPlayers,
                                mapMaxPlayers: mostRecentConfig.MapMaxPlayers,
                                mapCycleStartTime: mostRecentConfig.MapCycleStartTime,
                                mapCycleEndTime: mostRecentConfig.MapCycleEndTime,
                                mapCooldownCycles: mostRecentConfig.MapCooldownCycles
                            );
                            
                            // Save the updated config
                            SaveMapConfig(updatedMap);
                            
                            // Update in global maps list
                            int index = Variables.GlobalVariables.Maps.FindIndex(m => m.MapValue == officialMapName);
                            if (index >= 0)
                            {
                                Variables.GlobalVariables.Maps[index] = updatedMap;
                            }
                            
                            Instance?.Logger.LogInformation("Updated existing map config with official name: {OfficialMapName} using settings from {SourceMap}",
                                officialMapName, mostRecentConfig.MapValue);
                        }
                    }
                }
                
                // Delete the duplicate config files
                foreach (var map in maps)
                {
                    string mapFilePath = Path.Combine(MapsDirectoryPath, $"{map.MapValue}.json");
                    if (File.Exists(mapFilePath))
                    {
                        // Create a backup before deleting
                        string backupPath = Path.Combine(MapsDirectoryPath, "backups");
                        if (!Directory.Exists(backupPath))
                        {
                            Directory.CreateDirectory(backupPath);
                        }
                        
                        string backupFilePath = Path.Combine(backupPath, $"{map.MapValue}_{DateTime.Now:yyyyMMdd_HHmmss}.json.bak");
                        try
                        {
                            File.Copy(mapFilePath, backupFilePath);
                            Instance?.Logger.LogInformation("Created backup of duplicate config: {BackupFile}", backupFilePath);
                        }
                        catch (Exception ex)
                        {
                            Instance?.Logger.LogWarning(ex, "Failed to create backup of duplicate config: {MapFile}", mapFilePath);
                        }
                        
                        // Now delete the original
                        File.Delete(mapFilePath);
                        Instance?.Logger.LogInformation("Deleted duplicate map config file: {MapFile}", mapFilePath);
                    }
                    
                    // Remove from global maps list
                    Variables.GlobalVariables.Maps.Remove(map);
                    Variables.GlobalVariables.CycleMaps.Remove(map);
                }
                
                // Refresh the cycle maps list
                Variables.GlobalVariables.CycleMaps = Variables.GlobalVariables.Maps.Where(map => map.MapCycleEnabled).ToList();
                
                Instance?.Logger.LogInformation("Successfully merged workshop map configs for ID {WorkshopId}", workshopId);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error merging workshop configs for ID {WorkshopId}", workshopId);
            }
        }
    }
}