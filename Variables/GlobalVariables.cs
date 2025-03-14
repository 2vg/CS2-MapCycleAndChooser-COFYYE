﻿﻿using Mappen.Classes;
using Menu;
using System.Diagnostics;

namespace Mappen.Variables
{
    public static class GlobalVariables
    {
        private static List<Map> _cycleMaps = [];
        private static List<Map> _maps = [];
        private static readonly List<Map> _mapForVotes = [];
        private static bool _voteStarted = false;
        private static bool _votedForCurrentMap = false;
        private static bool _votedForExtendMap = false;
        private static int _currentExtendCount = 0;
        private static bool _isVotingInProgress = false;
        private static Map? _nextmap = null;
        private static string _lastmap = "";
        private static float _timeleft = 0; // in seconds
        private static float _currentTime = 0; // in seconds
        private static int _messageIndex = 0;
        private static int _nextmapIndex = 0;
        private static readonly Dictionary<string, List<string>> _votes = [];
        private static int _freezeTime = 0;
        private static readonly Stopwatch _timers = new();
        private static CounterStrikeSharp.API.Modules.Timers.Timer? _timeleftTimer = null;
        private static CounterStrikeSharp.API.Modules.Timers.Timer? _votingTimer = null;
        private static KitsuneMenu _kitsuneMenu { get; set; } = null!;
        private static readonly HashSet<string> _rtvPlayers = [];
        private static float _mapStartTime = 0;
        private static bool _rtvEnabled = false;
        private static bool _rtvTriggered = false;
        private static readonly Dictionary<string, Map> _nominatedMaps = []; // Map name to Map object
        private static readonly Dictionary<string, string> _playerNominations = []; // Player SteamID to nominated map name

        public static List<Map> CycleMaps { get { return _cycleMaps; } set { _cycleMaps = value; } }
        public static List<Map> Maps { get { return _maps; } set { _maps = value; } }
        public static List<Map> MapForVotes { get { return _mapForVotes; } }
        public static bool VoteStarted { get { return _voteStarted; } set { _voteStarted = value; } }
        public static bool VotedForCurrentMap { get { return _votedForCurrentMap; } set { _votedForCurrentMap = value; } }
        public static bool VotedForExtendMap { get { return _votedForExtendMap; } set { _votedForExtendMap = value; } }
        public static int CurrentExtendCount { get { return _currentExtendCount; } set { _currentExtendCount = value; } }
        public static bool IsVotingInProgress { get { return _isVotingInProgress; } set { _isVotingInProgress = value; } }
        public static Map? NextMap { get { return _nextmap; } set { _nextmap = value; } }
        public static string LastMap { get { return _lastmap; } set { _lastmap = value; } }
        public static float TimeLeft { get { return _timeleft; } set { _timeleft = value; } }
        public static float CurrentTime { get { return _currentTime; } set { _currentTime = value; } }
        public static int MessageIndex { get { return _messageIndex; } set { _messageIndex = value; } }
        public static int NextMapIndex { get { return _nextmapIndex; } set { _nextmapIndex = value; } }
        public static Dictionary<string, List<string>> Votes { get { return _votes; } }
        public static int FreezeTime { get { return _freezeTime; } set { _freezeTime = value; } }
        public static Stopwatch Timers { get { return _timers; } }
        public static CounterStrikeSharp.API.Modules.Timers.Timer? TimeLeftTimer { get { return _timeleftTimer; } set { _timeleftTimer = value; } }
        public static CounterStrikeSharp.API.Modules.Timers.Timer? VotingTimer { get { return _votingTimer; } set { _votingTimer = value; } }
        public static KitsuneMenu KitsuneMenu { get { return _kitsuneMenu; } set { _kitsuneMenu = value; } }
        public static HashSet<string> RtvPlayers { get { return _rtvPlayers; } }
        public static float MapStartTime { get { return _mapStartTime; } set { _mapStartTime = value; } }
        public static bool RtvEnabled { get { return _rtvEnabled; } set { _rtvEnabled = value; } }
        public static bool RtvTriggered { get { return _rtvTriggered; } set { _rtvTriggered = value; } }
        public static Dictionary<string, Map> NominatedMaps { get { return _nominatedMaps; } }
        public static Dictionary<string, string> PlayerNominations { get { return _playerNominations; } }
    }
}
