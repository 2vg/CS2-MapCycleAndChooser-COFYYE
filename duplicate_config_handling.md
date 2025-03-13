# Handling of Duplicate Map Configuration Files

## Overview

The MapCycleAndChooser-COFYYE plugin has improved the handling of duplicate configuration files for Workshop maps. This document explains how this process works.

## Duplicate Configuration File Processing

When multiple configuration files exist for the same Workshop map (due to differences between Workshop titles and actual map names), the plugin uses a sophisticated algorithm to merge them:

1. **First-time Mapping**:
   - When a Workshop map is loaded for the first time, the plugin creates a mapping between its Workshop ID and official map name.
   - At this point, since no mapping exists yet, the plugin prioritizes settings that the user configured under the unofficial name (Workshop title).

2. **Timestamp-based Selection**:
   - If multiple configuration files exist for the same map, the plugin uses the most recently modified file as the source of settings.
   - This ensures that the user's most recent changes are prioritized.

3. **Intelligent Merging**:
   - If no official map name configuration exists: A new one is created using settings from the most recent duplicate.
   - If an official map name configuration already exists: The plugin compares timestamps:
     - If the official configuration is newer: It is preserved.
     - If a duplicate configuration is newer: The official configuration is updated with those settings.

4. **Backup Creation**:
   - Before deleting duplicate configurations, the plugin creates backups in the `backups` folder.
   - This allows recovery if needed.

5. **Detailed Logging**:
   - The plugin logs all decisions and actions taken during the merging process.
   - This makes it easier to troubleshoot if issues arise.

This system ensures that user customizations are preserved while maintaining a clean, consistent configuration structure.

## Example Scenario

Consider the following scenario:

1. A user subscribes to a Workshop map called "awesome_map" and loads it on their server.
2. The plugin detects the Workshop ID and the actual map name "workshop_awesome_map".
3. The user edits the configuration file named "awesome_map" to change settings.
4. Later, when the map is loaded again, the plugin detects two configuration files: "awesome_map" and "workshop_awesome_map".
5. The plugin verifies that the "awesome_map" configuration file is more recent and applies those settings to the "workshop_awesome_map" configuration file.
6. The "awesome_map" configuration file is backed up and then deleted.

This ensures that the user's configuration changes are not lost and are properly reflected in the official map name configuration file.