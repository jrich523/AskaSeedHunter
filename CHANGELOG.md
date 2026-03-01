# Changelog

All notable changes to SeedHunter will be documented in this file.

## [1.0.0] - 2026-03-01

### Added
- Initial release
- F8: Continuous map reveal for caves, objectives, and locations
- F9: Fast movement mode with adjustable speed (5-100)
- F10: Noclip/fly mode with Space (up) and Ctrl (down) controls
- F7: Reset all cheats to normal gameplay
- +/-: Speed adjustment controls
- God mode (invincibility + no fall damage) auto-enabled with F9 or F10
- Persistent settings that save between sessions
- On-screen status overlay showing active features

### Fixed
- IL2CPP compatibility for velocity field access
- Optimized performance by removing debug logging
- Fixed collision detection for noclip mode

### Technical
- Compatible with BepInEx 6.0.738 IL2CPP
- Uses HarmonyX for runtime patching
- Supports ASKA game updates (tested with Feb 28, 2026 update)
