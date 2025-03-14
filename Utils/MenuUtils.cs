﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System.Text;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Timers;
using Mappen.Classes;
using Mappen.Variables;
using Microsoft.Extensions.Logging;
using Menu;
using Menu.Enums;

namespace Mappen.Utils
{
    public static class MenuUtils
    {
        public static Mappen Instance => Mappen.Instance;
        public static Dictionary<string, PlayerMenu> PlayersMenu { get; } = [];

        public static void ShowKitsuneMenuVoteMaps(CCSPlayerController player)
        {
            try
            {
                if (GlobalVariables.KitsuneMenu == null)
                {
                    Instance?.Logger.LogError("Menu object is null. Cannot show fortnite menu to {PlayerName}.", player?.PlayerName);
                    return;
                }

                string playerSteamId = player.SteamID.ToString();
                if (!PlayersMenu.TryGetValue(playerSteamId, out PlayerMenu? pm)) return;

                List<MenuItem> items = [];
                var mapsDict = new Dictionary<int, object>();
                int i = 0;

                string mapTitle = Instance?.Localizer?.ForPlayer(player, "menu.item.map") ?? "";
                foreach (Map map in GlobalVariables.MapForVotes)
                {
                    string mapText = map != null
                        ? (Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay)
                        : "Unknown Map";

                    items.Add(new MenuItem(MenuItemType.Button, [new MenuValue(mapTitle.Replace("{MAP_NAME}", mapText))]));
                    mapsDict[i++] = map.MapValue; // 常に技術名を使用
                }

                GlobalVariables.KitsuneMenu?.ShowScrollableMenu(player, Instance?.Localizer.ForPlayer(player, "menu.title.vote") ?? "", items, (buttons, menu, selected) =>
                {
                    if (selected == null) return;

                    if (buttons == MenuButtons.Select && mapsDict.TryGetValue(menu.Option, out var mapSelected) && !pm.Selected)
                    {
                        var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));

                        if (Instance?.Config?.EnablePlayerVotingInChat == true)
                        {
                            foreach (var p in players)
                            {
                                p.PrintToChat(Instance.Localizer.ForPlayer(p, "vote.player").Replace("{PLAYER_NAME}", p.PlayerName).Replace("{MAP_NAME}", (string)mapSelected));
                            }
                        }

                        MapUtils.AddPlayerToVotes((string)mapSelected, playerSteamId);

                        PlayersMenu[playerSteamId].Selected = true;
                        player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                    }
                }, false, freezePlayer: Instance?.Config?.EnablePlayerFreezeInMenu ?? false, defaultValues: mapsDict, disableDeveloper: true);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error showing vote menu to player {PlayerName}", player?.PlayerName);
            }
        }

        public static void ShowKitsuneMenuMaps(CCSPlayerController player)
        {
            try
            {
                if (GlobalVariables.KitsuneMenu == null)
                {
                    Instance?.Logger.LogError("Menu object is null. Cannot show fortnite menu to {PlayerName}.", player?.PlayerName);
                    return;
                }

                string playerSteamId = player.SteamID.ToString();
                if (!PlayersMenu.TryGetValue(playerSteamId, out PlayerMenu? pm)) return;

                List<MenuItem> items = [];
                var mapsDict = new Dictionary<int, object>();
                int i = 0;

                string mapTitle = Instance?.Localizer?.ForPlayer(player, "menu.item.map") ?? "";
                foreach (Map map in GlobalVariables.Maps)
                {
                    string mapText = map != null
                        ? (Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay)
                        : "Unknown Map";

                    items.Add(new MenuItem(MenuItemType.Button, [new MenuValue(mapTitle.Replace("{MAP_NAME}", mapText))]));
                    mapsDict[i++] = map.MapValue;
                }

                GlobalVariables.KitsuneMenu?.ShowScrollableMenu(player, Instance?.Localizer.ForPlayer(player, "menu.title.maps") ?? "", items, (buttons, menu, selected) =>
                {
                    try
                    {
                        if (selected == null) return;

                        if(buttons == MenuButtons.Select && mapsDict.TryGetValue(menu.Option, out var mapSelected))
                        {
                            Map? map = GlobalVariables.Maps.Find(map => map.MapValue == (string)mapSelected || map.MapDisplay == (string)mapSelected);

                            if (map != null)
                            {
                                var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));

                                foreach (var p in players)
                                {
                                    p.PrintToChat(Instance?.Localizer.ForPlayer(p, "admin.change.map").Replace("{PLAYER_NAME}", p.PlayerName).Replace("{MAP_NAME}", (string)mapSelected) ?? "");
                                }

                                player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");

                                Instance?.AddTimer(2.0f, () => MapUtils.ChangeMap(map), TimerFlags.STOP_ON_MAPCHANGE);
                            }
                            else
                            {
                                player.PrintToChat(Instance?.Localizer.ForPlayer(player, "map.not.found") ?? "");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Instance?.Logger.LogError(ex, "Error processing map selection for player {PlayerName}", player?.PlayerName);
                    }
                }, false, freezePlayer: Instance?.Config?.EnablePlayerFreezeInMenu ?? false, defaultValues: mapsDict, disableDeveloper: true);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error showing maps menu to player {PlayerName}", player?.PlayerName);
            }
        }

        public static void CreateAndOpenHtmlVoteMenu(CCSPlayerController player)
        {
            try
            {
                string playerSteamId = player.SteamID.ToString();
                if (!PlayersMenu.TryGetValue(playerSteamId, out PlayerMenu? pm)) return;

                List<string> menuValues = [];

                if(Instance?.Config?.EnableIgnoreVote == true && Instance.Config?.IgnoreVotePosition == "top")
                {
                    menuValues.Add("{menu.item.ignore.vote}{splitignorevote}" + Instance?.Localizer.ForPlayer(player, "menu.item.ignore.vote") ?? "-");
                }

                if (Instance?.Config?.EnableExtendMap == true && Instance.Config?.ExtendMapPosition == "top" && GlobalVariables.VotedForExtendMap == false)
                {
                    if(Instance?.Config?.PrioritizeRounds == true)
                    {
                        menuValues.Add("{menu.item.extend.map}{splitextendmap}" + Instance?.Localizer.ForPlayer(player, "menu.item.extend.map.round").Replace("{EXTEND_TIME}", Instance?.Config?.ExtendMapTime.ToString()) ?? "-");
                    }
                    else
                    {
                        menuValues.Add("{menu.item.extend.map}{splitextendmap}" + Instance?.Localizer.ForPlayer(player, "menu.item.extend.map.timeleft").Replace("{EXTEND_TIME}", Instance?.Config?.ExtendMapTime.ToString()) ?? "-");
                    }
                }

                foreach (Map map in GlobalVariables.MapForVotes)
                {
                    menuValues.Add(Instance?.Config?.DisplayMapByValue == true ? map.MapValue : map.MapDisplay);
                }

                if (Instance?.Config?.EnableIgnoreVote == true && Instance.Config?.IgnoreVotePosition == "bottom")
                {
                    menuValues.Add("{menu.item.ignore.vote}{splitignorevote}" + Instance?.Localizer.ForPlayer(player, "menu.item.ignore.vote") ?? "-");
                }

                if (Instance?.Config?.EnableExtendMap == true && Instance?.Config?.ExtendMapPosition == "bottom" && GlobalVariables.VotedForExtendMap == false)
                {
                    if (Instance.Config?.PrioritizeRounds == true)
                    {
                        menuValues.Add("{menu.item.extend.map}{splitextendmap}" + Instance?.Localizer.ForPlayer(player, "menu.item.extend.map.round").Replace("{EXTEND_TIME}", Instance?.Config?.ExtendMapTime.ToString()) ?? "-");
                    }
                    else
                    {
                        menuValues.Add("{menu.item.extend.map}{splitextendmap}" + Instance?.Localizer.ForPlayer(player, "menu.item.extend.map.timeleft").Replace("{EXTEND_TIME}", Instance?.Config?.ExtendMapTime.ToString()) ?? "-");
                    }
                }

                int currentIndex = PlayersMenu[playerSteamId].CurrentIndex;
                currentIndex = Math.Max(0, Math.Min(menuValues.ToArray().Length - 1, currentIndex));

                string bottomMenu = Instance?.Localizer.ForPlayer(player, "menu.bottom.vote") ?? "";
                string imageleft = Instance?.Localizer.ForPlayer(player, "menu.item.left") ?? "";
                string imageRight = Instance?.Localizer.ForPlayer(player, "menu.item.right") ?? "";

                int visibleOptions = 5;
                int startIndex = Math.Max(0, currentIndex - (visibleOptions - 1));

                if (GlobalVariables.Timers.ElapsedMilliseconds >= 70 && !pm.Selected)
                {
                    try
                    {
                        switch (player.Buttons)
                        {
                            case 0:
                                {
                                    pm.ButtonPressed = false;
                                    break;
                                }
                            case PlayerButtons.Back:
                                {
                                    currentIndex = Math.Min(menuValues.ToArray().Length - 1, currentIndex + 1);
                                    pm.CurrentIndex = currentIndex;
                                    player.ExecuteClientCommand("play sounds/ui/csgo_ui_contract_type4.vsnd_c");
                                    pm.ButtonPressed = true;
                                    break;
                                }
                            case PlayerButtons.Forward:
                                {
                                    currentIndex = Math.Max(0, currentIndex - 1);
                                    pm.CurrentIndex = currentIndex;
                                    player.ExecuteClientCommand("play sounds/ui/csgo_ui_contract_type4.vsnd_c");
                                    pm.ButtonPressed = true;
                                    break;
                                }
                            case PlayerButtons.Use:
                                {
                                    string currentMenuOption = menuValues.ToArray()[currentIndex];

                                    var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));

                                    var isIgnoreVoteOption = currentMenuOption.Split("{splitignorevote}");
                                    var isExtendMapOption = currentMenuOption.Split("{splitextendmap}");

                                    if (Instance?.Config?.EnablePlayerVotingInChat == true)
                                    {
                                        foreach (var p in players)
                                        {
                                            if(isIgnoreVoteOption.Length > 1)
                                            {
                                                p.PrintToChat(Instance.Localizer.ForPlayer(p, "vote.player").Replace("{PLAYER_NAME}", p.PlayerName).Replace("{MAP_NAME}", isIgnoreVoteOption[1]));
                                            }
                                            else if(isExtendMapOption.Length > 1)
                                            {
                                                p.PrintToChat(Instance.Localizer.ForPlayer(p, "vote.player").Replace("{PLAYER_NAME}", p.PlayerName).Replace("{MAP_NAME}", isExtendMapOption[1]));
                                            }
                                            else
                                            {
                                                p.PrintToChat(Instance.Localizer.ForPlayer(p, "vote.player").Replace("{PLAYER_NAME}", p.PlayerName).Replace("{MAP_NAME}", currentMenuOption));
                                            }
                                        }
                                    }

                                    if (isIgnoreVoteOption.Length > 1)
                                    {
                                        MapUtils.AddPlayerToVotes(isIgnoreVoteOption[0], playerSteamId);
                                    }
                                    else if (isExtendMapOption.Length > 1)
                                    {
                                        MapUtils.AddPlayerToVotes(isExtendMapOption[0], playerSteamId);
                                    }
                                    else
                                    {
                                        MapUtils.AddPlayerToVotes(currentMenuOption, playerSteamId);
                                    }

                                    player.ExecuteClientCommand("play sounds/ui/item_sticker_select.vsnd_c");
                                    pm.ButtonPressed = true;
                                    pm.Selected = true;
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Instance?.Logger.LogError(ex, "Error processing button input for player {PlayerName}", player?.PlayerName);
                    }
                }

                StringBuilder builder = new();

                string menuTitle = Instance?.Localizer.ForPlayer(player, "menu.title.vote") ?? "";
                builder.AppendLine(menuTitle);

                var percentages = MapUtils.CalculateMapsVotePercentages();

                for (int i = startIndex; i < startIndex + visibleOptions && i < menuValues.ToArray().Length; i++)
                {
                    string currentMenuOption = menuValues.ToArray()[i];

                    int percentage = 0;
                    var isIgnoreVoteOption = currentMenuOption.Split("{splitignorevote}");
                    var isExtendMapOption = currentMenuOption.Split("{splitextendmap}");

                    if (isIgnoreVoteOption.Length > 1)
                    {
                        percentage = percentages.TryGetValue(isIgnoreVoteOption[0], out int mapPercent) ? mapPercent : 0;
                    }
                    else if(isExtendMapOption.Length > 1)
                    {
                        percentage = percentages.TryGetValue(isExtendMapOption[0], out int mapPercent) ? mapPercent : 0;
                    }
                    else
                    {
                        percentage = percentages.TryGetValue(currentMenuOption, out int mapPercent) ? mapPercent : 0;
                    }

                    if (i == currentIndex)
                    {
                        string lineHtml = "";

                        if(isIgnoreVoteOption.Length > 1)
                        {
                            lineHtml = $"{imageRight} <span color='yellow'>{isIgnoreVoteOption[1]}</span> <b color='orange'>•</b> <b color='lime'>{percentage}%</b> {imageleft} <br />";
                        }
                        else if (isExtendMapOption.Length > 1)
                        {
                            lineHtml = $"{imageRight} <span color='yellow'>{isExtendMapOption[1]}</span> <b color='orange'>•</b> <b color='lime'>{percentage}%</b> {imageleft} <br />";
                        }
                        else
                        {
                            lineHtml = $"{imageRight} {Instance?.Localizer.ForPlayer(player, "menu.item.vote").Replace("{MAP_NAME}", currentMenuOption).Replace("{MAP_PERCENT}", percentage.ToString())} {imageleft} <br />";
                        }

                        builder.AppendLine(lineHtml);
                    }
                    else
                    {
                        string lineHtml = "";

                        if (isIgnoreVoteOption.Length > 1)
                        {
                            lineHtml = $"<span color='yellow'>{isIgnoreVoteOption[1]}</span> <b color='orange'>•</b> <b color='lime'>{percentage}%</b> <br />";
                        }
                        else if (isExtendMapOption.Length > 1)
                        {
                            lineHtml = $"<span color='yellow'>{isExtendMapOption[1]}</span> <b color='orange'>•</b> <b color='lime'>{percentage}%</b> <br />";
                        }
                        else
                        {
                            lineHtml = $"{Instance?.Localizer.ForPlayer(player, "menu.item.vote").Replace("{MAP_NAME}", currentMenuOption).Replace("{MAP_PERCENT}", percentage.ToString())} <br />";
                        }

                        builder.AppendLine(lineHtml);
                    }
                }

                if (startIndex + visibleOptions < menuValues.ToArray().Length)
                {
                    string moreItemsIndicator = Instance?.Localizer.ForPlayer(player, "menu.more.items") ?? "";
                    builder.AppendLine(moreItemsIndicator);
                }

                builder.AppendLine(bottomMenu);
                builder.AppendLine("</div>");

                string centerhtml = builder.ToString();

                if (string.IsNullOrEmpty(PlayersMenu[playerSteamId].Html)) PlayersMenu[playerSteamId].Html = centerhtml;

                if (GlobalVariables.Timers.ElapsedMilliseconds >= 70)
                {
                    PlayersMenu[playerSteamId].Html = centerhtml;
                    GlobalVariables.Timers.Restart();
                }

                player?.PrintToCenterHtml(PlayersMenu[playerSteamId].Html);
            }
            catch (Exception ex)
            {
                Instance?.Logger.LogError(ex, "Error creating HTML vote menu for player {PlayerName}", player?.PlayerName);
            }
        }
    }
}
