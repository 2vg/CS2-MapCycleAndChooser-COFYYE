using System.Text.Json;
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
        private static readonly string CooldownFilePath = Path.Combine("addons", "configs", "plugins", "MapCycleAndChooser-COFYYE", "cooldowns.json");
        private static readonly Dictionary<string, int> MapCooldowns = new();
        private static readonly SemaphoreSlim FileLock = new(1, 1);
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

        public static async Task LoadCooldownsAsync()
        {
            await FileLock.WaitAsync();
            try
            {
                if (!File.Exists(CooldownFilePath))
                {
                    Instance?.Logger.LogInformation("Cooldown file not found. Creating a new one.");
                    await SaveCooldownsAsync();
                    return;
                }

                string json = await File.ReadAllTextAsync(CooldownFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Instance?.Logger.LogInformation("Cooldown file is empty. Creating a new one.");
                    await SaveCooldownsAsync();
                    return;
                }

                List<MapCooldownData>? cooldownData = JsonSerializer.Deserialize<List<MapCooldownData>>(json);
                if (cooldownData == null)
                {
                    Instance?.Logger.LogWarning("Failed to deserialize cooldown data. Creating a new file.");
                    await SaveCooldownsAsync();
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
            finally
            {
                FileLock.Release();
            }
        }

        public static async Task SaveCooldownsAsync()
        {
            await FileLock.WaitAsync();
            try
            {
                List<MapCooldownData> cooldownData = new();

                // Get current cooldowns from maps
                foreach (var map in Variables.GlobalVariables.Maps)
                {
                    cooldownData.Add(new MapCooldownData
                    {
                        MapValue = map.MapValue,
                        CurrentCooldown = map.MapCurrentCooldown
                    });

                    // Update in-memory cache
                    MapCooldowns[map.MapValue] = map.MapCurrentCooldown;
                }

                string json = JsonSerializer.Serialize(cooldownData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(CooldownFilePath, json);
                Instance?.Logger.LogInformation("Saved cooldowns for {Count} maps.", cooldownData.Count);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error saving cooldown data.");
            }
            finally
            {
                FileLock.Release();
            }
        }

        public static void ApplyCooldownsToMaps()
        {
            foreach (var map in Variables.GlobalVariables.Maps)
            {
                if (MapCooldowns.TryGetValue(map.MapValue, out int cooldown))
                {
                    map.MapCurrentCooldown = cooldown;
                }
            }
        }

        public static async Task UpdateMapCooldownAsync(Map map)
        {
            MapCooldowns[map.MapValue] = map.MapCurrentCooldown;
            await SaveCooldownsAsync();
        }
    }
}