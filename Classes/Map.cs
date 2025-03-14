using System.Text.Json.Serialization;

namespace Mappen.Classes
{
    public class Map(string mapValue, string mapDisplay, bool mapIsWorkshop, string mapWorkshopId, bool mapCycleEnabled, bool mapCanVote, int mapMinPlayers, int mapMaxPlayers, string mapCycleStartTime, string mapCycleEndTime, int mapCooldownCycles = 0, int? mapMaxRounds = null, float? mapTimeLimit = null, int? mapMaxExtends = null)
    {
        [JsonPropertyName("map_value")]
        public string MapValue { get; init; } = mapValue;

        [JsonPropertyName("map_display")]
        public string MapDisplay { get; init; } = mapDisplay;

        [JsonPropertyName("map_is_workshop")]
        public bool MapIsWorkshop { get; init; } = mapIsWorkshop;

        [JsonPropertyName("map_workshop_id")]
        public string MapWorkshopId { get; init; } = mapWorkshopId;

        [JsonPropertyName("map_cycle_enabled")]
        public bool MapCycleEnabled { get; init; } = mapCycleEnabled;

        [JsonPropertyName("map_can_vote")]
        public bool MapCanVote { get; init; } = mapCanVote;

        [JsonPropertyName("map_min_players")]
        public int MapMinPlayers { get; init; } = mapMinPlayers;

        [JsonPropertyName("map_max_players")]
        public int MapMaxPlayers { get; init; } = mapMaxPlayers;

        [JsonPropertyName("map_cycle_start_time")]
        public string MapCycleStartTime { get; init; } = mapCycleStartTime;

        [JsonPropertyName("map_cycle_end_time")]
        public string MapCycleEndTime { get; init; } = mapCycleEndTime;

        [JsonPropertyName("map_cooldown_cycles")]
        public int MapCooldownCycles { get; init; } = mapCooldownCycles;

        [JsonPropertyName("map_max_rounds")]
        public int? MapMaxRounds { get; init; } = mapMaxRounds;

        [JsonPropertyName("map_time_limit")]
        public float? MapTimeLimit { get; init; } = mapTimeLimit;

        [JsonPropertyName("map_max_extends")]
        public int? MapMaxExtends { get; init; } = mapMaxExtends;
    }
}