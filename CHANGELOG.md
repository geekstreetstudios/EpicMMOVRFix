# Changelog

---

## [1.2.0] - 2026-04-22

### Added
- **Inventory XP Bar system**
  - Moves EpicMMO XP bar from HUD → Inventory (VR-friendly)
  - Fully configurable position (X/Y) and scale
  - Live toggle via config (`EnableInventoryXPBar`)
- **HUD fallback XP bar**
  - When inventory mode is disabled, XP bar returns to HUD
  - Separate position + scale config for HUD mode
- **VR Crit Damage Text**
  - Replaced EpicMMO TMP-based crit text with `VRDamageTexts`
  - Added **black outline** for improved readability in VR
- **Auto-recovery system for XP bar**
  - XP bar rebinds automatically after:
    - player death
    - respawn
    - UI reload
    - scene transitions

---

### Changed
- Forced `oldExpBar` to **always ON** for compatibility with VR
- XP bar is now **re-parented dynamically** instead of recreated
- UI processing optimized to avoid repeated canvas processing
- Logging reduced to prevent spam and micro-stutter

---

### Fixed
- XP bar disappearing after death or respawn
- XP bar duplicating or remaining in HUD when moved to inventory
- XP bar not updating correctly when toggling config live
- Incorrect XP bar placement when switching modes
- Crit damage text not visible in VR
- Repeated log spam: EpicMMO canvas processed for VR

---

### Performance
- Removed continuous polling loops for XP bar detection
- Replaced with **event-driven updates**:
- Inventory open/close
- Player spawn
- Reduced reflection usage where possible
- Eliminated unnecessary UI reprocessing
- Improved overall VR UI stability with large mod lists

---

## [1.0.2] - 2026-04-11

### Changed
- Refactored `EpicMMOVRHUD` to **only apply to players**
- Removed all logic related to:
- Monsters
- Creatures
- Mob level detection
- `DataMonsters` reflection usage
- Simplified level display to a consistent format:  
`PlayerName [Lv X]`

### Fixed
- Removed unintended `"moblvl"` text caused by EpicMMO mob config (`MobLVLChars`)
- Eliminated level conflicts between player and creature systems

### Performance
- Removed expensive reflection calls for monster level lookups
- Reduced string processing and cache complexity
- Lowered overhead in VR HUD updates

---

## [1.0.1] - Previous

### Added
- Initial VR UI compatibility patch for EpicMMOSystem
- VR canvas handling for EpicMMO UI
- HUD name modification with level display

---

## Notes

- This mod now:
- Fully supports **VR UI for EpicMMO**
- Moves key UI elements into **VR-friendly space**
- Maintains compatibility with **EpicMMO core systems**
- Designed for **high-mod-count environments (100+ mods)** with minimal overhead

---