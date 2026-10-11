# InspectTeamUpgrades

A client-side mod for **R.E.P.O.** that lets you inspect your teammates' equipped upgrades directly in the Tab map screen.

---

## Features

- **Teammate Upgrade Pages:** Hold **Tab** to view your upgrades, and scroll or press arrow keys to cycle through each teammate's equipped upgrades.
- **Client-Side Only:** Other players do **not** need the mod installed for you to see their upgrades.
- **Seamless UI:** Matches the game's built-in UI style and page indicator (`< 1/4 >`). Automatically jumps back to your own upgrades when closing the map or collecting an upgrade.

---

## Controls

While holding the **Map key** (**Tab** by default):

| Action | Default Input |
|---|---|
| **Next Player's Upgrades** | **Mouse Wheel Down** or **Right Arrow (→)** |
| **Previous Player's Upgrades** | **Mouse Wheel Up** or **Left Arrow (←)** |

*(All controls are configurable in the mod config file or r2modman config editor.)*

---

## Installation

### Option 1: Via Mod Manager (Recommended)
1. Install [r2modman](https://thunderstore.io/package/ebkr/r2modman/) or Thunderstore Mod Manager.
2. Search for **InspectTeamUpgrades**.

### Option 2: Manual Install
1. Ensure you have [BepInEx](https://thunderstore.io/c/repo/p/BepInEx/BepInExPack/) installed.
2. Download the binary under releases.
3. Extract `InspectTeamUpgrades.dll` into your `REPO/BepInEx/plugins/` folder (or create a subfolder `REPO/BepInEx/plugins/InspectTeamUpgrades/`).

---

## Configuration

Settings are saved in `BepInEx/config/jahnpm.InspectTeamUpgrades.cfg` (or editable via r2modman's Config Editor):

| Section | Setting | Default | Description |
|---|---|---|---|
| **Controls** | `EnableScrollWheel` | `true` | Cycle upgrade pages using the mouse scroll wheel. |
| **Controls** | `InvertScrollWheel` | `false` | Invert scroll direction (default: scroll down = next page). |
| **Controls** | `NextPageKey` | `RightArrow` | Keyboard key to show the next player's upgrades. |
| **Controls** | `PrevPageKey` | `LeftArrow` | Keyboard key to show the previous player's upgrades. |
| **General** | `ResetPageOnClose` | `true` | Jump back to your own upgrades every time the map is closed. |
| **Debug** | `TestPages` | `0` | Number of fake test pages to append for solo testing (0 = off). |

---

## Building from Source

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (8.0 or newer recommended)

### Configurable Paths
The project file [`InspectTeamUpgrades.csproj`](InspectTeamUpgrades.csproj) defines two paths that you may need to adjust for your system:

- **`GameDir`** (Default: `C:\Program Files (x86)\Steam\steamapps\common\REPO`):
  Path to your R.E.P.O. installation. Game assemblies located in `$(GameDir)\REPO_Data\Managed` are referenced during compilation.
- **`ProfilePluginsDir`** (Default: `$(AppData)\r2modmanPlus-local\REPO\profiles\Default\BepInEx\plugins`):
  Path to an r2modman profile's plugin folder. If this directory exists, the compiled mod is automatically deployed there on build.

You can configure these paths by:
- Creating a `Directory.Build.props.user` file in the repository root (recommended — ignored by Git)
- Editing them directly in `InspectTeamUpgrades.csproj`
- Passing them as properties via the command line (e.g., `-p:GameDir="<PathToGame>"`)

**Example `Directory.Build.props.user`:**
```xml
<Project>
  <PropertyGroup>
    <GameDir>D:\SteamLibrary\steamapps\common\REPO</GameDir>
    <ProfilePluginsDir>$(AppData)\r2modmanPlus-local\REPO\profiles\MyProfile\BepInEx\plugins</ProfilePluginsDir>
  </PropertyGroup>
</Project>
```

### Build
Run the following command from the repository root:

```sh
dotnet build -c Release
```

The compiled DLL will be located at `bin/Release/netstandard2.1/InspectTeamUpgrades.dll` (and deployed to `ProfilePluginsDir` if found).

---

## Disclaimer

This mod was created with the assistance of AI.
