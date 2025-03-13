using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using Mappen.Classes;
using Mappen.Variables;
using Microsoft.Extensions.Logging;
using Menu;
using Menu.Enums;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Mappen.Utils
{
    public static class NominateUtils
    {
        public static Mappen Instance => Mappen.Instance;

        // Check if player can nominate a map
        public static bool CanPlayerNominate(CCSPlayerController player)
        {
            if (!PlayerUtils.IsValidPlayer(player))
                return false;

            // Check if player has already nominated a map
            if (GlobalVariables.PlayerNominations.ContainsKey(player.SteamID.ToString()))
                return false;

            return true;
        }

        // Nominate a map
        public static bool NominateMap(CCSPlayerController player, Map map)
        {
            if (!CanPlayerNominate(player))
                return false;

            // Check if map is valid for nomination
            if (!IsMapValidForNomination(map, player))
                return false;

            // Add nomination
            GlobalVariables.NominatedMaps[map.MapValue] = map;
            GlobalVariables.PlayerNominations[player.SteamID.ToString()] = map.MapValue;

            // Notify all players
            var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
            foreach (var p in players)
            {
                p.PrintToChat(Instance.Localizer.ForPlayer(p, "nominate.map.added")
                    .Replace("{PLAYER_NAME}", player.PlayerName)
                    .Replace("{MAP_NAME}", map.MapValue));
            }

            return true;
        }

        // Check if map is valid for nomination
        public static bool IsMapValidForNomination(Map map, CCSPlayerController player)
        {
            // Cannot nominate current map
            if (map.MapValue == Server.MapName)
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.current.map"));
                return false;
            }

            // Check if map is already nominated
            if (GlobalVariables.NominatedMaps.ContainsKey(map.MapValue))
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.already.nominated"));
                return false;
            }

            // Check if map is in cycle and can be voted
            if (!map.MapCycleEnabled || !map.MapCanVote)
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.not.in.cycle"));
                return false;
            }

            // Check player count restrictions
            int currentPlayers = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).Count();
            if (map.MapMinPlayers > currentPlayers || map.MapMaxPlayers < currentPlayers)
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.player.count.invalid")
                    .Replace("{MIN}", map.MapMinPlayers.ToString())
                    .Replace("{MAX}", map.MapMaxPlayers.ToString())
                    .Replace("{CURRENT}", currentPlayers.ToString()));
                return false;
            }

            // Check time restrictions
            if (!MapUtils.CheckMapInCycleTime(map))
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.time.restricted"));
                return false;
            }

            // Check cooldown if enabled
            if (Instance.Config.EnableMapCooldown && CooldownManager.GetMapCooldown(map.MapValue) > 0)
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.on.cooldown")
                    .Replace("{COOLDOWN}", CooldownManager.GetMapCooldown(map.MapValue).ToString()));
                return false;
            }

            return true;
        }

        // Check if map is valid for force nomination (admin only)
        public static bool IsMapValidForForceNomination(Map map, CCSPlayerController player)
        {
            // Cannot nominate current map
            if (map.MapValue == Server.MapName)
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.current.map"));
                return false;
            }

            // Check if map is already nominated
            if (GlobalVariables.NominatedMaps.ContainsKey(map.MapValue))
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.already.nominated"));
                return false;
            }

            // Check if map is in cycle
            if (!map.MapCycleEnabled)
            {
                player?.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.not.in.cycle"));
                return false;
            }

            return true;
        }

        // Remove player's nomination
        public static bool RemovePlayerNomination(CCSPlayerController player)
        {
            string steamId = player.SteamID.ToString();
            
            if (!GlobalVariables.PlayerNominations.TryGetValue(steamId, out string? mapValue))
                return false;

            GlobalVariables.NominatedMaps.Remove(mapValue);
            GlobalVariables.PlayerNominations.Remove(steamId);

            // Notify all players
            var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
            foreach (var p in players)
            {
                p.PrintToChat(Instance.Localizer.ForPlayer(p, "nominate.map.removed")
                    .Replace("{PLAYER_NAME}", player.PlayerName)
                    .Replace("{MAP_NAME}", mapValue));
            }

            return true;
        }

        // Show current nominations
        public static void ShowNominations(CCSPlayerController player)
        {
            if (GlobalVariables.NominatedMaps.Count == 0)
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.no.maps"));
                return;
            }

            player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.current.header"));
            
            foreach (var nomination in GlobalVariables.NominatedMaps)
            {
                // Find the player who nominated this map
                string nominatorSteamId = GlobalVariables.PlayerNominations
                    .FirstOrDefault(p => p.Value == nomination.Key).Key;
                
                string nominatorName = "Unknown";
                if (!string.IsNullOrEmpty(nominatorSteamId))
                {
                    var nominator = Utilities.GetPlayers()
                        .FirstOrDefault(p => p.SteamID.ToString() == nominatorSteamId);
                    
                    if (nominator != null)
                        nominatorName = nominator.PlayerName;
                }

                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.current.entry")
                    .Replace("{MAP_NAME}", nomination.Value.MapValue)
                    .Replace("{PLAYER_NAME}", nominatorName));
            }
        }

        // Process nominate command with partial map name
        public static void ProcessNominateCommand(CCSPlayerController player, string partialMapName)
        {
            if (!CanPlayerNominate(player))
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.already.used"));
                return;
            }

            // Find maps that match the partial name
            var matchingMaps = FindMatchingMaps(partialMapName);
            
            if (matchingMaps.Count == 0)
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.no.matches")
                    .Replace("{MAP_NAME}", partialMapName));
                return;
            }
            
            if (matchingMaps.Count == 1)
            {
                // Only one match, nominate directly
                if (NominateMap(player, matchingMaps[0]))
                {
                    player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                }
                return;
            }
            
            // Multiple matches, show menu
            ShowMatchingMapsMenu(player, matchingMaps);
        }

        // Process force nominate command with partial map name (admin only)
        public static void ProcessForceNominateCommand(CCSPlayerController player, string partialMapName)
        {
            if (!PlayerUtils.IsValidPlayer(player)) return;

            // Check admin permissions
            if (!AdminManager.PlayerHasPermissions(player, "@css/changemap") && !AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "command.no.perm"));
                return;
            }

            if (!CanPlayerNominate(player))
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.already.used"));
                return;
            }

            // Find maps that match the partial name (for force nominate)
            var matchingMaps = FindMatchingMapsForForceNominate(partialMapName);
            
            if (matchingMaps.Count == 0)
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.no.matches")
                    .Replace("{MAP_NAME}", partialMapName));
                return;
            }
            
            if (matchingMaps.Count == 1)
            {
                // Only one match, force nominate directly
                if (ForceNominateMap(player, matchingMaps[0]))
                {
                    player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                }
                return;
            }
            
            // Multiple matches, show menu
            ShowMatchingMapsMenuForForceNominate(player, matchingMaps);
        }

        // Force nominate a map (admin only)
        public static bool ForceNominateMap(CCSPlayerController player, Map map)
        {
            if (!PlayerUtils.IsValidPlayer(player)) return false;

            // Check admin permissions
            if (!AdminManager.PlayerHasPermissions(player, "@css/changemap") && !AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "command.no.perm"));
                return false;
            }

            if (!CanPlayerNominate(player))
                return false;

            // Check if map is valid for force nomination
            if (!IsMapValidForForceNomination(map, player))
                return false;

            // Add nomination
            GlobalVariables.NominatedMaps[map.MapValue] = map;
            GlobalVariables.PlayerNominations[player.SteamID.ToString()] = map.MapValue;

            // Notify all players
            Server.PrintToChatAll(Instance.Localizer.ForPlayer(player, "nominate.map.added")
                .Replace("{PLAYER_NAME}", player.PlayerName)
                .Replace("{MAP_NAME}", map.MapValue));

            return true;
        }

        // Find maps matching partial name
        private static List<Map> FindMatchingMaps(string partialMapName)
        {
            int currentPlayers = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).Count();
            
            return GlobalVariables.Maps
                .Where(map =>
                    (map.MapValue.IndexOf(partialMapName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     map.MapDisplay.IndexOf(partialMapName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    map.MapValue != Server.MapName &&
                    map.MapCycleEnabled &&
                    map.MapCanVote &&
                    map.MapMinPlayers <= currentPlayers &&
                    map.MapMaxPlayers >= currentPlayers &&
                    MapUtils.CheckMapInCycleTime(map) &&
                    !GlobalVariables.NominatedMaps.ContainsKey(map.MapValue) &&
                    (!(Instance.Config.EnableMapCooldown) || CooldownManager.GetMapCooldown(map.MapValue) <= 0))
                .OrderBy(map => map.MapValue)
                .ToList();
        }

        // Find maps matching partial name for force nominate (admin only)
        private static List<Map> FindMatchingMapsForForceNominate(string partialMapName)
        {
            return GlobalVariables.Maps
                .Where(map =>
                    (map.MapValue.IndexOf(partialMapName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                     map.MapDisplay.IndexOf(partialMapName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    map.MapValue != Server.MapName &&
                    map.MapCycleEnabled &&
                    !GlobalVariables.NominatedMaps.ContainsKey(map.MapValue))
                .OrderBy(map => map.MapValue)
                .ToList();
        }

        // Show menu with matching maps
        private static void ShowMatchingMapsMenu(CCSPlayerController player, List<Map> matchingMaps)
        {
            if (GlobalVariables.KitsuneMenu == null)
            {
                Instance?.Logger.LogError("Menu object is null. Cannot show nominate menu to {PlayerName}.", player?.PlayerName);
                return;
            }

            string playerSteamId = player.SteamID.ToString();
            if (!MenuUtils.PlayersMenu.TryGetValue(playerSteamId, out PlayerMenu? pm)) return;

            List<MenuItem> items = [];
            var mapsDict = new Dictionary<int, object>();
            int i = 0;

            string mapTitle = Instance?.Localizer?.ForPlayer(player, "menu.item.map") ?? "";
            foreach (Map map in matchingMaps)
            {
                string mapText = map != null
                    ? (Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay)
                    : "Unknown Map";

                items.Add(new MenuItem(MenuItemType.Button, [new MenuValue(mapTitle.Replace("{MAP_NAME}", mapText))]));
                mapsDict[i++] = map;
            }

            GlobalVariables.KitsuneMenu?.ShowScrollableMenu(player, Instance?.Localizer.ForPlayer(player, "menu.title.nominate") ?? "", items, (buttons, menu, selected) =>
            {
                if (selected == null) return;

                if (buttons == MenuButtons.Select && mapsDict.TryGetValue(menu.Option, out var mapSelected))
                {
                    Map selectedMap = (Map)mapSelected;
                    
                    if (NominateMap(player, selectedMap))
                    {
                        player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                    }
                }
            }, false, freezePlayer: Instance?.Config?.EnablePlayerFreezeInMenu ?? false, defaultValues: mapsDict, disableDeveloper: true);
        }

        // Show menu with matching maps for force nominate (admin only)
        private static void ShowMatchingMapsMenuForForceNominate(CCSPlayerController player, List<Map> matchingMaps)
        {
            if (GlobalVariables.KitsuneMenu == null)
            {
                Instance?.Logger.LogError("Menu object is null. Cannot show force nominate menu to {PlayerName}.", player?.PlayerName);
                return;
            }

            string playerSteamId = player.SteamID.ToString();
            if (!MenuUtils.PlayersMenu.TryGetValue(playerSteamId, out PlayerMenu? pm)) return;

            List<MenuItem> items = [];
            var mapsDict = new Dictionary<int, object>();
            int i = 0;

            string mapTitle = Instance?.Localizer?.ForPlayer(player, "menu.item.map") ?? "";
            foreach (Map map in matchingMaps)
            {
                string mapText = map != null
                    ? (Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay)
                    : "Unknown Map";

                items.Add(new MenuItem(MenuItemType.Button, [new MenuValue(mapTitle.Replace("{MAP_NAME}", mapText))]));
                mapsDict[i++] = map;
            }

            GlobalVariables.KitsuneMenu?.ShowScrollableMenu(player, Instance?.Localizer.ForPlayer(player, "menu.title.nominate") ?? "", items, (buttons, menu, selected) =>
            {
                if (selected == null) return;

                if (buttons == MenuButtons.Select && mapsDict.TryGetValue(menu.Option, out var mapSelected))
                {
                    Map selectedMap = (Map)mapSelected;
                    
                    if (ForceNominateMap(player, selectedMap))
                    {
                        player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                    }
                }
            }, false, freezePlayer: Instance?.Config?.EnablePlayerFreezeInMenu ?? false, defaultValues: mapsDict, disableDeveloper: true);
        }

        // Show nominate menu with all eligible maps
        public static void ShowNominateMenu(CCSPlayerController player)
        {
            if (!CanPlayerNominate(player))
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.already.used"));
                return;
            }

            if (GlobalVariables.KitsuneMenu == null)
            {
                Instance?.Logger.LogError("Menu object is null. Cannot show nominate menu to {PlayerName}.", player?.PlayerName);
                return;
            }

            string playerSteamId = player.SteamID.ToString();
            if (!MenuUtils.PlayersMenu.TryGetValue(playerSteamId, out PlayerMenu? pm)) return;

            // Get eligible maps
            var eligibleMaps = FindMatchingMaps("");

            if (eligibleMaps.Count == 0)
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "nominate.no.eligible.maps"));
                return;
            }

            List<MenuItem> items = [];
            var mapsDict = new Dictionary<int, object>();
            int i = 0;

            string mapTitle = Instance?.Localizer?.ForPlayer(player, "menu.item.map") ?? "";
            foreach (Map map in eligibleMaps)
            {
                string mapText = map != null
                    ? (Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay)
                    : "Unknown Map";

                items.Add(new MenuItem(MenuItemType.Button, [new MenuValue(mapTitle.Replace("{MAP_NAME}", mapText))]));
                mapsDict[i++] = map;
            }

            GlobalVariables.KitsuneMenu?.ShowScrollableMenu(player, Instance?.Localizer.ForPlayer(player, "menu.title.nominate") ?? "", items, (buttons, menu, selected) =>
            {
                if (selected == null) return;

                if (buttons == MenuButtons.Select && mapsDict.TryGetValue(menu.Option, out var mapSelected))
                {
                    Map selectedMap = (Map)mapSelected;
                    
                    if (NominateMap(player, selectedMap))
                    {
                        player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                    }
                }
            }, false, freezePlayer: Instance?.Config?.EnablePlayerFreezeInMenu ?? false, defaultValues: mapsDict, disableDeveloper: true);
        }

        // Reset all nominations
        public static void ResetNominations()
        {
            GlobalVariables.NominatedMaps.Clear();
            GlobalVariables.PlayerNominations.Clear();
            Instance?.Logger.LogInformation("All map nominations have been reset");
        }
    }
}