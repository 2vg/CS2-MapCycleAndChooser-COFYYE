using System.Net.Http;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Mappen.Classes;
using Microsoft.Extensions.Logging;

namespace Mappen.Utils
{
    public static class WorkshopUtils
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static Mappen Instance => Mappen.Instance;

        /// <summary>
        /// Synchronizes maps from Steam Workshop collections
        /// </summary>
        public static void SyncWorkshopCollections()
        {
            if (Instance.Config.EnableWorkshopCollectionSync && Instance.Config.WorkshopCollectionIds.Count > 0)
            {
                Instance.Logger.LogInformation("Starting Workshop collection sync...");
                
                int totalNewMaps = 0;
                
                foreach (var collectionId in Instance.Config.WorkshopCollectionIds)
                {
                    try
                    {
                        int newMaps = SyncWorkshopCollection(collectionId);
                        totalNewMaps += newMaps;
                    }
                    catch (Exception ex)
                    {
                        Instance.Logger.LogError(ex, "Error syncing Workshop collection {CollectionId}", collectionId);
                    }
                }
                
                Instance.Logger.LogInformation("Workshop collection sync completed. Added {Count} new maps", totalNewMaps);
            }
        }

        /// <summary>
        /// Synchronizes maps from a specific Steam Workshop collection
        /// </summary>
        /// <param name="collectionId">The Steam Workshop collection ID</param>
        /// <returns>Number of new maps added</returns>
        private static int SyncWorkshopCollection(string collectionId)
        {
            try
            {
                string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={collectionId}";
                Instance.Logger.LogInformation("Fetching Workshop collection: {Url}", url);
                
                string pageSource;
                using (var response = httpClient.GetAsync(url).Result)
                {
                    response.EnsureSuccessStatusCode();
                    pageSource = response.Content.ReadAsStringAsync().Result;
                }
                
                // Regular expression to extract workshop IDs and map names
                var pattern = new Regex(@"<a href=""https://steamcommunity.com/sharedfiles/filedetails/\?id=(\d+)"">.*?<div class=""workshopItemTitle"">(.*?)</div>", RegexOptions.Singleline);
                
                // Find all matches
                var matches = pattern.Matches(pageSource);
                
                if (matches.Count == 0)
                {
                    Instance.Logger.LogWarning("No maps found in Workshop collection {CollectionId}", collectionId);
                    return 0;
                }
                
                Instance.Logger.LogInformation("Found {Count} maps in Workshop collection {CollectionId}", matches.Count, collectionId);
                
                int newMapsAdded = 0;
                
                foreach (Match match in matches)
                {
                    string workshopId = match.Groups[1].Value;
                    string mapName = match.Groups[2].Value;
                    
                    // Format map name (remove special characters, replace spaces with underscores)
                    string formattedMapName = FormatMapName(mapName);
                    
                    // Check if we have an official map name for this workshop ID
                    string? officialMapName = MapConfigManager.GetOfficialMapName(workshopId);
                    
                    // Use the official map name if available, otherwise use the formatted name
                    string mapValueToUse = officialMapName ?? formattedMapName;
                    
                    // Check if map already exists in the configuration
                    if (!Variables.GlobalVariables.Maps.Any(m => m.MapWorkshopId == workshopId))
                    {
                        // Create new map configuration
                        Map newMap = new Map(
                            mapValue: mapValueToUse,
                            mapDisplay: mapName,
                            mapIsWorkshop: true,
                            mapWorkshopId: workshopId,
                            mapCycleEnabled: true,
                            mapCanVote: true,
                            mapMinPlayers: 0,
                            mapMaxPlayers: 64,
                            mapCycleStartTime: "",
                            mapCycleEndTime: "",
                            mapCooldownCycles: 10
                        );
                        
                        // Add to global maps list
                        Variables.GlobalVariables.Maps.Add(newMap);
                        Variables.GlobalVariables.CycleMaps.Add(newMap);
                        
                        // Save map configuration
                        MapConfigManager.SaveMapConfig(newMap);
                        
                        newMapsAdded++;
                        Instance.Logger.LogInformation("Added new Workshop map: {MapName} (ID: {WorkshopId})", formattedMapName, workshopId);
                    }
                }
                
                Instance.Logger.LogInformation("Added {Count} new maps from Workshop collection {CollectionId}", newMapsAdded, collectionId);
                return newMapsAdded;
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError(ex, "Error processing Workshop collection {CollectionId}", collectionId);
                return 0;
            }
        }

        /// <summary>
        /// Formats a map name by removing special characters and replacing spaces with underscores
        /// </summary>
        /// <param name="mapName">The original map name</param>
        /// <returns>The formatted map name</returns>
        private static string FormatMapName(string mapName)
        {
            // Remove any special characters or spaces from the map name
            return Regex.Replace(mapName, @"[^\w\s]", "").Replace(" ", "_");
        }
    }
}