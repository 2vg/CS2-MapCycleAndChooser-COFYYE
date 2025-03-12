﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using MapCycleAndChooser_COFYYE.Classes;
using MapCycleAndChooser_COFYYE.Variables;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace MapCycleAndChooser_COFYYE.Utils
{
    public static class MapUtils
    {
        public static MapCycleAndChooser Instance => MapCycleAndChooser.Instance;
        public static void PopulateMapsForVotes()
        {
            try
            {
                GlobalVariables.MapForVotes.Clear();
                var random = new Random();

                int currentPlayers = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).Count();
                Instance?.Logger.LogInformation("Populating maps for voting with {PlayerCount} players", currentPlayers);

                var enableMapCooldown = Instance?.Config?.EnableMapCooldown == true;
                
                var eligibleMaps = GlobalVariables.Maps
                    .Where(map =>
                        map.MapValue != Server.MapName &&
                        map.MapCanVote &&
                        map.MapMinPlayers <= currentPlayers &&
                        map.MapMaxPlayers >= currentPlayers &&
                        CheckMapInCycleTime(map) &&
                        (!enableMapCooldown || CooldownManager.GetMapCooldown(map.MapValue) <= 0)
                    )
                    .ToList();

                Instance?.Logger.LogInformation("Found {Count} eligible maps with cooldown check", eligibleMaps.Count);

                if (eligibleMaps.Count == 0)
                {
                    // If no maps are eligible due to cooldowns, include maps regardless of cooldown
                    if (enableMapCooldown)
                    {
                        eligibleMaps = GlobalVariables.Maps
                            .Where(map =>
                                map.MapValue != Server.MapName &&
                                map.MapCanVote &&
                                map.MapMinPlayers <= currentPlayers &&
                                map.MapMaxPlayers >= currentPlayers &&
                                CheckMapInCycleTime(map)
                            )
                            .ToList();
                        
                        Instance?.Logger.LogInformation("Ignoring cooldowns. Found {Count} eligible maps", eligibleMaps.Count);
                    }

                    if (eligibleMaps.Count == 0)
                    {
                        Instance?.Logger.LogWarning("No eligible maps found for voting");
                        return;
                    }
                }

                if (eligibleMaps.Count <= 5)
                {
                    GlobalVariables.MapForVotes.AddRange(eligibleMaps);
                    Instance?.Logger.LogInformation("Added all {Count} eligible maps to vote list", eligibleMaps.Count);
                    return;
                }

                // Randomly select 5 maps
                for (int i = 0; i < 5; i++)
                {
                    if (eligibleMaps.Count == 0)
                    {
                        break;
                    }

                    var randomIndex = random.Next(eligibleMaps.Count);
                    var selectedMap = eligibleMaps[randomIndex];

                    GlobalVariables.MapForVotes.Add(selectedMap);
                    Instance?.Logger.LogDebug("Added map to vote list: {MapName}", selectedMap.MapValue);

                    eligibleMaps.RemoveAt(randomIndex);
                }
                
                Instance?.Logger.LogInformation("Populated vote list with {Count} maps", GlobalVariables.MapForVotes.Count);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error populating maps for voting");
                GlobalVariables.MapForVotes.Clear(); // Ensure the list is cleared in case of error
            }
        }

        public static (Map?, string) GetWinningMap()
        {
            try
            {
                if (GlobalVariables.Votes == null || GlobalVariables.Votes.Count == 0)
                {
                    Instance?.Logger.LogInformation("No votes cast. Returning null map.");
                    return (null, "");
                }

                var mapPercentages = CalculateMapsVotePercentages();

                if (mapPercentages == null || mapPercentages.Count == 0)
                {
                    Instance?.Logger.LogInformation("No map percentages calculated. Returning null map.");
                    return (null, "");
                }

                double maxPercentage = mapPercentages.Values.Max();
                Instance?.Logger.LogInformation("Maximum vote percentage: {Percentage}%", maxPercentage);

                var topMaps = GlobalVariables.MapForVotes
                    .Where(map =>
                    {
                        string key = Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay;
                        return mapPercentages.TryGetValue(key, out var percentage) && percentage == maxPercentage;
                    })
                    .ToList();

                Instance?.Logger.LogInformation("Found {Count} maps with the highest percentage", topMaps.Count);

                if (GlobalVariables.Votes.ContainsKey("{menu.item.ignore.vote}"))
                {
                    var ignoreVotePercentage = mapPercentages.GetValueOrDefault("{menu.item.ignore.vote}", 0);
                    if (ignoreVotePercentage == maxPercentage)
                    {
                        topMaps.Add(new Map("{menu.item.ignore.vote}", "Ignore Vote", false, "", true, true, 0, 64, "", ""));
                        Instance?.Logger.LogInformation("'Ignore Vote' option tied for highest percentage");
                    }
                }

                if (GlobalVariables.Votes.ContainsKey("{menu.item.extend.map}"))
                {
                    var extendMapPercentage = mapPercentages.GetValueOrDefault("{menu.item.extend.map}", 0);
                    if (extendMapPercentage == maxPercentage)
                    {
                        topMaps.Add(new Map("{menu.item.extend.map}", "Extend Map", false, "", true, true, 0, 64, "", ""));
                        Instance?.Logger.LogInformation("'Extend Map' option tied for highest percentage");
                    }
                }

                var ignoreVoteOption = topMaps.FirstOrDefault(map => map.MapValue.Equals("{menu.item.ignore.vote}", StringComparison.OrdinalIgnoreCase));
                if (ignoreVoteOption != null)
                {
                    if (GlobalVariables.MapForVotes.Count != 0)
                    {
                        var random = new Random();
                        var randomMap = GlobalVariables.MapForVotes[random.Next(GlobalVariables.MapForVotes.Count)];
                        Instance?.Logger.LogInformation("'Ignore Vote' won. Selecting random map: {MapName}", randomMap.MapValue);
                        
                        // Notify players about the tie and random selection
                        var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
                        foreach (var player in players)
                        {
                            player.PrintToChat(Instance?.Localizer.ForPlayer(player, "vote.ignore.random") ?? "Ignore Vote option won. Selecting a random map.");
                        }
                        
                        return (randomMap, "ignorevote");
                    }

                    Instance?.Logger.LogInformation("'Ignore Vote' won but no maps available for random selection");
                    return (null, "ignorevote");
                }

                var extendMapOption = topMaps.FirstOrDefault(map => map.MapValue.Equals("{menu.item.extend.map}", StringComparison.OrdinalIgnoreCase));
                if (extendMapOption != null)
                {
                    Instance?.Logger.LogInformation("'Extend Map' option won");
                    return (null, "extendmap");
                }

                if (topMaps.Count > 1)
                {
                    var random = new Random();
                    var selectedMap = topMaps[random.Next(topMaps.Count)];
                    Instance?.Logger.LogInformation("Multiple maps tied for highest percentage. Randomly selected: {MapName}",
                        selectedMap.MapValue);
                    
                    // Notify players about the tie and random selection
                    var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
                    foreach (var player in players)
                    {
                        player.PrintToChat(Instance?.Localizer.ForPlayer(player, "vote.tie.random").Replace("{MAP_NAME}", selectedMap.MapValue) ??
                            $"Vote ended in a tie. Randomly selected: {selectedMap.MapValue}");
                    }
                    
                    return (selectedMap, "tie");
                }

                var winningMap = topMaps.FirstOrDefault();
                if (winningMap != null)
                {
                    Instance?.Logger.LogInformation("Winning map: {MapName} with {Percentage}%",
                        winningMap.MapValue, maxPercentage);
                }
                else
                {
                    Instance?.Logger.LogInformation("No winning map found");
                }
                
                return (winningMap, "");
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error determining winning map");
                return (null, "error");
            }
        }

        public static void AutoSetNextMap()
        {
            if (GlobalVariables.CycleMaps.Count > 0)
            {
                var enableMapCooldown = Instance?.Config?.EnableMapCooldown == true;
                
                if (Instance?.Config?.EnableRandomNextMap == true)
                {
                    // Filter maps with no cooldown if enabled
                    var eligibleMaps = enableMapCooldown
                        ? GlobalVariables.CycleMaps.Where(map => CooldownManager.GetMapCooldown(map.MapValue) <= 0).ToList()
                        : GlobalVariables.CycleMaps;
                    
                    // If no maps are eligible due to cooldowns, use all maps
                    if (eligibleMaps.Count == 0 && enableMapCooldown)
                    {
                        eligibleMaps = GlobalVariables.CycleMaps;
                    }
                    
                    GlobalVariables.NextMap = eligibleMaps[new Random().Next(eligibleMaps.Count)];
                }
                else
                {
                    if (GlobalVariables.NextMapIndex > GlobalVariables.CycleMaps.Count - 1)
                    {
                        GlobalVariables.NextMapIndex = 0;
                    }
                    
                    // If cooldown is enabled, find the next map with no cooldown
                    if (enableMapCooldown)
                    {
                        int startIndex = GlobalVariables.NextMapIndex;
                        bool foundEligibleMap = false;
                        
                        // Try to find a map with no cooldown
                        while (!foundEligibleMap)
                        {
                            if (CooldownManager.GetMapCooldown(GlobalVariables.CycleMaps[GlobalVariables.NextMapIndex].MapValue) <= 0)
                            {
                                foundEligibleMap = true;
                            }
                            else
                            {
                                GlobalVariables.NextMapIndex = (GlobalVariables.NextMapIndex + 1) % GlobalVariables.CycleMaps.Count;
                                
                                // If we've checked all maps and none are eligible, use the original next map
                                if (GlobalVariables.NextMapIndex == startIndex)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    
                    GlobalVariables.NextMap = GlobalVariables.CycleMaps[GlobalVariables.NextMapIndex];
                    GlobalVariables.NextMapIndex = (GlobalVariables.NextMapIndex + 1) % GlobalVariables.CycleMaps.Count;
                }
            }
            else
            {
                GlobalVariables.NextMap = new Map(Server.MapName, Server.MapName, false, "", false, false, 0, 64, "", "");
            }
        }
        
        // Decrease cooldown for all maps by 1
        public static void DecreaseCooldownForAllMaps()
        {
            if (Instance?.Config?.EnableMapCooldown != true)
                return;
                
            foreach (var map in GlobalVariables.Maps)
            {
                CooldownManager.DecreaseMapCooldown(map.MapValue);
            }
        }
        
        // Reset cooldown for a specific map
        public static void ResetMapCooldown(Map map)
        {
            if (Instance?.Config?.EnableMapCooldown != true)
                return;
                
            CooldownManager.ResetMapCooldown(map);
        }

        public static void AddPlayerToVotes(string mapValue, string playerSteamId)
        {
            if (!GlobalVariables.Votes.TryGetValue(mapValue, out List<string>? value))
            {
                value = ([]);
                GlobalVariables.Votes[mapValue] = value;
            }

            value.Add(playerSteamId);
        }

        public static Dictionary<string, int> CalculateMapsVotePercentages()
        {
            try
            {
                var percentages = new Dictionary<string, int>();

                if (GlobalVariables.Votes == null)
                {
                    Instance?.Logger.LogWarning("Votes collection is null when calculating percentages");
                    return percentages;
                }

                int totalVotes = GlobalVariables.Votes.Values.SelectMany(voteList => voteList).Count();
                Instance?.Logger.LogInformation("Calculating vote percentages for {TotalVotes} total votes", totalVotes);

                if (totalVotes == 0)
                {
                    Instance?.Logger.LogInformation("No votes cast, returning empty percentages");
                    return percentages;
                }

                foreach (var vote in GlobalVariables.Votes)
                {
                    try
                    {
                        string map = vote.Key;
                        int votesForMap = vote.Value?.Count ?? 0;

                        int percentage = (int)Math.Round((double)votesForMap / totalVotes * 100);
                        percentages[map] = percentage;
                        
                        Instance?.Logger.LogDebug("Map {MapName}: {VoteCount} votes, {Percentage}%",
                            map, votesForMap, percentage);
                    }
                    catch (Exception ex)
                    {
                        Instance?.Logger.LogError(ex, "Error calculating percentage for map {MapKey}", vote.Key);
                    }
                }

                return percentages;
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error calculating map vote percentages");
                return new Dictionary<string, int>();
            }
        }

        public static HookResult CheckAndPickMapsForVoting()
        {
            try
            {
                float maxLimit;
                float timeLeft;
                int minValue;

                if (Instance?.Config?.DependsOnTheRound == true)
                {
                    maxLimit = ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>() ?? 0;
                    minValue = Instance?.Config?.VoteTriggerTimeBeforeMapEnd ?? 3; // rounds
                }
                else
                {
                    maxLimit = ConVar.Find("mp_timelimit")?.GetPrimitiveValue<float>() ?? 0.0f;
                    minValue = (Instance?.Config?.VoteTriggerTimeBeforeMapEnd ?? 3) * 60; // from minutes to seconds
                }

                var gameRules = ServerUtils.GetGameRules();
                if (maxLimit > 0 && !GlobalVariables.VoteStarted && !GlobalVariables.VotedForCurrentMap && gameRules?.WarmupPeriod == false)
                {
                    if (Instance?.Config?.DependsOnTheRound == true && gameRules != null)
                    {
                        timeLeft = maxLimit - gameRules.TotalRoundsPlayed;
                    }
                    else
                    {
                        timeLeft = GlobalVariables.TimeLeft - GlobalVariables.CurrentTime;
                    }

                    if (timeLeft <= minValue)
                    {
                        try
                        {
                            MapUtils.PopulateMapsForVotes();

                            if (GlobalVariables.MapForVotes.Count < 1)
                            {
                                GlobalVariables.VotedForCurrentMap = true;
                                Instance?.Logger.LogInformation("The list of voting maps is empty. I'm suspending the vote.");
                                return HookResult.Continue;
                            }

                            foreach (var map in GlobalVariables.MapForVotes)
                            {
                                if (!GlobalVariables.Votes.ContainsKey(map.MapValue))
                                {
                                    GlobalVariables.Votes[map.MapValue] = [];
                                }
                            }

                            if (Instance?.Config?.DependsOnTheRound == true && Instance?.Config?.VoteMapOnFreezeTime == true)
                            {
                                Server.ExecuteCommand($"mp_freezetime {(Instance?.Config?.VoteMapDuration ?? GlobalVariables.FreezeTime) + 2}");
                                Instance?.Logger.LogInformation("Setting mp_freezetime to {FreezeTime} for vote",
                                    (Instance?.Config?.VoteMapDuration ?? GlobalVariables.FreezeTime) + 2);
                            }
                        }
                        catch (Exception ex)
                        {
                            Instance?.Logger.LogError(ex, "Error populating maps for voting");
                        }
                    }
                    else
                    {
                        if(Instance?.Config?.DependsOnTheRound == true)
                        {
                            Server.ExecuteCommand($"mp_freezetime {GlobalVariables.FreezeTime}");
                        }
                    }
                }
                else
                {
                    if (Instance?.Config?.DependsOnTheRound == true)
                    {
                        Server.ExecuteCommand($"mp_freezetime {GlobalVariables.FreezeTime}");
                    }
                }

                return HookResult.Continue;
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error in CheckAndPickMapsForVoting");
                return HookResult.Continue;
            }
        }

        public static HookResult CheckAndStartMapVoting()
        {
            try
            {
                float maxLimit;
                float timeLeft;
                int minValue;

                if (Instance?.Config?.DependsOnTheRound == true)
                {
                    maxLimit = (float)(ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>() ?? 0);
                    minValue = Instance?.Config?.VoteTriggerTimeBeforeMapEnd ?? 3; // rounds
                }
                else
                {
                    maxLimit = ConVar.Find("mp_timelimit")?.GetPrimitiveValue<float>() ?? 0.0f;
                    minValue = (Instance?.Config?.VoteTriggerTimeBeforeMapEnd ?? 3) * 60; // from minutes to seconds
                }

                var gameRules = ServerUtils.GetGameRules();
                if (maxLimit > 0 && !GlobalVariables.VoteStarted && !GlobalVariables.VotedForCurrentMap && gameRules?.WarmupPeriod == false)
                {
                    if(Instance?.Config?.DependsOnTheRound == true && gameRules != null)
                    {
                        timeLeft = maxLimit - gameRules.TotalRoundsPlayed;
                    }
                    else
                    {
                        timeLeft = GlobalVariables.TimeLeft - GlobalVariables.CurrentTime;
                    }

                    if (timeLeft <= minValue)
                    {
                        if (GlobalVariables.MapForVotes.Count < 1)
                        {
                            Instance?.Logger.LogInformation("The list of voting maps is empty. I'm suspending the vote.");
                            return HookResult.Continue;
                        }

                        GlobalVariables.VoteStarted = true;
                        GlobalVariables.IsVotingInProgress = true;

                        var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).ToList();
                        Instance?.Logger.LogInformation("Starting map vote with {Count} players", players.Count);

                        string? soundToPlay = "";
                        if (Instance?.Config?.Sounds.Count > 0)
                        {
                            soundToPlay = Instance?.Config.Sounds[new Random().Next(Instance?.Config?.Sounds.Count ?? 1)];
                        }

                        foreach (var player in players)
                        {
                            try
                            {
                                player.PrintToChat(Instance?.Localizer.ForPlayer(player, "vote.started") ?? "");

                                if (!string.IsNullOrEmpty(soundToPlay))
                                {
                                    player.ExecuteClientCommand($"play {soundToPlay}");
                                }

                                MenuUtils.ShowKitsuneMenuVoteMaps(player);
                            }
                            catch (Exception ex)
                            {
                                Instance?.Logger.LogError(ex, "Error showing vote menu to player {PlayerName}", player.PlayerName);
                            }
                        }

                        float duration = (float)(Instance?.Config?.VoteMapDuration ?? 15);
                        Instance?.Logger.LogInformation("Vote duration set to {Duration} seconds", duration);

                        Instance?.AddTimer(duration, () =>
                        {
                            try
                            {
                                var (winningMap, type) = MapUtils.GetWinningMap();

                                if (winningMap != null)
                                {
                                    GlobalVariables.NextMap = winningMap;
                                    Instance?.Logger.LogInformation("Vote completed. Next map: {MapName}", winningMap.MapValue);
                                }
                                else if (winningMap == null && type == "extendmap")
                                {
                                    if (Instance?.Config?.DependsOnTheRound == true)
                                    {
                                        int extendTime = Instance?.Config?.ExtendMapTime ?? 5;
                                        Server.ExecuteCommand($"mp_maxrounds {(int)timeLeft + extendTime}");
                                        Instance?.Logger.LogInformation("Vote to extend map. Setting mp_maxrounds to {Rounds}", (int)timeLeft + extendTime);
                                    }
                                    else
                                    {
                                        int extendTime = Instance?.Config?.ExtendMapTime ?? 5;
                                        Server.ExecuteCommand($"mp_timelimit {Math.Ceiling((float)timeLeft / 60) + extendTime}");
                                        Instance?.Logger.LogInformation("Vote to extend map. Setting mp_timelimit to {Minutes}", Math.Ceiling((float)timeLeft / 60) + extendTime);
                                    }
                                    GlobalVariables.VotedForExtendMap = true;
                                    GlobalVariables.VotedForCurrentMap = false;
                                }
                                else if (winningMap == null && type == "ignorevote")
                                {
                                    GlobalVariables.NextMap = MapUtils.GetRandomNextMapByPlayers();
                                    Instance?.Logger.LogInformation("Winning map is Ignore Vote. Next map is {NEXTMAP}", GlobalVariables.NextMap?.MapValue);
                                }
                                else if (winningMap == null && type == "tie")
                                {
                                    // This case is handled in GetWinningMap
                                    Instance?.Logger.LogInformation("Vote ended in a tie. Random map was selected.");
                                }
                                else if (winningMap == null && type == "error")
                                {
                                    GlobalVariables.NextMap = MapUtils.GetRandomNextMapByPlayers();
                                    Instance?.Logger.LogWarning("Error during vote processing. Selecting random map: {MapName}", GlobalVariables.NextMap?.MapValue);
                                }
                                else
                                {
                                    GlobalVariables.NextMap = MapUtils.GetRandomNextMapByPlayers();
                                    Instance?.Logger.LogInformation("No winning map determined. Selecting random map: {MapName}", GlobalVariables.NextMap?.MapValue);
                                }

                                // Clean up
                                GlobalVariables.Votes.Clear();
                                GlobalVariables.MapForVotes.Clear();
                                GlobalVariables.VoteStarted = false;
                                Instance?.AddTimer(1.0f, () => GlobalVariables.IsVotingInProgress = false);

                                if (type != "extendmap")
                                {
                                    GlobalVariables.VotedForCurrentMap = true;
                                }

                                // Notify players
                                var activePlayers = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).ToList();
                                foreach (var player in activePlayers)
                                {
                                    try
                                    {
                                        if (type == "extendmap")
                                        {
                                            player.PrintToChat(Instance?.Localizer.ForPlayer(player, "vote.finished.extend.map.round").Replace("{EXTENDED_TIME}", Instance?.Config?.ExtendMapTime.ToString()) ?? "");
                                        }
                                        else
                                        {
                                            player.PrintToChat(Instance?.Localizer.ForPlayer(player, "vote.finished").Replace("{MAP_NAME}", GlobalVariables.NextMap?.MapValue) ?? "");
                                        }

                                        GlobalVariables.KitsuneMenu?.ClearMenus(player);
                                    }
                                    catch (Exception ex)
                                    {
                                        Instance?.Logger.LogError(ex, "Error notifying player {PlayerName} about vote results", player.PlayerName);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Instance?.Logger.LogError(ex, "Error processing vote results");
                                
                                // Ensure we clean up even if there's an error
                                GlobalVariables.Votes.Clear();
                                GlobalVariables.MapForVotes.Clear();
                                GlobalVariables.VoteStarted = false;
                                GlobalVariables.IsVotingInProgress = false;
                                GlobalVariables.VotedForCurrentMap = true;
                            }
                        }, TimerFlags.STOP_ON_MAPCHANGE);
                    }
                }

                return HookResult.Continue;
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error in CheckAndStartMapVoting");
                return HookResult.Continue;
            }
        }

        public static Map? GetRandomNextMapByPlayers()
        {
            try
            {
                int currentPlayers = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).Count();
                Instance?.Logger.LogInformation("Getting random map for {PlayerCount} players", currentPlayers);

                var maps = GlobalVariables.Maps
                                          .Where(map =>
                                            map.MapValue != Server.MapName &&
                                            map.MapMinPlayers <= currentPlayers &&
                                            map.MapMaxPlayers >= currentPlayers
                                          )
                                          .ToList();

                Instance?.Logger.LogInformation("Found {Count} eligible maps based on player count", maps.Count);

                if (maps.Count < 1)
                {
                    var fallbackMap = GlobalVariables.CycleMaps.FirstOrDefault();
                    if (fallbackMap != null)
                    {
                        Instance?.Logger.LogInformation("No eligible maps found. Using fallback map: {MapName}", fallbackMap.MapValue);
                    }
                    else
                    {
                        Instance?.Logger.LogWarning("No eligible maps found and no fallback maps available");
                    }
                    return fallbackMap;
                }

                var selectedMap = maps[new Random().Next(maps.Count)];
                Instance?.Logger.LogInformation("Randomly selected map: {MapName}", selectedMap.MapValue);
                return selectedMap;
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error selecting random map by player count");
                
                // Fallback to first cycle map in case of error
                var fallbackMap = GlobalVariables.CycleMaps.FirstOrDefault();
                Instance?.Logger.LogInformation("Using fallback map due to error: {MapName}",
                    fallbackMap?.MapValue ?? "none available");
                return fallbackMap;
            }
        }

        public static bool CheckMapInCycleTime(Map map)
        {
            try
            {
                if (map == null)
                {
                    Instance?.Logger.LogWarning("CheckMapInCycleTime called with null map");
                    return true;
                }
                
                var start = map.MapCycleStartTime;
                var end = map.MapCycleEndTime;

                // if both are empty, lets ignore the setting
                if (string.IsNullOrWhiteSpace(start) || string.IsNullOrWhiteSpace(end))
                {
                    return true;
                }

                DateTime parsedStart, parsedEnd;
                
                // Try to parse the start time
                if (!DateTime.TryParseExact(start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStart))
                {
                    Instance?.Logger.LogWarning("Invalid start time format for map {MapName}: {StartTime}. Expected format: HH:mm",
                        map.MapValue, start);
                    return true; // Allow the map if we can't parse the time
                }
                
                // Try to parse the end time
                if (!DateTime.TryParseExact(end, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedEnd))
                {
                    Instance?.Logger.LogWarning("Invalid end time format for map {MapName}: {EndTime}. Expected format: HH:mm",
                        map.MapValue, end);
                    return true; // Allow the map if we can't parse the time
                }

                var now = DateTime.Now;
                var startTime = now.Date.Add(parsedStart.TimeOfDay);
                var endTime = now.Date.Add(parsedEnd.TimeOfDay);
            
                // if 'start' - 'end' into the next day
                // example: 23:00 - 05:00
                if (startTime > endTime)
                {
                    endTime = endTime.AddDays(1);
                }

                bool isInTimeRange = now >= startTime && now <= endTime;
                
                if (!isInTimeRange)
                {
                    Instance?.Logger.LogDebug("Map {MapName} is not available at current time {CurrentTime}. Available from {StartTime} to {EndTime}",
                        map.MapValue, now.ToString("HH:mm"), start, end);
                }
                
                return isInTimeRange;
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error checking map cycle time for map {MapName}", map?.MapValue ?? "unknown");
                return true; // Allow the map if there's an error
            }
        }
    }
}
