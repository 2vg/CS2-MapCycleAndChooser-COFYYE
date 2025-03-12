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
        private static readonly object FileLock = new();
        private static MapCycleAndChooser Instance => MapCycleAndChooser.Instance;
        private static Map? DefaultMapConfig = null;

        static MapConfigManager()
        {
            EnsureDirectoryExists();
            LoadDefaultMapConfig();
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

        public static Map? CreateDefaultMapConfig(string mapName)
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
                    mapIsWorkshop: DefaultMapConfig.MapIsWorkshop,
                    mapWorkshopId: DefaultMapConfig.MapWorkshopId,
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
                    mapIsWorkshop: false,
                    mapWorkshopId: "",
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
            Instance?.Logger.LogInformation("Created configuration for map: {MapName} based on default template", mapName);

            return defaultMap;
        }

        public static Map? GetOrCreateMapConfig(string mapName)
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
                mapConfig = CreateDefaultMapConfig(mapName);
            }

            return mapConfig;
        }
    }
}