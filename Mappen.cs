﻿using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using Mappen.Utils;
using Mappen.Classes;
using CounterStrikeSharp.API.Modules.Memory;
using Mappen.Variables;
using Menu;
using System.Runtime.InteropServices;
namespace Mappen;

public class Mappen : BasePlugin, IPluginConfig<Config.Config>
{
    public override string ModuleName => "Map Cycle and Chooser";
    public override string ModuleVersion => "2.0";
    public override string ModuleAuthor => "cofyye, 2vg";
    public override string ModuleDescription => "https://github.com/2vg";
    public static Mappen Instance { get; set; } = new();
    public Config.Config Config { get; set; } = new();

    public string workShopID = "";
    public string workShopURL = "";

    private delegate IntPtr GetAddonNameDelegate(IntPtr thisPtr);
    private readonly ForceFullUpdate.INetworkServerService networkServerService = new();

    public string GetAddonID()
    {
        IntPtr networkGameServer = networkServerService.GetIGameServer().Handle;
        IntPtr vtablePtr = Marshal.ReadIntPtr(networkGameServer);
        IntPtr functionPtr = Marshal.ReadIntPtr(vtablePtr + (25 * IntPtr.Size));
        var getAddonName = Marshal.GetDelegateForFunctionPointer<GetAddonNameDelegate>(functionPtr);
        IntPtr result = getAddonName(networkGameServer);
        return Marshal.PtrToStringAnsi(result)!.Split(',')[0];
    }


    public void OnConfigParsed(Config.Config config)
    {
        Instance = this;

        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        Logger.LogInformation("Loaded Config version: {Version}", config.Version);
        Config = Utils.ConfigMigrator.MigrateConfig(config);
        Logger.LogInformation("Config version: {Version}", Config.Version);

        ServerUtils.CheckAndValidateConfig();

        // First try to load maps from individual config files
        Utils.MapConfigManager.LoadMapConfigs();

        // If no maps were loaded from individual files, use the ones from the global config
        if (GlobalVariables.Maps.Count == 0)
        {
            GlobalVariables.Maps = Config?.Maps ?? [];
            GlobalVariables.CycleMaps = Config?.Maps?.Where(map => map.MapCycleEnabled == true).ToList() ?? [];
            
            // Migrate maps from global config to individual files
            Utils.MapConfigManager.MigrateFromGlobalConfig();
        }

        Server.ExecuteCommand($"mp_match_restart_delay {Config?.DelayToChangeMapInTheEnd ?? 10}");
        Logger.LogInformation("mp_match_restart_delay are set to {RestartDelay}.", Config?.DelayToChangeMapInTheEnd ?? 10);

        Logger.LogInformation("Initialized {MapCount} maps.", GlobalVariables.Maps.Count);
        Logger.LogInformation("Initialized {MapCount} cycle maps.", GlobalVariables.CycleMaps.Count);
        
        // Load saved cooldowns from file
        Utils.CooldownManager.LoadCooldowns();
        Logger.LogInformation("Loaded map cooldowns from file.");
        
        // Sync Workshop collections if enabled
        if (Config.EnableWorkshopCollectionSync)
        {
            Utils.WorkshopUtils.SyncWorkshopCollections();
        }
    }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        // Commands are now registered using attributes

        RegisterEventHandler<EventCsWinPanelMatch>(CsWinPanelMatchHandler);
        RegisterEventHandler<EventRoundStart>(RoundStartHandler);
        RegisterEventHandler<EventPlayerChat>(PlayerChatHandler);
        RegisterEventHandler<EventWarmupEnd>(WarmupEndHandler);
        RegisterEventHandler<EventPlayerConnectFull>(PlayerConnectFullHandler);
        RegisterEventHandler<EventPlayerDisconnect>(PlayerDisconnectHandler);

        if(Config?.RtvEnable == true)
        {
            GlobalVariables.MapStartTime = (float)GlobalVariables.Timers.Elapsed.TotalSeconds;
            GlobalVariables.RtvEnabled = false;
        }

        if(Config?.VoteMapEnable == true)
        {
            GlobalVariables.FreezeTime = ConVar.Find("mp_freezetime")?.GetPrimitiveValue<int>() ?? 5;
            GlobalVariables.VotedForCurrentMap = false;
            RegisterEventHandler<EventRoundEnd>(RoundEndHandler);
        }

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);

        //if(Config?.EnableScreenMenu == false)
        //{
            //RegisterListener<Listeners.OnTick>(OnTick);
        //}

        if(!GlobalVariables.Timers.IsRunning) GlobalVariables.Timers.Start();

        if(Config?.EnableCommandAdsInChat == true)
        {
            AddTimer(300.0f, () =>
            {
                var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).ToList();

                foreach (var player in players)
                {
                    switch (GlobalVariables.MessageIndex)
                    {
                        case 0:
                            {
                                player.PrintToChat(Localizer.ForPlayer(player, "nextmap.get.command.info"));
                                break;
                            }
                        case 1:
                            {
                                player.PrintToChat(Localizer.ForPlayer(player, "currentmap.get.command.info"));
                                break;
                            }
                        case 2:
                            {
                                player.PrintToChat(Localizer.ForPlayer(player, "lastmap.get.command.info"));
                                break;
                            }
                        case 3:
                            {
                                player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.info"));
                                break;
                            }
                        default:
                            {
                                GlobalVariables.MessageIndex = 0;
                                break;
                            }
                    }
                }

                if(GlobalVariables.MessageIndex + 1 >= 4)
                {
                    GlobalVariables.MessageIndex = 0;
                }
                else
                {
                    GlobalVariables.MessageIndex += 1;
                }
            }, TimerFlags.REPEAT);
        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);

        GlobalVariables.KitsuneMenu = new KitsuneMenu(this);
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);

        DeregisterEventHandler<EventCsWinPanelMatch>(CsWinPanelMatchHandler);
        DeregisterEventHandler<EventRoundStart>(RoundStartHandler);
        DeregisterEventHandler<EventPlayerChat>(PlayerChatHandler);
        DeregisterEventHandler<EventPlayerConnectFull>(PlayerConnectFullHandler);
        DeregisterEventHandler<EventPlayerDisconnect>(PlayerDisconnectHandler);
        DeregisterEventHandler<EventWarmupEnd>(WarmupEndHandler);

        if (Config?.VoteMapEnable == true)
        {
            DeregisterEventHandler<EventRoundEnd>(RoundEndHandler);
        }

        RemoveListener<Listeners.OnMapStart>(OnMapStart);
        RemoveListener<Listeners.OnMapEnd>(OnMapEnd);

        //if(Config?.EnableScreenMenu == false)
        //{
            //RemoveListener<Listeners.OnTick>(OnTick);
        //}

        if(GlobalVariables.Timers.IsRunning) GlobalVariables.Timers.Stop();
    }

    public void OnSetNextMap(CCSPlayerController? caller, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(caller)) return;

        if (!AdminManager.PlayerHasPermissions(caller, "@css/changemap") && !AdminManager.PlayerHasPermissions(caller, "@css/root"))
        {
            caller?.PrintToConsole(Localizer.ForPlayer(caller, "command.no.perm"));
            return;
        }

        if (command.ArgString == "")
        {
            caller?.PrintToConsole(Localizer.ForPlayer(caller, "nextmap.set.command.expected.value"));
            return;
        }

        Map? map = GlobalVariables.CycleMaps.Find(m => m.MapValue == command.GetArg(1));

        if (map == null)
        {
            caller?.PrintToConsole(Localizer.ForPlayer(caller, "map.not.found"));
            return;
        }

        GlobalVariables.NextMap = map;

        Server.PrintToChatAll(Localizer.ForPlayer(caller, "nextmap.set.command.new.map").Replace("{ADMIN_NAME}", caller?.PlayerName).Replace("{MAP_NAME}", GlobalVariables.NextMap?.MapValue));
        
        return;
    }

    public void OnMapsList(CCSPlayerController? caller, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(caller)) return;

        if (!AdminManager.PlayerHasPermissions(caller, "@css/changemap") && !AdminManager.PlayerHasPermissions(caller, "@css/root"))
        {
            caller?.PrintToConsole(Localizer.ForPlayer(caller, "command.no.perm"));
            return;
        }

        MenuUtils.ShowKitsuneMenuMaps(caller!);
    }

    public void OnNominateMap(CCSPlayerController? caller, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(caller)) return;

        if (command.ArgString == "")
        {
            // Show nominate menu
            NominateUtils.ShowNominateMenu(caller!);
            return;
        }

        // Process nominate command with map name
        string mapName = command.ArgString.Trim();
        NominateUtils.ProcessNominateCommand(caller!, mapName);
    }

    public void OnForceNominateMap(CCSPlayerController? caller, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(caller)) return;

        // Check admin permissions
        if (!AdminManager.PlayerHasPermissions(caller, "@css/changemap") && !AdminManager.PlayerHasPermissions(caller, "@css/root"))
        {
            caller?.PrintToChat(Localizer.ForPlayer(caller, "command.no.perm"));
            return;
        }

        if (command.ArgString == "")
        {
            caller?.PrintToChat(Localizer.ForPlayer(caller, "nominate.force.usage"));
            return;
        }

        // Process force nominate command with map name
        string mapName = command.ArgString.Trim();
        NominateUtils.ProcessForceNominateCommand(caller!, mapName);
    }

    public void OnShowNominations(CCSPlayerController? caller, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(caller)) return;

        NominateUtils.ShowNominations(caller!);
    }

    public HookResult PlayerConnectFullHandler(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var steamId = @event?.Userid?.SteamID.ToString();

        if (string.IsNullOrEmpty(steamId)) return HookResult.Continue;

        MenuUtils.PlayersMenu.Add(steamId, new(){
            CurrentIndex = 0,
            ButtonPressed = false,
            MenuOpened = false,
            Selected = false,
            Html = ""
        });

        return HookResult.Continue;
    }

    public HookResult PlayerDisconnectHandler(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var steamId = @event?.Userid?.SteamID.ToString();

        if (string.IsNullOrEmpty(steamId)) return HookResult.Continue;

        MenuUtils.PlayersMenu.Remove(steamId);

        return HookResult.Continue;
    }

    public HookResult WarmupEndHandler(EventWarmupEnd @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        ServerUtils.InitializeCvars();

        GlobalVariables.TimeLeftTimer ??= AddTimer(1.0f, () =>
            {
                var timeLimit = ConVar.Find("mp_timelimit")?.GetPrimitiveValue<float>();

                GlobalVariables.TimeLeft = (timeLimit ?? 5.0f) * 60; // in seconds
                GlobalVariables.CurrentTime += 1;
            }, TimerFlags.REPEAT);

        if (Config?.DependsOnTheRound == false)
        {
            GlobalVariables.VotingTimer ??= AddTimer(3.0f, () =>
                {
                    if (GlobalVariables.IsVotingInProgress) return;

                    MapUtils.CheckAndPickMapsForVoting();
                    MapUtils.CheckAndStartMapVoting();
                }, TimerFlags.REPEAT);
        }

        return HookResult.Continue;
    }

    public HookResult CsWinPanelMatchHandler(EventCsWinPanelMatch @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        AddTimer((Config?.DelayToChangeMapInTheEnd ?? 10.0f) - 3.0f, () =>
        {
            try
            {
                // Save cooldowns before map change
                Utils.CooldownManager.SaveCooldowns();
                
                // Clean up resources
                GlobalVariables.Votes.Clear();
                GlobalVariables.MapForVotes.Clear();
                
                // Kill any active timers
                GlobalVariables.TimeLeftTimer?.Kill();
                GlobalVariables.VotingTimer?.Kill();
                
                // Set last map
                GlobalVariables.LastMap = Server.MapName;
                
                if (GlobalVariables.NextMap != null)
                {
                    // Use the common map change utility method
                    MapUtils.ChangeMap(GlobalVariables.NextMap);
                }
                else
                {
                    // If NextMap is null, use a random map from the cycle maps
                    if (GlobalVariables.CycleMaps.Count > 0)
                    {
                        var randomMap = GlobalVariables.CycleMaps[new Random().Next(GlobalVariables.CycleMaps.Count)];
                        Logger.LogInformation("NextMap was null, using random map: {MapName}", randomMap.MapValue);
                        
                        // Use the common map change utility method
                        MapUtils.ChangeMap(randomMap);
                    }
                    else
                    {
                        Logger.LogWarning("No next map set and no cycle maps available. Map will not change.");
                        Server.PrintToChatAll("No next map set and no cycle maps available. Map will not change.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during map change");
            }
        }, TimerFlags.STOP_ON_MAPCHANGE);

        return HookResult.Continue;
    }

    public HookResult RoundStartHandler(EventRoundStart @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        if (Config?.DependsOnTheRound == true)
        {
            return MapUtils.CheckAndStartMapVoting();
        }

        return HookResult.Continue;
    }

    public HookResult RoundEndHandler(EventRoundEnd @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        
        // Check if RTV was triggered and we need to change map at round end
        if (Config?.RtvEnable == true && GlobalVariables.RtvTriggered && !Config.RtvChangeInstantly)
        {
            try
            {
                if (GlobalVariables.NextMap != null)
                {
                    // Use the common map change utility method
                    MapUtils.ChangeMap(GlobalVariables.NextMap);
                }
                else
                {
                    Logger.LogWarning("RTV was triggered but NextMap is null. Map will not change.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during RTV map change in RoundEndHandler");
            }
            finally
            {
                // Always reset the flag, regardless of success or failure
                GlobalVariables.RtvTriggered = false;
            }
            
            return HookResult.Continue;
        }

        if(Config?.DependsOnTheRound == true)
        {
            return MapUtils.CheckAndPickMapsForVoting();
        }

        return HookResult.Continue;
    }

    public HookResult PlayerChatHandler(EventPlayerChat @event, GameEventInfo info)
    {
        if(@event == null) return HookResult.Continue;

        // RTV commands are now handled via attributes

        // Nominate commands are now handled via attributes

        // Map info commands are now handled via attributes

        return HookResult.Continue;
    }

    public void OnMapStart(string mapName)
    {
        if (Config?.RtvEnable == true)
        {
            GlobalVariables.MapStartTime = (float)GlobalVariables.Timers.Elapsed.TotalSeconds;
            GlobalVariables.RtvTriggered = false;
            RtvUtils.ResetRtv();
            Logger.LogInformation("RTV has been reset for new map");
        }

        if (Config?.VoteMapEnable == true)
        {
            GlobalVariables.VotedForCurrentMap = false;
            GlobalVariables.VotedForExtendMap = false;
            if (!GlobalVariables.Timers.IsRunning) GlobalVariables.Timers.Start();
        }

        // Reset nominations for new map
        NominateUtils.ResetNominations();
        Logger.LogInformation("Map nominations have been reset for new map");

        // Check if we need to reload map configs (in case they were modified externally)
        Utils.MapConfigManager.LoadMapConfigs();

        // Define currentMap variable outside the if-else block
        Map? currentMap = null;

        workShopID = "";

        // First create a temporary config if needed, which will be updated once we have the workshop ID
        if (!string.IsNullOrWhiteSpace(Server.MapName) && Server.MapName != "<empty>" && Server.MapName != "\u003Cempty\u003E")
        {
            // Check if the current map has a config file, if not create a temporary one
            currentMap = GlobalVariables.Maps.FirstOrDefault(m => m.MapValue == Server.MapName);
            if (currentMap == null)
            {
                // Create default config for this map (will be updated if it's a workshop map)
                currentMap = Utils.MapConfigManager.GetOrCreateMapConfig(Server.MapName, false, "");
                
                // Add to global maps list if not already there
                if (currentMap != null && !GlobalVariables.Maps.Any(m => m.MapValue == currentMap.MapValue))
                {
                    GlobalVariables.Maps.Add(currentMap);
                    if (currentMap.MapCycleEnabled)
                    {
                        GlobalVariables.CycleMaps.Add(currentMap);
                    }
                    Logger.LogInformation("Added new map to maps list: {MapName}", Server.MapName);
                }
            }
        }
        else
        {
            Logger.LogWarning("Current map name is empty or <empty>. Skipping map config creation.");
        }

        // Get workshop ID and update config after a delay to ensure the ID is available
        AddTimer(1.0f, () =>
        {
            workShopID = GetAddonID();
            if (!string.IsNullOrEmpty(workShopID))
            {
                string workshopUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={workShopID}";
                workShopURL = workshopUrl;
                
                // Update workshop mapping with the official map name
                if (!string.IsNullOrWhiteSpace(Server.MapName) && Server.MapName != "<empty>" && Server.MapName != "\u003Cempty\u003E")
                {
                    Utils.MapConfigManager.UpdateWorkshopMapping(workShopID, Server.MapName);
                    
                    // Merge any duplicate configs that might exist
                    Utils.MapConfigManager.MergeWorkshopConfigs(workShopID, Server.MapName);
                    
                    Logger.LogInformation("Updated workshop mapping for ID {WorkshopId} to map name {MapName}", workShopID, Server.MapName);
                    
                    // Now that we have the workshop ID, update the map config if needed
                    currentMap = GlobalVariables.Maps.FirstOrDefault(m => m.MapValue == Server.MapName);
                    
                    if (currentMap == null)
                    {
                        // Create a workshop map config
                        currentMap = new Map(
                            mapValue: Server.MapName,
                            mapDisplay: Server.MapName,
                            mapIsWorkshop: true,
                            mapWorkshopId: workShopID,
                            mapCycleEnabled: true,
                            mapCanVote: true,
                            mapMinPlayers: 0,
                            mapMaxPlayers: 64,
                            mapCycleStartTime: "",
                            mapCycleEndTime: "",
                            mapCooldownCycles: 10
                        );
                        
                        // Save the config
                        Utils.MapConfigManager.SaveMapConfig(currentMap);
                        
                        // Add to global maps list
                        if (!GlobalVariables.Maps.Any(m => m.MapValue == currentMap.MapValue))
                        {
                            GlobalVariables.Maps.Add(currentMap);
                            if (currentMap.MapCycleEnabled)
                            {
                                GlobalVariables.CycleMaps.Add(currentMap);
                            }
                        }
                        
                        Logger.LogInformation("Created new workshop map config: {MapName} with ID {WorkshopId}", Server.MapName, workShopID);
                    }
                    else if (!currentMap.MapIsWorkshop || currentMap.MapWorkshopId != workShopID)
                    {
                        // If the map exists but is not marked as a workshop map or has a different workshop ID,
                        // update the map to mark it as a workshop map with the correct ID
                        Map updatedMap = new Map(
                            mapValue: currentMap.MapValue,
                            mapDisplay: currentMap.MapDisplay,
                            mapIsWorkshop: true,
                            mapWorkshopId: workShopID,
                            mapCycleEnabled: currentMap.MapCycleEnabled,
                            mapCanVote: currentMap.MapCanVote,
                            mapMinPlayers: currentMap.MapMinPlayers,
                            mapMaxPlayers: currentMap.MapMaxPlayers,
                            mapCycleStartTime: currentMap.MapCycleStartTime,
                            mapCycleEndTime: currentMap.MapCycleEndTime,
                            mapCooldownCycles: currentMap.MapCooldownCycles
                        );
                        
                        // Save the updated config
                        Utils.MapConfigManager.SaveMapConfig(updatedMap);
                        
                        // Update in global maps list
                        int index = GlobalVariables.Maps.FindIndex(m => m.MapValue == currentMap.MapValue);
                        if (index >= 0)
                        {
                            GlobalVariables.Maps[index] = updatedMap;
                            currentMap = updatedMap;
                        }
                        
                        Logger.LogInformation("Updated existing map to mark as workshop map: {MapName} with ID {WorkshopId}", Server.MapName, workShopID);
                    }
                }
            }
        }, TimerFlags.STOP_ON_MAPCHANGE);

        // Decrease cooldown for all maps - this ensures we only decrease cooldowns when a map is actually loaded
        Utils.MapUtils.DecreaseCooldownForAllMaps();
        
        // Reset cooldown for the current map - this ensures we only reset cooldown when a map is actually played
        if (currentMap != null && Config?.EnableMapCooldown == true &&
            !string.IsNullOrWhiteSpace(Server.MapName) && Server.MapName != "<empty>" && Server.MapName != "\u003Cempty\u003E")
        {
            Utils.MapUtils.ResetMapCooldown(currentMap);
            Logger.LogInformation("Reset cooldown for current map: {MapName}", Server.MapName);
        }

        // Save cooldowns after map changes and cooldown updates
        Utils.CooldownManager.SaveCooldowns();
        Logger.LogInformation("Saved map cooldowns to file on map start.");

        if (Config?.VoteMapOnFreezeTime == true)
        {
            GlobalVariables.FreezeTime = ConVar.Find("mp_freezetime")?.GetPrimitiveValue<int>() ?? 5;
        }

        GlobalVariables.Votes.Clear();
        GlobalVariables.MapForVotes.Clear();
        MenuUtils.PlayersMenu.Clear();
        GlobalVariables.CurrentTime = 0.0f;
        GlobalVariables.TimeLeftTimer?.Kill();
        GlobalVariables.TimeLeftTimer = null;
        GlobalVariables.VotingTimer?.Kill();
        GlobalVariables.VotingTimer = null;
    }

    public void OnMapEnd()
    {
        if (Config?.RtvEnable == true)
        {
            GlobalVariables.RtvTriggered = false;
            RtvUtils.ResetRtv();
            Logger.LogInformation("RTV has been reset for map end");
        }

        if (Config?.VoteMapEnable == true)
        {
            GlobalVariables.VotedForCurrentMap = false;
            GlobalVariables.VotedForExtendMap = false;
            if (!GlobalVariables.Timers.IsRunning) GlobalVariables.Timers.Start();
        }

        GlobalVariables.Votes.Clear();
        GlobalVariables.MapForVotes.Clear();
        MenuUtils.PlayersMenu.Clear();
        GlobalVariables.CurrentTime = 0.0f;
        GlobalVariables.NextMap = null;
        GlobalVariables.TimeLeftTimer?.Kill();
        GlobalVariables.TimeLeftTimer = null;
        GlobalVariables.VotingTimer?.Kill();
        GlobalVariables.VotingTimer = null;
    }

    //public void OnTick()
    //{
    //    if (GlobalVariables.VoteStarted && Config?.VoteMapEnable == true)
    //    {
    //        var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));

    //        foreach (var player in players)
    //        {
    //            if (!MenuUtils.PlayersMenu.ContainsKey(player.SteamID.ToString())) continue;
    //            MenuUtils.PlayersMenu[player.SteamID.ToString()].MenuOpened = true;

    //            if (Config?.EnablePlayerFreezeInMenu == true)
    //            {
    //                if (player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
    //                {
    //                    player.PlayerPawn.Value!.MoveType = MoveType_t.MOVETYPE_NONE;
    //                    Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 0);
    //                    Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
    //                }
    //            }

    //            MenuUtils.CreateAndOpenHtmlVoteMenu(player);
    //        }
    //    }
    //    else if(!GlobalVariables.VoteStarted || Config?.VoteMapEnable == false)
    //    {
    //        var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));

    //        foreach (var player in players)
    //        {
    //            if (!MenuUtils.PlayersMenu.ContainsKey(player.SteamID.ToString())) continue;
    //            if (MenuUtils.PlayersMenu[player.SteamID.ToString()].MenuOpened)
    //            {
    //                if (Config?.EnablePlayerFreezeInMenu == true)
    //                {
    //                    if (player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
    //                    {
    //                        player.PlayerPawn.Value!.MoveType = MoveType_t.MOVETYPE_NONE;
    //                        Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 0);
    //                        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
    //                    }
    //                }

    //                MenuUtils.CreateAndOpenHtmlMapsMenu(player);
    //            }
    //            else
    //            {
    //                if(Config?.EnablePlayerFreezeInMenu == true)
    //                {
    //                    if (player.PlayerPawn.Value != null && player.PlayerPawn.Value.IsValid)
    //                    {
    //                        player.PlayerPawn.Value!.MoveType = MoveType_t.MOVETYPE_WALK;
    //                        Schema.SetSchemaValue(player.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
    //                        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    // Command registration using attributes

    [ConsoleCommand("css_setnextmap", "Set a next map")]
    [CommandHelper(minArgs: 1, usage: "[map]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/changemap")]
    public void OnSetNextMapCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnSetNextMap(player, command);
    }

    [ConsoleCommand("css_maps", "List of all maps")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/changemap")]
    public void OnMapsListCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnMapsList(player, command);
    }

    [ConsoleCommand("css_nominate", "Nominate a map for voting")]
    [CommandHelper(usage: "[map]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnNominateCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnNominateMap(player, command);
    }

    [ConsoleCommand("css_nom", "Nominate a map for voting")]
    [CommandHelper(usage: "[map]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnNomShortCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnNominateMap(player, command);
    }

    [ConsoleCommand("css_force_nominate", "Force nominate a map for voting (admin only)")]
    [CommandHelper(usage: "[map]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/changemap")]
    public void OnForceNominateCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnForceNominateMap(player, command);
    }

    [ConsoleCommand("css_force_nom", "Force nominate a map for voting (admin only)")]
    [CommandHelper(usage: "[map]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/changemap")]
    public void OnForceNomShortCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnForceNominateMap(player, command);
    }

    [ConsoleCommand("css_nomlist", "Show current map nominations")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnNomListCommand(CCSPlayerController? player, CommandInfo command)
    {
        OnShowNominations(player, command);
    }

    [ConsoleCommand("css_rtv", "Rock the vote to change the map")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnRtvCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(player)) return;

        if (Config?.RtvEnable != true)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "rtv.disabled"));
            return;
        }

        if (RtvUtils.CanPlayerRtv(player))
        {
            RtvUtils.AddPlayerRtv(player);
        }
        else
        {
            // Check why player can't RTV
            float currentTime = (float)GlobalVariables.Timers.Elapsed.TotalSeconds;
            if (currentTime - GlobalVariables.MapStartTime < Config.RtvDelay)
            {
                int remainingTime = (int)(Config.RtvDelay - (currentTime - GlobalVariables.MapStartTime));
                player.PrintToChat(Localizer.ForPlayer(player, "rtv.too.early").Replace("{TIME}", remainingTime.ToString()));
            }
            else if (GlobalVariables.RtvPlayers.Contains(player.SteamID.ToString()))
            {
                player.PrintToChat(Localizer.ForPlayer(player, "rtv.already.voted"));
            }
            else if (GlobalVariables.IsVotingInProgress || GlobalVariables.VoteStarted)
            {
                player.PrintToChat(Localizer.ForPlayer(player, "rtv.vote.in.progress"));
            }
        }
    }

    [ConsoleCommand("css_nextmap", "Show the next map")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnNextMapCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(player)) return;

        if (Config?.EnableNextMapCommand != true)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "nextmap.get.command.disabled"));
            return;
        }

        if (GlobalVariables.NextMap != null)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "nextmap.get.command").Replace("{MAP_NAME}", GlobalVariables.NextMap.MapValue));
        }
        else
        {
            player.PrintToChat(Localizer.ForPlayer(player, "nextmap.get.command.not.set"));
        }
    }

    [ConsoleCommand("css_currentmap", "Show the current map")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnCurrentMapCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(player)) return;

        if (Config?.EnableCurrentMapCommand != true)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "currentmap.get.command.disabled"));
            return;
        }

        player.PrintToChat(Localizer.ForPlayer(player, "currentmap.get.command").Replace("{MAP_NAME}", Server.MapName));
    }

    [ConsoleCommand("css_timeleft", "Show time left on the current map")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnTimeLeftCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(player)) return;

        if (Config?.EnableTimeLeftCommand != true)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.disabled"));
            return;
        }

        var gameRules = ServerUtils.GetGameRules();

        // Check if RTV has been triggered
        if (GlobalVariables.RtvTriggered)
        {
            // If RTV has been triggered, inform the player that the map will change at the end of the round
            player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.rtv.triggered").Replace("{MAP_NAME}", GlobalVariables.NextMap?.MapValue ?? ""));
        }
        // Check if a vote has completed and next map is set, but not through RTV
        else if (GlobalVariables.VotedForCurrentMap && GlobalVariables.NextMap != null)
        {
            // If a vote has completed through normal time-based voting, show the next map but don't imply immediate change
            player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.vote.completed").Replace("{MAP_NAME}", GlobalVariables.NextMap?.MapValue ?? ""));
        }
        else if(Config?.DependsOnTheRound == true && gameRules != null)
        {
            var maxRounds = ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>() ?? 0;
            var roundLeft = maxRounds - gameRules.TotalRoundsPlayed;

            if(roundLeft > 0)
            {
                player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.round").Replace("{TIME_LEFT}", roundLeft.ToString()));
            }
            else
            {
                player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.expired"));
            }
        }
        else if(Config?.DependsOnTheRound == true && gameRules == null)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.unavailable"));
        }
        else
        {
            var timeLeft = GlobalVariables.TimeLeft - GlobalVariables.CurrentTime;
            var minutes = Math.Ceiling(timeLeft / 60);

            if(minutes > 0)
            {
                player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.timeleft").Replace("{TIME_LEFT}", minutes.ToString()));
            }
            else
            {
                player.PrintToChat(Localizer.ForPlayer(player, "timeleft.get.command.expired"));
            }
        }
    }

    [ConsoleCommand("css_lastmap", "Show the last map played")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnLastMapCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(player)) return;

        if (Config?.EnableLastMapCommand != true)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "lastmap.get.command.disabled"));
            return;
        }

        if(string.IsNullOrEmpty(GlobalVariables.LastMap))
        {
            player.PrintToChat(Localizer.ForPlayer(player, "lastmap.get.command.null"));
        }
        else
        {
            player.PrintToChat(Localizer.ForPlayer(player, "lastmap.get.command").Replace("{MAP_NAME}", GlobalVariables.LastMap));
        }
    }
    
    [ConsoleCommand("css_force_rtv", "Force Rock the Vote to change the map (admin only)")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/changemap")]
    public void OnForceRtvCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (!PlayerUtils.IsValidPlayer(player)) return;
        
        if (Config?.RtvEnable != true)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "rtv.force.disabled"));
            return;
        }
        
        if (GlobalVariables.IsVotingInProgress || GlobalVariables.VoteStarted)
        {
            player.PrintToChat(Localizer.ForPlayer(player, "rtv.vote.in.progress"));
            return;
        }
        
        // Notify all players that an admin has forced RTV
        var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
        foreach (var p in players)
        {
            p.PrintToChat(Localizer.ForPlayer(p, "rtv.force.success").Replace("{ADMIN_NAME}", player.PlayerName));
        }
        
        // Start RTV process
        RtvUtils.StartRtv();
    }
}
