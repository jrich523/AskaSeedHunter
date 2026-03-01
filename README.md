# SeedHunter - ASKA World Seed Explorer Mod

A BepInEx mod for ASKA that helps players rapidly evaluate world seeds by revealing POIs and enabling fast exploration. Perfect for finding your ideal world!

## Features

- **F8** - Reveal all caves, locations, and objectives on the map
- **F9** - Toggle fast movement + infinite stamina + god mode (adjustable speed)
- **F10** - Toggle noclip/fly mode (Space=up, Ctrl=down) + god mode
- **F7** - Disable all cheats and return to normal
- **+/-** - Increase/decrease movement speed (range: 5-100, default: 50)

All settings are automatically saved between sessions!

## Installation

### Via Thunderstore (Recommended)

1. Install [r2modman](https://thunderstore.io/c/aska/p/ebkr/r2modman/) or [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)
2. Search for "SeedHunter" in the mod manager
3. Click Install

### Manual Installation

1. Install [BepInEx 6 IL2CPP](https://thunderstore.io/c/aska/p/BepInEx/BepInExPack_IL2CPP/) for ASKA
2. Download `SeedHunter.dll` from the [latest release](https://github.com/jrich523/SeedHunter/releases)
3. Place `SeedHunter.dll` in `ASKA/BepInEx/plugins/` folder
4. Launch the game

## Usage

This isnt the perfect approach but it works. you'll want to enable all features and likely bump speed to 100 and then run forward and jump. while you are holding space you cant move forward, so get high (sometimes you can go too high) and then fall forward until you get low (dont hit the ground, i couldnt stop fall damage) and hold space again. Easy enough once you get the hang of it and can quick open a map.

### Map Reveal (F8)

Continuously reveals caves, objectives, and locations as they load. Toggle on before exploring to ensure everything is revealed as you move around the world.

### Fast Movement (F9)

- Increases movement speed (default 50x, adjustable with +/-)
- Infinite stamina
- God mode (invincibility + no fall damage)

### Noclip/Fly Mode (F10)

- Fly freely through the world
- Space = Fly up
- Ctrl = Fly down
- God mode (invincibility + no fall damage)

### Speed Adjustment (+/-)

- Use `+` (equals key) or numpad `+` to increase speed
- Use `-` (minus key) or numpad `-` to decrease speed
- Range: 5-100 (default: 50)
- Works with both fast movement and noclip modes

### Reset All (F7)

Disables all cheats and returns to normal gameplay.

## Requirements

- ASKA (Steam version)
- [BepInEx 6.0.738 or later](https://thunderstore.io/c/aska/p/BepInEx/BepInExPack_IL2CPP/)

## Development

### Building from Source

1. Clone this repository
2. Ensure BepInEx reference DLLs are in `lib/` folder
3. Run `quick-build.bat` to build and auto-deploy

### Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you would like to change.

## License

MIT License - Feel free to modify and redistribute

## Credits

- Inspired by the ASKA modding community
- Built with [BepInEx](https://github.com/BepInEx/BepInEx) and [HarmonyX](https://github.com/BepInEx/HarmonyX)
