using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Mappen.Utils
{
    public static class ConfigMigrator
    {
        // Reference to centralized version info
        public static int latestVersion => Config.VersionInfo.CurrentConfigVersion;
        private static readonly string ConfigBackupDir = Path.Combine(Application.RootDirectory, "configs/plugins/Mappen/backups");
        private static Mappen Instance => Mappen.Instance;

        static ConfigMigrator()
        {
            EnsureDirectoryExists();
        }

        private static void EnsureDirectoryExists()
        {
            if (!Directory.Exists(ConfigBackupDir))
            {
                Directory.CreateDirectory(ConfigBackupDir);
            }
        }

        /// <summary>
        /// Check config version and migrate if necessary
        /// </summary>
        /// <param name="config">Current config</param>
        /// <returns>Migrated config</returns>
        public static Config.Config MigrateConfig(Config.Config config)
        {
            if (config == null)
            {
                Instance?.Logger.LogError("Config is null, cannot migrate");
                return new Config.Config();
            }

            Instance?.Logger.LogInformation("Parsed Config version: {Version}", config.Version);

            // Do nothing if config version is already latest
            if (config.Version == latestVersion)
            {
                return config;
            }

            // Create backup
            BackupConfig(config);

            // Migrate through versions sequentially
            if (config.Version < latestVersion)
            {
                config = MigrateToLatest(config);
                
                // 移行が必要な場合のみ、最新バージョンに設定する
                config.Version = latestVersion;
                Instance?.Logger.LogInformation("Config migrated to version {Version}", latestVersion);
            }
            else
            {
                // 移行が不要な場合は、元のバージョンをそのまま保持
                Instance?.Logger.LogInformation("Config is already at latest version {Version}", config.Version);
            }
            
            // Save migrated config
            SaveConfig(config);
            
            return config;
        }

        /// <summary>
        /// Create a backup of the config file
        /// </summary>
        /// <param name="config">Config to backup</param>
        private static void BackupConfig(Config.Config config)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string version = config.Version.ToString();
                string backupPath = Path.Combine(ConfigBackupDir, $"config_v{version}_{timestamp}.json");

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(backupPath, json);
                Instance?.Logger.LogInformation("Config backup created at {Path}", backupPath);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Failed to create config backup");
            }
        }

        /// <summary>
        /// Save the config file
        /// </summary>
        /// <param name="config">Config to save</param>
        private static void SaveConfig(Config.Config config)
        {
            try
            {
                // CounterStrikeSharp normally saves config files automatically,
                // but we explicitly save it to ensure the migrated config is saved
                string configPath = Path.Combine(
                    Application.RootDirectory,
                    "configs/plugins/Mappen",
                    "Mappen.json");

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(configPath, json);
                Instance?.Logger.LogInformation("Config saved to {Path}", configPath);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Failed to save config");
            }
        }

        /// <summary>
        /// Migrate from old version (unversioned) to latest version
        /// </summary>
        /// <param name="oldConfig">Old config</param>
        /// <returns>Migrated config</returns>
        private static Config.Config MigrateToLatest(Config.Config oldConfig)
        {
            // Create new config object
            Config.Config newConfig = new Config.Config();
            
            int originalVersion = oldConfig.Version;
            
            // Copy existing properties
            CopyExistingProperties(oldConfig, newConfig);
            
            newConfig.Version = originalVersion;

            // Default values for new features like RTV are already set in the new config

            Instance?.Logger.LogInformation("Migrated config from version {} to version {}", oldConfig.Version, latestVersion);
            return newConfig;
        }

        /// <summary>
        /// Copy existing properties to a new config object
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        private static void CopyExistingProperties(object source, object target)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            foreach (var sourceProperty in sourceType.GetProperties())
            {
                // Get JsonPropertyName attribute
                var jsonAttr = sourceProperty.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true)
                    .FirstOrDefault() as JsonPropertyNameAttribute;

                if (jsonAttr == null)
                {
                    continue;
                }

                string propertyName = jsonAttr.Name;

                // Check if target has property with same name
                var targetProperty = targetType.GetProperties()
                    .FirstOrDefault(p => 
                    {
                        var attr = p.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true)
                            .FirstOrDefault() as JsonPropertyNameAttribute;
                        return attr != null && attr.Name == propertyName;
                    });

                if (targetProperty != null && targetProperty.CanWrite)
                {
                    try
                    {
                        var value = sourceProperty.GetValue(source);
                        if (value != null)
                        {
                            // Check if property types are compatible
                            if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                            {
                                targetProperty.SetValue(target, value);
                            }
                            else
                            {
                                Instance?.Logger.LogWarning("Property type mismatch: {Property}", propertyName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Instance?.Logger.LogError(ex, "Error copying property {Property}", propertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Get config version from JSON object
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>Config version, or 0 if not found</returns>
        public static int GetConfigVersionFromJson(string jsonString)
        {
            try
            {
                var jsonNode = JsonNode.Parse(jsonString);
                // BasePluginConfigでは"ConfigVersion"というプロパティ名を使用
                var versionNode = jsonNode?["ConfigVersion"];
                
                if (versionNode != null)
                {
                    return versionNode.GetValue<int>();
                }
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Failed to parse config version from JSON");
            }
            
            return 0; // Treat as version 0 if version is not set
        }
    }
}