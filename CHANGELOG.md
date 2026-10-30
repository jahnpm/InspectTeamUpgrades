# Changelog

All notable changes to **InspectTeamUpgrades** will be documented in this file.

## [0.9.1] - 2026-10-30

### Added
- **Gamepad Support:** Cycle teammate upgrade pages using a controller while holding the map key. Default controls are **D-Pad Right** (next page) and **D-Pad Left** (previous page).
- **Gamepad Configuration:** Added `EnableGamepad`, `NextPageGamepadButton`, and `PrevPageGamepadButton` configuration options in `BepInEx/config/jahnpm.InspectTeamUpgrades.cfg` (also configurable via r2modman / Thunderstore Mod Manager).

### Changed
- **Universal Empty State:** Replaced the English `"No upgrades"` text with a language-agnostic dash symbol (`—`) for teammates with no equipped upgrades, maintaining a clean look across all 21 supported languages.
- Verified compatibility with R.E.P.O. v0.5.0 (The Sunstone Update).

---

## [0.9.0] - 2026-10-11

### Added
- Initial release.
- Inspect teammates' equipped upgrades directly in the Tab map screen.
- Cycle upgrade pages using mouse scroll wheel or keyboard arrow keys (Left/Right).
- Configurable controls, page reset on map close, and solo test pages.
