using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using Mappen.Classes;
using Mappen.Variables;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Mappen.Utils
{
    public static class RtvUtils
    {
        public static Mappen Instance => Mappen.Instance;

        public static bool CanPlayerRtv(CCSPlayerController player)
        {
            if (!PlayerUtils.IsValidPlayer(player))
                return false;

            if (!Instance.Config.RtvEnable)
                return false;

            // Check if RTV delay has passed
            float currentTime = (float)GlobalVariables.Timers.Elapsed.TotalSeconds;
            if (currentTime - GlobalVariables.MapStartTime < Instance.Config.RtvDelay)
                return false;

            // Check if player has already RTVed
            if (GlobalVariables.RtvPlayers.Contains(player.SteamID.ToString()))
                return false;

            return true;
        }

        public static void AddPlayerRtv(CCSPlayerController player)
        {
            if (!CanPlayerRtv(player))
                return;

            GlobalVariables.RtvPlayers.Add(player.SteamID.ToString());
            
            // Calculate if RTV threshold has been reached
            int totalPlayers = Utilities.GetPlayers().Count(p => PlayerUtils.IsValidPlayer(p));
            int rtvCount = GlobalVariables.RtvPlayers.Count;
            int requiredPlayers = (int)Math.Ceiling(totalPlayers * (Instance.Config.RtvPlayerPercentage / 100.0));
            
            // Notify all players
            var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
            foreach (var p in players)
            {
                p.PrintToChat(Instance.Localizer.ForPlayer(p, "rtv.player.added")
                    .Replace("{PLAYER_NAME}", player.PlayerName)
                    .Replace("{CURRENT}", rtvCount.ToString())
                    .Replace("{REQUIRED}", requiredPlayers.ToString()));
            }

            // Check if RTV threshold has been reached
            if (rtvCount >= requiredPlayers)
            {
                StartRtv();
            }
        }

        /// <summary>
        /// Notifies all players that RTV has been triggered.
        /// </summary>
        private static void NotifyRtvStarted()
        {
            var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
            foreach (var player in players)
            {
                player.PrintToChat(Instance.Localizer.ForPlayer(player, "rtv.started"));
            }

            Instance.Logger.LogInformation("RTV vote has been triggered");
        }

        /// <summary>
        /// Handles the case when we should respect the already set nextmap.
        /// </summary>
        /// <returns>True if the nextmap was respected and used, false otherwise.</returns>
        private static bool HandleRespectNextmap()
        {
            // Only respect nextmap if it was explicitly set (not auto-set at map start)
            if (Instance.Config.RtvRespectNextmap && GlobalVariables.NextMap != null)
            {
                // Use the already set nextmap without voting
                Instance.Logger.LogInformation("Using already set nextmap: {MapName}", GlobalVariables.NextMap.MapValue);
                
                // Notify players
                var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
                foreach (var player in players)
                {
                    player.PrintToChat(Instance.Localizer.ForPlayer(player, "rtv.using.nextmap").Replace("{MAP_NAME}", GlobalVariables.NextMap.MapValue));
                }

                // Change map based on the setting
                if (Instance.Config.RtvChangeInstantly)
                {
                    ChangeMapImmediately(GlobalVariables.NextMap);
                }
                else
                {
                    // Set a flag to change map at the end of the round
                    GlobalVariables.RtvTriggered = true;
                    Instance.Logger.LogInformation("Map will change at the end of the round");
                }
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Shows the vote menu to all players.
        /// </summary>
        private static void ShowRtvVoteMenu()
        {
            var players = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p));
            foreach (var player in players)
            {
                try
                {
                    string? soundToPlay = "";
                    if (Instance.Config.Sounds.Count > 0)
                    {
                        soundToPlay = Instance.Config.Sounds[new Random().Next(Instance.Config.Sounds.Count)];
                    }

                    if (!string.IsNullOrEmpty(soundToPlay))
                    {
                        player.ExecuteClientCommand($"play {soundToPlay}");
                    }

                    MenuUtils.ShowKitsuneMenuVoteMaps(player);
                }
                catch (Exception ex)
                {
                    Instance.Logger.LogError(ex, "Error showing vote menu to player {PlayerName}", player.PlayerName);
                }
            }
        }

        /// <summary>
        /// Processes the RTV vote results.
        /// </summary>
        private static void ProcessRtvVoteResults()
        {
            try
            {
                var (winningMap, type) = MapUtils.GetWinningMap();

                if (winningMap != null)
                {
                    GlobalVariables.NextMap = winningMap;
                    Instance.Logger.LogInformation("Vote completed. Next map: {MapName}", winningMap.MapValue);
                }
                else if (winningMap == null && type == "extendmap")
                {
                    HandleExtendMapVote();
                }
                else if (winningMap == null && type == "ignorevote")
                {
                    GlobalVariables.NextMap = MapUtils.GetRandomNextMapByPlayers();
                    Instance.Logger.LogInformation("Winning map is Ignore Vote. Next map is {NEXTMAP}", GlobalVariables.NextMap?.MapValue);
                }
                else if (winningMap == null && type == "tie")
                {
                    // This case is handled in GetWinningMap
                    Instance.Logger.LogInformation("Vote ended in a tie. Random map was selected.");
                }
                else if (winningMap == null && type == "error")
                {
                    GlobalVariables.NextMap = MapUtils.GetRandomNextMapByPlayers();
                    Instance.Logger.LogWarning("Error during vote processing. Selecting random map: {MapName}", GlobalVariables.NextMap?.MapValue);
                }
                else
                {
                    GlobalVariables.NextMap = MapUtils.GetRandomNextMapByPlayers();
                    Instance.Logger.LogInformation("No winning map determined. Selecting random map: {MapName}", GlobalVariables.NextMap?.MapValue);
                }

                // Clean up
                GlobalVariables.Votes.Clear();
                GlobalVariables.MapForVotes.Clear();
                GlobalVariables.VoteStarted = false;
                Instance.AddTimer(1.0f, () => GlobalVariables.IsVotingInProgress = false);

                if (type != "extendmap")
                {
                    GlobalVariables.VotedForCurrentMap = true;
                }

                NotifyPlayersAboutVoteResults(type);

                // If not extending the map, change map based on settings
                if (type != "extendmap")
                {
                    if (Instance.Config.RtvChangeInstantly)
                    {
                        ChangeMapImmediately(GlobalVariables.NextMap);
                    }
                    else
                    {
                        // Set a flag to change map at the end of the round
                        GlobalVariables.RtvTriggered = true;
                        Instance.Logger.LogInformation("Map will change at the end of the round");
                    }
                }
            }
            catch (Exception ex)
            {
                Instance.Logger.LogError(ex, "Error processing vote results");
                
                // Ensure we clean up even if there's an error
                GlobalVariables.Votes.Clear();
                GlobalVariables.MapForVotes.Clear();
                GlobalVariables.VoteStarted = false;
                GlobalVariables.IsVotingInProgress = false;
                GlobalVariables.VotedForCurrentMap = true;
                GlobalVariables.RtvTriggered = false;
            }
        }

        /// <summary>
        /// Handles the extend map vote option.
        /// </summary>
        private static void HandleExtendMapVote()
        {
            var gameRules = ServerUtils.GetGameRules();
            float timeLeft;
            
            if (Instance.Config.DependsOnTheRound == true && gameRules != null)
            {
                float maxLimit = (float)(ConVar.Find("mp_maxrounds")?.GetPrimitiveValue<int>() ?? 0);
                timeLeft = maxLimit - gameRules.TotalRoundsPlayed;
                
                int extendTime = Instance.Config.ExtendMapTime;
                Server.ExecuteCommand($"mp_maxrounds {(int)timeLeft + extendTime}");
                Instance.Logger.LogInformation("Vote to extend map. Setting mp_maxrounds to {Rounds}", (int)timeLeft + extendTime);
            }
            else
            {
                timeLeft = GlobalVariables.TimeLeft - GlobalVariables.CurrentTime;
                
                int extendTime = Instance.Config.ExtendMapTime;
                Server.ExecuteCommand($"mp_timelimit {Math.Ceiling((float)timeLeft / 60) + extendTime}");
                Instance.Logger.LogInformation("Vote to extend map. Setting mp_timelimit to {Minutes}", Math.Ceiling((float)timeLeft / 60) + extendTime);
            }
            
            GlobalVariables.VotedForExtendMap = true;
            GlobalVariables.VotedForCurrentMap = false;
            GlobalVariables.RtvTriggered = false;
        }

        /// <summary>
        /// Notifies all players about the vote results.
        /// </summary>
        /// <param name="type">The type of vote result</param>
        private static void NotifyPlayersAboutVoteResults(string type)
        {
            var activePlayers = Utilities.GetPlayers().Where(p => PlayerUtils.IsValidPlayer(p)).ToList();
            foreach (var player in activePlayers)
            {
                try
                {
                    if (type == "extendmap")
                    {
                        player.PrintToChat(Instance.Localizer.ForPlayer(player, "vote.finished.extend.map.round").Replace("{EXTENDED_TIME}", Instance.Config.ExtendMapTime.ToString()));
                    }
                    else
                    {
                        player.PrintToChat(Instance.Localizer.ForPlayer(player, "vote.finished").Replace("{MAP_NAME}", GlobalVariables.NextMap?.MapValue));
                    }

                    GlobalVariables.KitsuneMenu?.ClearMenus(player);
                }
                catch (Exception ex)
                {
                    Instance.Logger.LogError(ex, "Error notifying player {PlayerName} about vote results", player.PlayerName);
                }
            }
        }

        /// <summary>
        /// Starts the RTV process, which either changes to the next map or initiates a vote.
        /// </summary>
        public static void StartRtv()
        {
            if (GlobalVariables.IsVotingInProgress || GlobalVariables.VoteStarted)
                return;

            NotifyRtvStarted();

            // Check if we should respect the already set nextmap
            if (HandleRespectNextmap())
            {
                return;
            }

            // Start map vote
            MapUtils.PopulateMapsForVotes();
            GlobalVariables.VoteStarted = true;
            GlobalVariables.IsVotingInProgress = true;
            
            // RTVの目的はマップを変更することなので、マップ延長オプションは表示しない
            // VotedForExtendMapフラグを使用してマップ延長オプションを非表示にする
            GlobalVariables.VotedForExtendMap = true;
            
            // 投票処理後にフラグをリセットするタイマーを設定
            Instance.AddTimer(Instance.Config.VoteMapDuration + 1.0f, () => {
                // 投票が終了した後、フラグをリセットする（次回の通常投票では延長オプションを表示可能にする）
                if (!GlobalVariables.IsVotingInProgress) {
                    GlobalVariables.VotedForExtendMap = false;
                }
            }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.STOP_ON_MAPCHANGE);

            // Show vote menu to all players
            ShowRtvVoteMenu();

            // Set timer for vote duration
            float duration = (float)(Instance.Config.VoteMapDuration);
            Instance.Logger.LogInformation("Vote duration set to {Duration} seconds", duration);

            Instance.AddTimer(duration, ProcessRtvVoteResults, CounterStrikeSharp.API.Modules.Timers.TimerFlags.STOP_ON_MAPCHANGE);
        }

        private static void ChangeMapImmediately(Map? nextMap)
        {
            // Use the common map change utility method
            MapUtils.ChangeMap(nextMap);
        }

        public static void ResetRtv()
        {
            GlobalVariables.RtvPlayers.Clear();
            GlobalVariables.RtvEnabled = false;
        }
    }
}