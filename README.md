# Vessel Bookmark Mod

A Kerbal Space Program mod that allows you to bookmark vessels by their command modules for easy navigation and management.

## Features

- **Bookmark Management**: Add and remove bookmarks for any vessel with a command module
- **Easy Navigation**: Quickly switch to any bookmarked vessel from the Tracking Station, Map View, or in-flight
- **Filtering**: Filter bookmarks by celestial body and vessel type
- **Custom Comments**: Add personal notes to each bookmark
- **Custom Ordering**: Reorder bookmarks to your preference
- **Persistent Storage**: Bookmarks are saved with your game save file
- **Context Menu Integration**: Add/remove bookmarks directly from the part action menu on command modules

## Installation

1. Download the latest release from the [releases page](https://github.com/lhervier/KSP-VesselBookmark/releases)
2. Extract the `VesselBookmarkMod` folder to your `KSP/GameData/` directory
3. Ensure you have [ModuleManager](https://github.com/sarbian/ModuleManager) installed (required for the mod to function)

## Usage

### Adding a Bookmark

1. Right-click on any command module (cockpit, probe core, etc.)
2. Click "Add to Bookmarks" in the part action menu
3. A confirmation message will appear

### Managing Bookmarks

1. Click the Vessel Bookmark icon in the application launcher toolbar (visible in Flight, Map View, Tracking Station, and Space Center)
2. The bookmark window will open showing all your bookmarks
3. Use the filters to narrow down your bookmarks by:
   - **Body**: Filter by celestial body (Kerbin, Mun, Duna, etc.)
   - **Type**: Filter by vessel type (Ship, Station, Probe, Rover, etc.)

### Bookmark Actions

For each bookmark, you can:

- **Edit**: Add or modify a comment for the bookmark
- **Go to**: Switch to that vessel (works from any scene)
- **Remove**: Delete the bookmark
- **Reorder**: Use ↑ and ↓ buttons to change the bookmark order

### Navigation

The "Go to" button will:
- Switch to the vessel if you're already in flight
- Load the flight scene and focus on the vessel if you're in the Tracking Station or Map View
- Handle both loaded and unloaded vessels automatically

## Technical Details

### How It Works

- The mod uses the `flightID` of command modules as unique identifiers
- Bookmarks are stored in your save file under the `VESSEL_BOOKMARKS` node
- The mod automatically injects a `VesselBookmarkPartModule` into all parts with `ModuleCommand` using ModuleManager
- Bookmarks persist across game sessions and are tied to your save file

### Compatibility

- **KSP Version**: Compatible with KSP 1.x (tested with KSP 1.12+)
- **Dependencies**: Requires ModuleManager
- **Mod Conflicts**: Should be compatible with most mods. If you encounter issues, please report them.

## Building from Source

### Prerequisites

- .NET Framework 4.7.2 or later
- Visual Studio or MSBuild
- Kerbal Space Program installed (for assembly references)

### Build Steps

1. Set the `KSPDIR` environment variable to your KSP installation directory:
   ```batch
   set KSPDIR=C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program
   ```

2. Run the build script:
   ```batch
   build.bat
   ```

3. The compiled mod will be in the `Release` folder as `VesselBookmarkMod.zip`

### Install to KSP

1. Set the `KSPDIR` environment variable (if not already set)
2. Run the install script:
   ```batch
   install.bat
   ```

## Project Structure

```
KSP-VesselBookmark/
├── GameData/
│   └── VesselBookmarkMod/
│       ├── VesselBookmarkMod.cfg    # ModuleManager config
│       └── icon.png                 # Toolbar icon
├── VesselBookmarkMod.cs             # Main mod entry point
├── VesselBookmarkManager.cs         # Bookmark management logic
├── VesselBookmarkUI.cs              # User interface
├── VesselBookmarkData.cs            # Bookmark data structure
├── VesselBookmarkContextMenu.cs     # Part module for context menu
├── VesselNavigator.cs                # Vessel navigation logic
├── VesselSituationDetector.cs       # Vessel situation detection
├── ModLogger.cs                     # Logging utility
├── build.bat                        # Build script
└── install.bat                      # Installation script
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Issues

If you encounter any bugs or have feature requests, please open an issue on the [GitHub Issues page](https://github.com/lhervier/KSP-VesselBookmark/issues).

## Credits

- **Author**: Lionel Hervier, using Cursor
- **License**: MIT License

## Changelog

### Version 1.0
- Initial release
- Bookmark management
- Filtering by body and vessel type
- Custom comments
- Bookmark reordering
- Navigation to vessels
- Persistent storage in save files
