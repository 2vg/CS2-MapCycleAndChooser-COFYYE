using System;

namespace Mappen.Config
{
    /// <summary>
    /// Centralized version information for the plugin
    /// </summary>
    public static class VersionInfo
    {
        /// <summary>
        /// Current version of the config file format
        /// </summary>
        public const int CurrentConfigVersion = 3;

        /// <summary>
        /// Get the appropriate config version for a given plugin version
        /// </summary>
        /// <param name="pluginVersion">Plugin version string</param>
        /// <returns>Config version number</returns>
        public static int GetConfigVersionForPluginVersion(string pluginVersion)
        {
            // This method can be expanded in the future to map specific plugin versions
            // to specific config versions if needed
            
            // For now, we just return the current config version
            return CurrentConfigVersion;
        }
    }
}