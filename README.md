# EpicMMOVRFix

A compatibility fix that makes the EpicMMO mod's user interface properly visible and functional in Valheim VR.

---

## 🚀 Features

- ✅ Full **EpicMMO UI support in VR**
- ✅ Enemy level display works correctly in VR HUD
- ✅ **XP Bar moved into Inventory (VR-friendly)**
- ✅ Optional fallback to standard HUD XP bar
- ✅ **Configurable XP bar position & scale (Inventory + HUD)**
- ✅ **VR-compatible critical damage numbers**
  - Uses VR-native floating text
  - Includes black outline for readability
- ✅ Automatic recovery after:
  - Death / respawn
  - Scene changes
  - UI reloads
- ✅ Non-intrusive — does NOT modify EpicMMO files

---

## 📦 Installation

1. Install :contentReference[oaicite:0]{index=0}  
2. Install Valheim VR Mod (VHVR)  
3. Install :contentReference[oaicite:1]{index=1}  
4. Install **EpicMMOVRFix**

---

## 🎮 Usage

The mod works automatically.

### Default Behavior
- Open **Inventory** → XP bar appears inside VR UI
- Close Inventory → XP bar returns to HUD (if enabled)

### Important
- ⚠️ **"Force Old XP Bar" MUST be enabled** in EpicMMO  
  (This mod depends on it for stability)

---

## ⚙️ Configuration

Config file allows full control:

### XP Bar (Inventory Mode)
- `EnableInventoryXPBar` (default: true)
- `InventoryXPBarPosX` (default: 500)
- `InventoryXPBarPosY` (default: 650)
- `InventoryXPBarScale`

### XP Bar (HUD Mode)
- `HudXPBarPosX`
- `HudXPBarPosY`
- `HudXPBarScale`

### General
- Enable/disable mod
- Enable debug logging

### 🔄 Live Config
- Changes apply **immediately in-game**
- No restart required

---

## 🧠 How it Works

This mod acts as a **bridge between EpicMMO and Valheim VR (VHVR)**:

- Converts EpicMMO UI into **VR-compatible world space canvases**
- Repositions UI elements (like XP bar) into VR-friendly locations
- Replaces incompatible systems:
  - ❌ TMP damage text → ✅ VRDamageTexts
- Uses **event-driven updates** (not polling) for performance

---

## ⚡ Performance

Designed for **heavy mod setups (100+ mods)**:

- No constant update loops
- Minimal reflection usage
- UI updates triggered only when needed:
  - Inventory open/close
  - Player spawn
- Eliminates UI spam and stutter

---

## 🔧 Compatibility

- ✔ Requires EpicMMO 1.5.11+
- ✔ Requires Valheim VR Mod (VHVR)
- ✔ Works alongside other VR UI mods
- ✔ Safe for multiplayer

---

## ⚠️ Known Issues

- Other player levels may not always display in VR
- XP bar positioning may need manual adjustment depending on UI scale mods

---

## 🛠 Troubleshooting

If something isn’t working:

1. Verify EpicMMO works in flatscreen first
2. Ensure **Old XP Bar is enabled**
3. Check all dependencies are installed
4. Enable debug logs in config
5. Open inventory once to force UI refresh

---

## 📂 Source Code

Available on GitHub:  
https://github.com/geekstreetstudios/EpicMMOVRFix

---

## 🙌 Credits

- WackyMole — EpicMMO  
- Valheim VR Mod Team  
- Valheim modding community  

---