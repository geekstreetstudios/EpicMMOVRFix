# EpicMMOVRFix

A compatibility fix that makes the EpicMMO mod's user interface properly visible and functional in Valheim VR.

## Features

- Makes EpicMMO's level UI visible in VR
- Ensures enemy level displays work correctly in VR
- Maintains all EpicMMO functionality
- Non-intrusive - doesn't modify original EpicMMO files

## Installation

1. Install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
2. Install [Valheim VR Mod (VHVR)
3. Install [EpicMMO](https://thunderstore.io/c/valheim/p/WackyMole/WackyEpicMMOSystem/)
4. Install this mod

## Usage

The mod works automatically! Once installed:
- Press `Inventory` to open the EpicMMO level UI
- Enemy levels will display above their heads in VR
- 
## How it Works

This mod acts as a bridge between EpicMMO and Valheim VR Mod (VHVR). It detects when EpicMMO UI elements are shown and ensures they're properly processed by VHVR's VR rendering system for correct positioning and visibility.

## Compatibility

- Requires EpicMMO 1.5.11 or newer
- Requires Valheim VR Mod
- Compatible with other VR UI fixes

## Known Issues
- Other players level may not appear

## Configuration

The mod includes optional configuration:
- Enable/disable the VR fix
- Toggle debug logging for troubleshooting

## Troubleshooting

If UI elements don't appear:
1. Verify EpicMMO works in flatscreen first
2. Check all dependencies are installed correctly
3. Ensure you're running the latest versions
4. Enable debug logging in config for more information

## Source Code

Available on [GitHub](https://github.com/geekstreetstudios/EpicMMOVRFix)

## Credits

- Thanks to WackyMole for EpicMMO
- Thanks to the Valheim VR Mod team
- Thanks to the Valheim modding community
