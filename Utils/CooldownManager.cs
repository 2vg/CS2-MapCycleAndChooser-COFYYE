using System.Text.Json;
using CounterStrikeSharp.API.Core;
using MapCycleAndChooser_COFYYE.Classes;
using Microsoft.Extensions.Logging;

namespace MapCycleAndChooser_COFYYE.Utils
{
    public class MapCooldownData
    {
        public string MapValue { get; set; } = string.Empty;
        public int CurrentCooldown { get; set; } = 0;
    }

    public static class CooldownManager
    {
        private static readonly string CooldownFilePath = Path.Combine(Application.RootDirectory, "configs/plugins/MapCycleAndChooser-COFYYE/cooldowns.json");
        private static readonly Dictionary<string, int> MapCooldowns = new();
        private static readonly object FileLock = new();
        private static MapCycleAndChooser Instance => MapCycleAndChooser.Instance;

        static CooldownManager()
        {
            EnsureDirectoryExists();
        }

        private static void EnsureDirectoryExists()
        {
            string directory = Path.GetDirectoryName(CooldownFilePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void LoadCooldowns()
        {
            lock (FileLock)
            {
                try
                {
                    if (!File.Exists(CooldownFilePath))
                    {
                        Instance?.Logger.LogInformation("Cooldown file not found. Creating a new one.");
                        SaveCooldowns();
                        return;
                    }

                    string json = File.ReadAllText(CooldownFilePath);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Instance?.Logger.LogInformation("Cooldown file is empty. Creating a new one.");
                        SaveCooldowns();
                        return;
                    }

                    List<MapCooldownData>? cooldownData = JsonSerializer.Deserialize<List<MapCooldownData>>(json);
                    if (cooldownData == null)
                    {
                        Instance?.Logger.LogWarning("Failed to deserialize cooldown data. Creating a new file.");
                        SaveCooldowns();
                        return;
                    }

                    MapCooldowns.Clear();
                    foreach (var data in cooldownData)
                    {
                        MapCooldowns[data.MapValue] = data.CurrentCooldown;
                    }

                    Instance?.Logger.LogInformation("Loaded cooldowns for {Count} maps.", MapCooldowns.Count);
                }
                catch (Exception ex)
                {
                    Instance?.Logger.LogError(ex, "Error loading cooldown data.");
                }
            }
        }

        public static void SaveCooldowns()
        {
            lock (FileLock)
            {
                try
                {
                    List<MapCooldownData> cooldownData = new();
    
                    // Get current cooldowns from in-memory cache
                    foreach (var mapCooldown in MapCooldowns)
                    {
                        cooldownData.Add(new MapCooldownData
                        {
                            MapValue = mapCooldown.Key,
                            CurrentCooldown = mapCooldown.Value
                        });
                    }
    
                    string json = JsonSerializer.Serialize(cooldownData, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
    
                    File.WriteAllText(CooldownFilePath, json);
                    Instance?.Logger.LogInformation("Saved cooldowns for {Count} maps.", cooldownData.Count);
                }
                catch (Exception ex)
                {
                    Instance?.Logger.LogError(ex, "Error saving cooldown data.");
                }
            }
        }

        // Get the current cooldown for a map
        public static int GetMapCooldown(string mapValue)
        {
            if (MapCooldowns.TryGetValue(mapValue, out int cooldown))
            {
                return cooldown;
            }
            return 0;
        }

        // Set the current cooldown for a map
        public static void SetMapCooldown(string mapValue, int cooldown)
        {
            MapCooldowns[mapValue] = cooldown;
        }

        // Decrease the cooldown for a map by 1
        public static void DecreaseMapCooldown(string mapValue)
        {
            if (MapCooldowns.TryGetValue(mapValue, out int cooldown) && cooldown > 0)
            {
                MapCooldowns[mapValue] = cooldown - 1;
            }
        }

        // Reset the cooldown for a map to its cooldown cycles value
        public static void ResetMapCooldown(Map map)
        {
            if (map == null || string.IsNullOrEmpty(map.MapValue))
            {
                Instance?.Logger.LogWarning("Attempted to reset cooldown for null or invalid map");
                return;
            }
            
            MapCooldowns[map.MapValue] = map.MapCooldownCycles;
        }

        // Update a map's cooldown and save to file
        public static void UpdateMapCooldown(string mapValue, int cooldown)
        {
            MapCooldowns[mapValue] = cooldown;
            SaveCooldowns();
        }
    }
}