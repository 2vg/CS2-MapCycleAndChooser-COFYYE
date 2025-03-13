﻿﻿﻿﻿﻿using CounterStrikeSharp.API.Core;
using MapCycleAndChooser_COFYYE.Classes;
using System.Text.Json.Serialization;

namespace MapCycleAndChooser_COFYYE.Config
{
    public class Config : BasePluginConfig
    {
        [JsonPropertyName("ConfigVersion")]
        public override int Version { get; set; } = VersionInfo.CurrentConfigVersion; // Config file version
        [JsonPropertyName("rtv_enable")]
        public bool RtvEnable { get; init; } = true;

        [JsonPropertyName("rtv_delay")]
        public int RtvDelay { get; init; } = 60; // seconds after map start

        [JsonPropertyName("rtv_player_percentage")]
        public int RtvPlayerPercentage { get; init; } = 60; // percentage of players needed to trigger RTV

        [JsonPropertyName("rtv_change_instantly")]
        public bool RtvChangeInstantly { get; init; } = false; // if true, changes map immediately, otherwise waits for round end

        [JsonPropertyName("rtv_respect_nextmap")]
        public bool RtvRespectNextmap { get; init; } = true; // if true, uses the already set nextmap when RTV is triggered

        [JsonPropertyName("commands_rtv")]
        public List<string> CommandsRtv { get; init; } =
        [
            "!rtv",
            "!rockthevote"
        ];

        [JsonPropertyName("vote_map_enable")]
        public bool VoteMapEnable { get; init; } = true;

        [JsonPropertyName("vote_map_duration")]
        public int VoteMapDuration { get; init; } = 15;

        [JsonPropertyName("vote_map_on_freezetime")]
        public bool VoteMapOnFreezeTime { get; init; } = true;

        [JsonPropertyName("depends_on_the_round")]
        public bool DependsOnTheRound { get; init; } = true;

        //[JsonPropertyName("enable_screen_menu")]
        //public bool EnableScreenMenu { get; init; } = true;

        [JsonPropertyName("enable_random_nextmap")]
        public bool EnableRandomNextMap { get; init; } = false;

        [JsonPropertyName("enable_player_freeze_in_menu")]
        public bool EnablePlayerFreezeInMenu { get; init; } = true;

        [JsonPropertyName("enable_player_voting_in_chat")]
        public bool EnablePlayerVotingInChat { get; init; } = true;

        [JsonPropertyName("enable_nextmap_command")]
        public bool EnableNextMapCommand { get; init; } = true;

        [JsonPropertyName("enable_lastmap_command")]
        public bool EnableLastMapCommand { get; init; } = true;

        [JsonPropertyName("enable_currentmap_command")]
        public bool EnableCurrentMapCommand { get; init; } = true;

        [JsonPropertyName("enable_timeleft_command")]
        public bool EnableTimeLeftCommand { get; init; } = true;

        [JsonPropertyName("enable_command_ads_in_chat")]
        public bool EnableCommandAdsInChat { get; init; } = true;

        [JsonPropertyName("enable_ignore_vote")]
        public bool EnableIgnoreVote { get; init; } = true;

        [JsonPropertyName("ignore_vote_position")]
        public string IgnoreVotePosition { get; init; } = "top";

        [JsonPropertyName("enable_extend_map")]
        public bool EnableExtendMap { get; init; } = true;

        [JsonPropertyName("extend_map_time")]
        public int ExtendMapTime { get; init; } = 8; // rounds or minutes if depends on round is false

        [JsonPropertyName("extend_map_position")]
        public string ExtendMapPosition { get; init; } = "bottom";

        [JsonPropertyName("delay_to_change_map_in_the_end")]
        public int DelayToChangeMapInTheEnd { get; init; } = 10; // in seconds

        [JsonPropertyName("vote_trigger_time_before_map_end")]
        public int VoteTriggerTimeBeforeMapEnd { get; init; } = 3; // rounds or minutes if depends on round is false

        [JsonPropertyName("display_map_by_value")]
        public bool DisplayMapByValue { get; init; } = true;

        [JsonPropertyName("vote_map_count")]
        public int VoteMapCount { get; init; } = 5;

        [JsonPropertyName("commands_css_nextmap")]
        public List<string> CommandsCSSNextmap { get; init; } =
        [
            "css_nextmap",
            "css_sledecamapa"
        ];

        [JsonPropertyName("commands_css_maps")]
        public List<string> CommandsCSSMaps { get; init; } =
        [
            "css_maps",
            "css_mape"
        ];

        [JsonPropertyName("commands_css_nominate")]
        public List<string> CommandsCSSNominate { get; init; } =
        [
            "css_nominate",
            "css_nom"
        ];

        [JsonPropertyName("commands_css_nominations")]
        public List<string> CommandsCSSNominations { get; init; } =
        [
            "css_nominations",
            "css_noms"
        ];

        [JsonPropertyName("commands_nextmap")]
        public List<string> CommandsNextMap { get; init; } =
        [
            "!nextmap",
            "!sledecamapa"
        ];

        [JsonPropertyName("commands_lastmap")]
        public List<string> CommandsLastMap { get; init; } =
        [
            "!lastmap",
            "!proslamapa"
        ];

        [JsonPropertyName("commands_currentmap")]
        public List<string> CommandsCurrentMap { get; init; } =
        [
            "!currentmap",
            "!trenutnamapa"
        ];

        [JsonPropertyName("commands_timeleft")]
        public List<string> CommandsTimeLeft { get; init; } =
        [
            "!timeleft",
            "!preostalovreme"
        ];

        [JsonPropertyName("sounds")]
        public List<string> Sounds { get; init; } = 
        [
            "sounds/voice/gman_choose1.vsnd_c",
            "sounds/voice/gman_choose2.vsnd_c"
        ];

        [JsonPropertyName("enable_map_cooldown")]
        public bool EnableMapCooldown { get; init; } = true;

        [JsonPropertyName("enable_workshop_collection_sync")]
        public bool EnableWorkshopCollectionSync { get; init; } = false;

        [JsonPropertyName("workshop_collection_ids")]
        public List<string> WorkshopCollectionIds { get; init; } = [];

        [JsonPropertyName("maps")]
        public List<Map> Maps { get; init; } = [];
    }
}