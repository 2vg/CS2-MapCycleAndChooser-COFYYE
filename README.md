# Mappen

## 📌 Overview  
Mappen is a map cycle management plugin on the CS2 server, based on [cofyye/CS2-MapCycleAndChooser-COFYYE](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE) with a lot of modifications and improvements.

The idea behind Mappen is to reduce the complexity of management.

It is implemented to automatically create any configuration files and automatically migrate old settings.

## What "Mappen" meaning

The original plugin author's(cofyye) icon was a penguin, so I just combined `Map` and `Pen`guin.

I also prefer short words in project names.

## Features

- **Next Map Configuration & Display**: Set the next map in the cycle and display it for players.
- **Map Voting**: Players can vote for a new map towards the end of the current map.
- **Real-time Voting Percentages**: View the percentage of votes for each map in real time.
- **Admin Map List**: Admins can access a list of available maps and instantly change the current map.
- **Dynamic Map Cycle**: Set which maps are part of the cycle and which ones can be changed from the map list.
- **Dynamic Map Cycle Time**: Set whether each map is selected as a cycle based on time.
- **Dynamic Map Selection**: Maps are selected based on the current number of players, with options for max and min player thresholds.
- **Custom Map Display**: Show custom names instead of map values (e.g., showing "Dust II" instead of "de_dust2").
- **Round/Timelimit-Based Map Changes**: Change maps based on the number of rounds or time limits.
- **Vote Start Sound**: Play a sound when the voting for a new map begins.
- **Ignore Option**: Players can now choose to ignore the current vote.
- **Extend Map Option**: Allows players to vote to extend the current map instead of switching to a new one.
- **Timeleft Command**: Displays the remaining time before the map changes.
- **Current Map Command**: Shows the name of the map currently being played.
- **Last Map Command**: Displays the previously played map.
- **Map Change Delay**: Configurable delay after the current map ends before the next map loads.
- **Vote Trigger Time**: Set how long before the end of the map the voting process should start.
- **Command Aliases**: Each command can have multiple aliases for easier accessibility.
- **Rock The Vote**: Allows players to vote to change the map before the end of the current map.
- **Map Cooldowns**: Prevents the same maps from being played too frequently by setting a cooldown period in map cycles.
- **Individual Map Configuration Files**: Each map now has its own configuration file for easier management.
- **Map Nomination**: Allows players to nominate maps to be included in the next map vote.
- **Workshop Map Mapping System**: Automatically maps Workshop map titles to their actual in-game names to prevent duplicate configurations.

## Dependencies

To run this plugin, you need the following dependencies:

1. **Metamod:Source (2.x)**  
   Download from: [Metamod:Source Official Site](https://www.sourcemm.net/downloads.php/?branch=master)

2. **CounterStrikeSharp**  
   You can get it from: [CounterStrikeSharp GitHub Releases](https://github.com/roflmuffin/CounterStrikeSharp/releases)

3. **MultiAddonManager** *(optional)*  
   Download from: [MultiAddonManager GitHub Releases](https://github.com/Source2ZE/MultiAddonManager/releases)  

   **※ ↓ This settngs will be changed in the future. ↓**
   - If you want to play a sound when map voting begins, this dependency is required. You can use your own custom sounds, though no tutorial is provided. Search online for guidance.  
   - Alternatively, if you'd like to use pre-configured sounds, visit this link: [Steam Workshop Sounds](https://steamcommunity.com/sharedfiles/filedetails/?id=3420306144).  

   **Setup for Sounds:**
   - To enable the sounds, you must add the `3420306144` ID to the `multiaddonmanager.cfg` file.  
   - Path to file: `game/csgo/cfg/multiaddonmanager/multiaddonmanager.cfg`.  
   - Add the ID under the `mm_extra_addons` section, for example:  
     ```json
     "....,3420306144"
     ```
## Commands and Permissions

### Console Commands
These commands can be used in the console and are registered using attributes:

1. **`css_setnextmap`**
   - **Description**: Sets the next map in the rotation.
   - **Usage**: `css_setnextmap <map>`
   - **Access**: Admins only, requires `@css/changemap` permission.

2. **`css_maps`**
   - **Description**: Lists all maps and allows instant map changes.
   - **Access**: Admins only, requires `@css/changemap` permission.

3. **`css_nominate`** or **`css_nom`**
   - **Description**: Console command to nominate a map.
   - **Usage**: `css_nominate <map>` or just `css_nominate` to open a menu.
   - **Access**: Available to all players.

4. **`css_force_nominate`** or **`css_force_nom`**
   - **Description**: Force nominate a map for voting (bypasses restrictions).
   - **Usage**: `css_force_nominate <map>`
   - **Access**: Admins only, requires `@css/changemap` permission.

5. **`css_nomlist`**
   - **Description**: Console command to show current map nominations.
   - **Access**: Available to all players.

6. **`css_rtv`**
   - **Description**: Console command to rock the vote.
   - **Access**: Available to all players.

7. **`css_nextmap`**
   - **Description**: Console command to show the next map.
   - **Access**: Available to all players.

8. **`css_currentmap`**
   - **Description**: Console command to show the current map.
   - **Access**: Available to all players.

9. **`css_timeleft`**
   - **Description**: Console command to show time left on the current map.
   - **Access**: Available to all players.

10. **`css_lastmap`**
    - **Description**: Console command to show the last map played.
    - **Access**: Available to all players.

## What's New in v2.0

Version 2.0 includes several significant improvements:

### Command Registration System Refactoring

The command registration system has been refactored to use attributes instead of manual registration:

1. **Attribute-Based Command Registration**:
   - Commands are now registered using attributes like `[ConsoleCommand]`, `[CommandHelper]`, and `[RequiresPermissions]`.
   - This makes the code more maintainable and follows CounterStrikeSharp's recommended practices.
   - All console commands (`css_*`) are now registered using this method.

2. **Expanded Console Commands**:
   - Added more console commands for better functionality.
   - Each chat command now has a corresponding console command.

Previously, chat was handled manually, and as the number of commands increased, maintainability became worse.

By unifying the implementation, we will make it easier to maintain.

### Configuration System Improvements

The configuration system has been significantly enhanced for better organization and management.

## Configuration Tutorial

Below is a step-by-step guide explaining the available configuration options for **Mappen**. These options allow you to customize how the plugin behaves and interacts with players.

### Configuration File Structure Changes

In version 2.0, the configuration system has been significantly improved:

1. **Individual Map Configuration Files**:
   - Each map now has its own configuration file located in `game/csgo/addons/counterstrikesharp/configs/plugins/Mappen/maps/`.
   - Files are named after the map (e.g., `de_dust2.json`).
   - This makes it easier to manage settings for individual maps without editing a large configuration file.

2. **Default Map Template**:
   - A default map configuration template is stored at `game/csgo/addons/counterstrikesharp/configs/plugins/Mappen/default_map_config.json`.
   - This template is used when creating configuration files for new maps.
   - default template:
     ```jsonc
      {
          mapValue: "default",
          mapDisplay: "Default Map",
          mapIsWorkshop: false,
          mapWorkshopId: "",
          mapCycleEnabled: true,
          mapCanVote: true,
          mapMinPlayers: 0,
          mapMaxPlayers: 64,
          mapCycleStartTime: "",
          mapCycleEndTime: "",
          mapCooldownCycles: 10
      }
     ```

3. **Cooldown System**:
   - Map cooldowns are stored in `game/csgo/addons/counterstrikesharp/configs/plugins/Mappen/cooldowns.json`.

4. **Automatic Migration**:
   - When upgrading from v1.1, your existing map configurations will be automatically migrated to individual files.

### General Settings

## Configuration Options  

1. **`vote_map_enable`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Enables or disables the voting system for a new map.  
     - `true`: Voting is enabled.  
     - `false`: Voting is disabled.  

2. **`vote_map_duration`**  
   - **Possible Values**: Integer values between `1` and `45` (e.g., `15`, `30`, etc.)  
   - **Description**: Specifies the duration (in seconds) for the map voting period. Must be greater than `0` and less than `45`, otherwise an error will occur.  

3. **`vote_map_on_freezetime`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Controls whether voting starts during freeze time.  
     - `true`: Increases the freeze time in the next round and starts voting during it.  
     - `false`: Voting starts at the beginning of the next round but does not extend freeze time. Players can vote while the round progresses.  

4. **`prioritize_rounds`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Determines whether map voting is based on rounds or time.  
     - `true`: The plugin uses `mp_maxrounds` to trigger voting.  
     - `false`: The plugin uses `mp_timelimit` to trigger voting.  

5. **`enable_player_freeze_in_menu`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Freezes players when the map list menu or voting menu is active.  
     - `true`: Players cannot move until the menu closes.  
     - `false`: Players can move even while voting.  
     - *Note*: For the best experience, set both `vote_map_on_freezetime` and this option to `true`. Otherwise, players may remain frozen after the round starts if only this option is enabled.  

6. **`enable_player_voting_in_chat`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Logs in the chat which player voted for which map.  
     - `true`: Displays voting logs in the chat.  
     - `false`: Disables voting logs.  

7. **`display_map_by_value`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Defines how maps are displayed.  
     - `true`: Displays the map by its technical name (e.g., `de_dust2`).  
     - `false`: Displays the map by its custom tag (e.g., `Dust II`).  

8. **`enable_random_nextmap`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Determines if the next map is selected randomly or cyclically.  
     - `true`: The next map will be chosen randomly.  
     - `false`: The next map will follow a cyclic order.  

9. **`enable_nextmap_command`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Enables or disables the `!nextmap` command.  

10. **`enable_lastmap_command`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables the `!lastmap` command.  

11. **`enable_currentmap_command`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables the `!currentmap` command.  

12. **`enable_timeleft_command`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables the `!timeleft` command.  

13. **`enable_command_ads_in_chat`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Displays command advertisements in chat every 5 minutes if enabled.  

14. **`enable_ignore_vote`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Adds an "Ignore Vote" option to the map voting menu.  

15. **`ignore_vote_position`**  
    - **Possible Values**: `"top"`, `"bottom"`  
    - **Description**: Defines whether the "Ignore Vote" option appears at the top or bottom of the voting menu.  

16. **`enable_extend_map`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Adds an "Extend Map" option to the map voting menu.  

17. **`extend_map_time`**  
    - **Possible Values**: Integer values greater than `0`  
    - **Description**: Defines how much time the map will be extended.  
      - If `prioritize_rounds` is `true`, the value represents rounds.  
      - If `prioritize_rounds` is `false`, the value represents minutes.  

18. **`extend_map_max_times`**  
    - **Possible Values**: Integer values greater than `0`  
    - **Description**: Defines the maximum number of times a map can be extended through voting.  
      - Default is 1, meaning maps can only be extended once.
      - This is a global setting that applies to all maps unless overridden by map-specific settings.

19. **`extend_map_position`**  
    - **Possible Values**: `"top"`, `"bottom"`  
    - **Description**: Defines whether the "Extend Map" option appears at the top or bottom of the voting menu.  

20. **`delay_to_change_map_in_the_end`**  
    - **Possible Values**: Integer values greater than `5`  
    - **Description**: Defines the delay (in seconds) between the end of the current map and the actual map change.  

21. **`vote_trigger_time_before_map_end`**  
    - **Possible Values**: Integer values greater than `2`  
    - **Description**: Defines how long before the end of the current map the vote is triggered.  
      - If `prioritize_rounds` is `true`, the value is in rounds.  
      - If `prioritize_rounds` is `false`, the value is in minutes.  

22. **`rtv_enable`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables the Rock The Vote (RTV) functionality.  
      - `true`: RTV is enabled.  
      - `false`: RTV is disabled.  

23. **`rtv_delay`**  
    - **Possible Values**: Integer values greater than `0`  
    - **Description**: Defines the delay (in seconds) after map start before players can use RTV.  

24. **`rtv_player_percentage`**  
    - **Possible Values**: Integer values between `1` and `100`  
    - **Description**: Defines the percentage of players needed to trigger an RTV vote.  

25. **`rtv_change_instantly`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Determines if the map changes immediately when RTV is triggered.  
      - `true`: Map changes immediately.  
      - `false`: Map changes at the end of the round.  

26. **`rtv_respect_nextmap`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Determines if RTV should use the already set nextmap.  
      - `true`: Uses the already set nextmap when RTV is triggered.  
      - `false`: Starts a new vote when RTV is triggered.  

27. **`enable_map_cooldown`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables map cooldowns to prevent the same maps from being played too frequently.  
      - When enabled, maps that were recently played will be excluded from the map cycle and voting until their cooldown expires.

28. **`sounds`**
   - **Possible Values**: An array of string paths to sound files.
   - **Description**: Specifies the sounds that play when map voting begins.
     - Add as many sounds as you'd like, and the plugin will play one randomly.
     - Leave this field empty (`[]`) to disable sounds.

29. **`enable_workshop_collection_sync`**
   - **Possible Values**: `true`, `false`
   - **Description**: Enables or disables automatic synchronization of maps from Steam Workshop collections.
     - When enabled, the plugin will fetch maps from the specified Workshop collections and add them to your map pool.

30. **`workshop_collection_ids`**
   - **Possible Values**: List of strings (Workshop collection IDs)
   - **Description**: List of Steam Workshop collection IDs to synchronize maps from.
     - Example: `["123456789", "987654321"]`
     - Leave this field empty (`[]`) to disable sounds.

### Map Configuration Options

Each map now has its own configuration file with the following options:

1. **`map_value`**  
   - **Description**: The technical name of the map (e.g., `de_dust2`).

2. **`map_display`**  
   - **Description**: The custom display name for the map (e.g., `Dust II`).

3. **`map_is_workshop`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Indicates if the map is from the Steam Workshop.

4. **`map_workshop_id`**  
   - **Description**: The Workshop ID of the map (required if `map_is_workshop` is `true`, otherwise set to `""`).

5. **`map_cycle_enabled`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Determines if the map should be included in the map cycle.
     - `true`: The map will be included in the automatic map rotation.
     - `false`: The map will only appear in the admin map list (`css_maps`).

6. **`map_can_vote`**  
   - **Possible Values**: `true`, `false`  
   - **Description**: Determines if the map should appear in the voting system.

7. **`map_min_players`**  
   - **Description**: Minimum number of players required for the map to be included in voting.

8. **`map_max_players`**  
   - **Description**: Maximum number of players allowed for the map to be included in voting.

9. **`map_cycle_start_time`**  
   - **Format**: `"HH:mm"` (24-hour format)
   - **Description**: Start time for map cycle. The map will only be available in the cycle between the start and end times.
   - **Note**: Ignored if both start and end fields are empty.

10. **`map_cycle_end_time`**  
    - **Format**: `"HH:mm"` (24-hour format)
    - **Description**: End time for map cycle. The map will only be available in the cycle between the start and end times.
    - **Note**: Ignored if both start and end fields are empty.

11. **`map_cooldown_cycles`**  
    - **Possible Values**: Integer values (0 or greater)
    - **Description**: The number of map cycles that must pass before this map can be played again.
    - **Example**: If set to 2, the map will be unavailable for 2 map changes after it's played.
    - **Note**: Only applies when `enable_map_cooldown` is set to `true` in the main configuration.

12. **`map_max_rounds`**  
    - **Possible Values**: Integer values (null or greater than 0)
    - **Description**: The maximum number of rounds this specific map. If want rounds-based, `map_time_limit` should be `null`.
    - **Example**: If set to 3, players can play 3 rounds.
    - **Note**: If not specified (null), the global `extend_map_time` setting will be used instead.

13. **`map_time_limit`**  
    - **Possible Values**: Integer values (null or greater than 0)
    - **Description**: The maximum timelimit of rounds this specific map. If want timelimit-based, `map_max_rounds` should be `null`.
    - **Example**: If set to 10, players can play 10 minutes.
    - **Note**: If not specified (null), the global `extend_map_time` setting will be used instead.

14. **`map_max_extends`**  
    - **Possible Values**: Integer values (null or greater than 0)
    - **Description**: The maximum number of times this specific map can be extended through voting.
    - **Example**: If set to 3, players can vote to extend this map up to 3 times before the extend option is removed.
    - **Note**: If not specified (null), the global `extend_map_max_times` setting will be used instead.

### Workshop Map Mapping System

The plugin now includes a sophisticated mapping system for Workshop maps that solves the problem of duplicate configurations caused by mismatches between Workshop titles and actual in-game map names.

More detail is here: [duplicate_config_handling.md](duplicate_config_handling.md)

#### How It Works

1. **Automatic Detection**: When a Workshop map is loaded, the plugin automatically detects its Workshop ID and the actual in-game map name.

2. **Mapping Storage**: This relationship is stored in a mapping file located at:
  > game/csgo/addons/counterstrikesharp/configs/plugins/Mappen/workshop_map_mapping.json

3. **Configuration Consolidation**: If duplicate configurations exist (created from Workshop titles that differ from actual map names), the plugin automatically merges them into a single configuration using the official map name.

4. **Workshop Collection Sync**: When synchronizing maps from Workshop collections, the plugin uses this mapping to ensure consistent configuration names.

#### Benefits

- **Prevents Duplicate Configurations**: No more duplicate config files for the same map.
- **Consistent Map Names**: Uses the actual in-game map names for configurations.
- **Automatic Management**: No manual intervention required - the system handles everything automatically.
- **Preserves Settings**: When merging configurations, all settings from the original config are preserved.

### Installation

※ There is no release yet.

1. Download the **[Mappen v2.0](https://github.com/2vg/Mappen/releases/download/2.0/Mappen-v2.0.zip)** plugin as a `.zip` file.  
2. Upload the contents of the `.zip` file into the following directory on your server:  
   > game/csgo/addons/counterstrikesharp/plugins  
3. After uploading, change the map or restart your server to activate the plugin.  
4. The configuration files will be generated at:  
   > game/csgo/addons/counterstrikesharp/configs/plugins/Mappen/  
   Adjust all settings in these files as needed.

### Language Support
The language files are located in the following directory:
> game/csgo/addons/counterstrikesharp/plugins/Mappen/lang

Currently, there are three language files:
- `en.json` (English)
- `sr.json` (Serbian)
- `pl.json` (Polish)

## Bug Reports & Suggestions

If you encounter any bugs or issues while using the plugin, please report them on the [GitHub Issues page](https://github.com/2vg/Mappen/issues). Provide a detailed description of the problem, and I will work on resolving it as soon as possible.

Feel free to submit any suggestions for improvements or new features you'd like to see in future releases. Your feedback is highly appreciated!

## Important Notes

- **ScreenMenuAPI Not Included**:
  The **ScreenMenuAPI** is not included in this version of the plugin due to necessary adjustments required for full compatibility. Additional refinements are needed to ensure seamless functionality with this plugin.

  Please be patient, and expect an update in the near future that will introduce **ScreenMenuAPI**! 🚀

## Credits

- **COFYYE**: Thanks to COFYYE, the original plugin creator.

- **Code Snippets for Menu**: The menu code snippets were sourced from [oqyh's GitHub](https://github.com/oqyh). I would like to thank him for providing valuable resources that helped in building parts of this plugin.
  
- **Other Contributors**: A big thank you to all other authors and contributors of similar plugins that inspired the creation of this MapCycle and Chooser plugin. Their work was a key part of shaping the final version of this plugin.

This plugin is my version of the MapCycle and Chooser functionality, combining various elements from the community to provide a better and more customizable experience for server admins and players alike.
