# MapCycleAndChooser

## 📊 Plugin Statistics  

<p align="center">
  <img src="https://img.shields.io/github/downloads/cofyye/CS2-MapCycleAndChooser-COFYYE/total" alt="Total Downloads">
  <img src="https://img.shields.io/github/stars/cofyye/CS2-MapCycleAndChooser-COFYYE" alt="GitHub Stars">
  <img src="https://img.shields.io/github/last-commit/cofyye/CS2-MapCycleAndChooser-COFYYE" alt="Last Update">
  <img src="https://img.shields.io/github/issues/cofyye/CS2-MapCycleAndChooser-COFYYE" alt="Open Issues">
</p>

## 📌 Overview  
MapCycleAndChooser is a CS2 plugin designed to give server admins complete control over map rotations and player interactions. With this plugin, you can manage map cycles, enable map voting, and customize various settings to optimize gameplay for all players.

## See it in action
You can check out the plugin live on my server:  
**IP**: 198.186.131.31:27015  
Join and experience this plugin, along with all the other custom plugins I create for a unique gameplay experience.

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
- **Timeleft Command (`!timeleft`)**: Displays the remaining time before the map changes.
- **Current Map Command (`!currentmap`)**: Shows the name of the map currently being played.
- **Last Map Command (`!lastmap`)**: Displays the previously played map.
- **Map Change Delay**: Configurable delay after the current map ends before the next map loads.
- **Vote Trigger Time**: Set how long before the end of the map the voting process should start.
- **Command Aliases**: Each command can have multiple aliases for easier accessibility.
- **Rock The Vote (`!rtv`)**: Allows players to vote to change the map before the end of the current map.
- **Map Cooldowns**: Prevents the same maps from being played too frequently by setting a cooldown period in map cycles.
- **Individual Map Configuration Files**: Each map now has its own configuration file for easier management.
- **Map Nomination (`!nominate`)**: Allows players to nominate maps to be included in the next map vote.

## Screenshots

1. **`!nextmap` Command in Chat**: Displays the next map when the `!nextmap` command is typed in chat.

   ![Nextmap Command](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/blob/resources/nextmap.png?raw=true)

2. **Start of Map Voting**: Displays when a map vote begins.

   ![Map Voting Start](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/blob/resources/votemap1.png?raw=true)

3. **Voting Percentage After Map Selection**: Shows the percentage of votes for each map.

   ![Voting Percentage](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/blob/resources/votemap2.png?raw=true)

4. **Player Voting Logs**: Shows logs of which players voted for which maps and the next map.

   ![Player Voting Logs](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/blob/resources/votemap3.png?raw=true)

5. **Admin Map List**: Displays the map list that the admin can access and modify.

   ![Admin Map List](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/blob/resources/maps_menu.png?raw=true)

6. **Support for Local Language Change**: Demonstrates the plugin's support for language customization.

   ![Language Support](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/blob/resources/changed_language.png?raw=true)

## Dependencies

To run this plugin, you need the following dependencies:

1. **Metamod:Source (2.x)**  
   Download from: [Metamod:Source Official Site](https://www.sourcemm.net/downloads.php/?branch=master)

2. **CounterStrikeSharp**  
   You can get it from: [CounterStrikeSharp GitHub Releases](https://github.com/roflmuffin/CounterStrikeSharp/releases)

3. **MultiAddonManager** *(optional)*  
   Download from: [MultiAddonManager GitHub Releases](https://github.com/Source2ZE/MultiAddonManager/releases)  

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

1. **`!nextmap`**
   - **Description**: Displays the next map in the cycle.
   - **Access**: Available to all players.
     
2. **`!timeleft`**
   - **Description**: Displays the remaining time before the current map ends (rounds/minutes).
   - **Access**: Available to all players.

3. **`!currentmap`**
   - **Description**: Displays the current map name.
   - **Access**: Available to all players.

4. **`!lastmap`**
   - **Description**: Displays the last played map.
   - **Access**: Available to all players.

5. **`!rtv`** or **`!rockthevote`**
   - **Description**: Initiates a Rock The Vote to change the current map.
   - **Access**: Available to all players.

6. **`!nominate`** or **`!nom`**
   - **Description**: Nominates a map to be included in the next map vote.
   - **Usage**: `!nominate <map>` or just `!nominate` to open a menu.
   - **Access**: Available to all players.

7. **`!nominations`** or **`!noms`**
   - **Description**: Shows the current map nominations.
   - **Access**: Available to all players.

8. **`css_nextmap`**
   - **Description**: Sets the next map in the rotation.
   - **Access**: Admins only, requires `@css/changemap` permission.

9. **`css_maps`**
   - **Description**: Lists all maps and allows instant map changes.
   - **Access**: Admins only, requires `@css/changemap` permission.

10. **`css_nominate`** or **`css_nom`**
    - **Description**: Console command to nominate a map.
    - **Usage**: `css_nominate <map>` or just `css_nominate` to open a menu.
    - **Access**: Available to all players.

11. **`css_nominations`** or **`css_noms`**
    - **Description**: Console command to show current map nominations.
    - **Access**: Available to all players.
   - **Access**: Admins only, requires `@css/changemap` permission.

## Configuration Tutorial

Below is a step-by-step guide explaining the available configuration options for **MapCycleAndChooser**. These options allow you to customize how the plugin behaves and interacts with players.

### Configuration File Structure Changes in v1.2

In version 1.2, the configuration system has been significantly improved:

1. **Individual Map Configuration Files**:
   - Each map now has its own configuration file located in `game/csgo/addons/counterstrikesharp/configs/plugins/MapCycleAndChooser-COFYYE/maps/`.
   - Files are named after the map (e.g., `de_dust2.json`).
   - This makes it easier to manage settings for individual maps without editing a large configuration file.

2. **Default Map Template**:
   - A default map configuration template is stored at `game/csgo/addons/counterstrikesharp/configs/plugins/MapCycleAndChooser-COFYYE/default_map_config.json`.
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
   - Map cooldowns are stored in `game/csgo/addons/counterstrikesharp/configs/plugins/MapCycleAndChooser-COFYYE/cooldowns.json`.

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

4. **`depends_on_the_round`**  
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
      - If `depends_on_round` is `true`, the value represents rounds.  
      - If `depends_on_round` is `false`, the value represents minutes.  

18. **`extend_map_position`**  
    - **Possible Values**: `"top"`, `"bottom"`  
    - **Description**: Defines whether the "Extend Map" option appears at the top or bottom of the voting menu.  

19. **`delay_to_change_map_in_the_end`**  
    - **Possible Values**: Integer values greater than `5`  
    - **Description**: Defines the delay (in seconds) between the end of the current map and the actual map change.  

20. **`vote_trigger_time_before_map_end`**  
    - **Possible Values**: Integer values greater than `2`  
    - **Description**: Defines how long before the end of the current map the vote is triggered.  
      - If `depends_on_round` is `true`, the value is in rounds.  
      - If `depends_on_round` is `false`, the value is in minutes.  

21. **`rtv_enable`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables the Rock The Vote (RTV) functionality.  
      - `true`: RTV is enabled.  
      - `false`: RTV is disabled.  

22. **`rtv_delay`**  
    - **Possible Values**: Integer values greater than `0`  
    - **Description**: Defines the delay (in seconds) after map start before players can use RTV.  

23. **`rtv_player_percentage`**  
    - **Possible Values**: Integer values between `1` and `100`  
    - **Description**: Defines the percentage of players needed to trigger an RTV vote.  

24. **`rtv_change_instantly`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Determines if the map changes immediately when RTV is triggered.  
      - `true`: Map changes immediately.  
      - `false`: Map changes at the end of the round.  

25. **`rtv_respect_nextmap`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Determines if RTV should use the already set nextmap.  
      - `true`: Uses the already set nextmap when RTV is triggered.  
      - `false`: Starts a new vote when RTV is triggered.  

26. **`enable_map_cooldown`**  
    - **Possible Values**: `true`, `false`  
    - **Description**: Enables or disables map cooldowns to prevent the same maps from being played too frequently.  
      - When enabled, maps that were recently played will be excluded from the map cycle and voting until their cooldown expires.

27. **`commands_css_nextmap`**  
    - **Possible Values**: List of strings  
    - **Description**: Defines alias commands for `css_nextmap`.  

28. **`commands_css_maps`**  
    - **Possible Values**: List of strings  
    - **Description**: Defines alias commands for `css_maps`.  

29. **`commands_nextmap`**  
    - **Possible Values**: List of strings  
    - **Description**: Defines alias commands for `!nextmap`.  

30. **`commands_lastmap`**  
    - **Possible Values**: List of strings  
    - **Description**: Defines alias commands for `!lastmap`.  

31. **`commands_currentmap`**  
    - **Possible Values**: List of strings  
    - **Description**: Defines alias commands for `!currentmap`.  

32. **`commands_timeleft`**  
    - **Possible Values**: List of strings  
    - **Description**: Defines alias commands for `!timeleft`.  

33. **`commands_rtv`**
    - **Possible Values**: List of strings
    - **Description**: Defines alias commands for `!rtv`.

34. **`commands_css_nominate`**
    - **Possible Values**: List of strings
    - **Description**: Defines alias commands for `css_nominate`.

35. **`commands_css_nominations`**
    - **Possible Values**: List of strings
    - **Description**: Defines alias commands for `css_nominations`.

36. **`sounds`**
   - **Possible Values**: An array of string paths to sound files.
   - **Description**: Specifies the sounds that play when map voting begins.
     - Add as many sounds as you'd like, and the plugin will play one randomly.
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

### Installation

1. Download the **[MapCycleAndChooser v1.2](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/releases/download/1.2/MapCycleAndChooser-COFYYE-v1.2.zip)** plugin as a `.zip` file.  
2. Upload the contents of the `.zip` file into the following directory on your server:  
   > game/csgo/addons/counterstrikesharp/plugins  
3. After uploading, change the map or restart your server to activate the plugin.  
4. The configuration files will be generated at:  
   > game/csgo/addons/counterstrikesharp/configs/plugins/MapCycleAndChooser-COFYYE/  
   Adjust all settings in these files as needed.

### Language Support
The language files are located in the following directory:
> game/csgo/addons/counterstrikesharp/plugins/MapCycleAndChooser-COFYYE/lang

Currently, there are three language files:
- `en.json` (English)
- `sr.json` (Serbian)
- `pl.json` (Polish)

## Bug Reports & Suggestions

If you encounter any bugs or issues while using the plugin, please report them on the [GitHub Issues page](https://github.com/cofyye/CS2-MapCycleAndChooser-COFYYE/issues). Provide a detailed description of the problem, and I will work on resolving it as soon as possible.

Feel free to submit any suggestions for improvements or new features you'd like to see in future releases. Your feedback is highly appreciated!

## Important Notes

- **ScreenMenuAPI Not Included**:
  The **ScreenMenuAPI** is not included in this version of the plugin due to necessary adjustments required for full compatibility. Additional refinements are needed to ensure seamless functionality with this plugin.

  Please be patient, and expect an update in the near future that will introduce **ScreenMenuAPI**! 🚀

## Credits

- **Code Snippets for Menu**: The menu code snippets were sourced from [oqyh's GitHub](https://github.com/oqyh). I would like to thank him for providing valuable resources that helped in building parts of this plugin.
  
- **Other Contributors**: A big thank you to all other authors and contributors of similar plugins that inspired the creation of this MapCycle and Chooser plugin. Their work was a key part of shaping the final version of this plugin.

This plugin is my version of the MapCycle and Chooser functionality, combining various elements from the community to provide a better and more customizable experience for server admins and players alike.

## Donation

If you would like to support me and help maintain and improve this plugin, you can donate via PayPal:

[Donate on PayPal](https://paypal.me/cofyye)

Your support is greatly appreciated!
