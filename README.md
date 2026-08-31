# MIniMap

[![Thunderstore](https://img.shields.io/badge/Thunderstore-minimapa%20diman3012-blue)](https://thunderstore.io/c/lethal-company/p/SHLUHA/minimapa_diman3012/)
[![GitHub: invertigo260](https://img.shields.io/badge/GitHub-invertigo260-black?logo=github)](https://github.com/invertigo260)
[![License: AGPL-3.0](https://img.shields.io/badge/License-AGPL--3.0-blue)](LICENSE)
[![Game: Lethal Company](https://img.shields.io/badge/Game-Lethal%20Company-red)](https://store.steampowered.com/app/1966720/Lethal_Company/)
[![BepInEx 5](https://img.shields.io/badge/BepInEx-5.x-green)](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)

Minimalist minimap mod for **Lethal Company**. Displays the ship radar directly on your HUD so you can track teammates, scrap, and threats without returning to the ship monitor.

Inspired by [LethalCompanyMinimap](https://github.com/tyzeron/LethalCompanyMinimap) by **tyzeron**; reworked into a lightweight HUD overlay with BepInEx config and custom target-switching logic.

> **Note:** Starting from version **1.1.6**, **[invertigo260](https://github.com/invertigo260)** joined the development, completely overhauling the camera system, zoom levels, target tracking logic, and input handling.

**[Download on Thunderstore](https://thunderstore.io/c/lethal-company/p/SHLUHA/minimapa_diman3012/)** · **[GitHub (Diman3012)](https://github.com/Diman3012/MIniMap)** · **[GitHub (invertigo260)](https://github.com/invertigo260)**

---

## Features

- **HUD overlay** — radar view in the top-right corner of your screen.
- **Interactive Edit Mode (v1.1.6+)** — Hold `F2` for 2 seconds to unlock the cursor. Drag the map to reposition it on your screen, or drag its edges to dynamically resize it while maintaining a perfect square.
- **Independent Minimap Camera (v1.1.6+)** — separate Camera and RenderTexture wired directly into the minimap UI to avoid interfering with the ship's main monitor.
- **Separate Target Tracking (v1.1.6+)** — uses `CustomTarget`/`CustomTargetIndex` so cycling minimap targets no longer disrupts ship radar operations.
- **Zoom Controls (v1.1.6+)** — dynamic zoom level cycling on hotkey press.
- **Smart Resource Management (v1.1.6+)** — automatically disables the camera during ship phase to save system resources.
- **Persistent State** — toggle on/off with `F2`; preference saved in BepInEx config.
- **Typing Protection (v1.1.6+)** — hotkeys are automatically suppressed while typing in chat or using the terminal.
- **HUD Tip Feedback (v1.1.6+)** — displays a visual HUD tip notification when toggling the minimap.
- **Auto-rotate & Icon Correction** — map rotates with target view while keeping icons upright.

---

## Controls

| Action | Key | Description |
| --- | --- | --- |
| Toggle minimap | `F2` (Press) | Show/hide minimap, show HUD tip, and save state to config |
| Edit Mode | `F2` (Hold 2s) | Enable/disable UI editing. Unlocks the mouse cursor to customize the map |
| Move Map | `Left Click (Drag)` | Click and drag inside the map during Edit Mode to move it around the screen |
| Resize Map | `Left Click (Edges)` | Click and drag the edges of the map during Edit Mode to scale its size |
| Switch target | `F3` | Cycle through valid minimap targets independently |
| Zoom minimap | `F4` | Cycle through available zoom levels (`ZoomLevels`) |

---

## Developer Contributions (v1.1.6 Update by invertigo260)

The v1.1.6 release introduces major structural improvements designed and implemented by **[invertigo260](https://github.com/invertigo260)**:

- **Interactive UI Edit Mode:** Designed a Canvas-aware scaling system to convert screen space coordinates to local UI positions, allowing players to drag and resize the minimap in real-time without breaking player look inputs.
- **Independent Camera & RenderTexture:** Created a dedicated camera system wired into the minimap UI, complete with full lifecycle management (initialization, recreation, and resource release). Automatically powers off during the ship phase to minimize performance impact.
- **Decoupled Target Tracking (`CustomTarget` / `CustomTargetIndex`):** Implemented standalone target state handling and `SwitchKey` processing, making the minimap completely independent of ship radar selection.
- **Zoom & Positioning Pipeline:** Integrated `ZoomKey`, `ZoomLevels`, and `currentZoomIndex` cycling, alongside a per-frame `UpdateMinimapCamera` method to control positioning and rotation smoothly.
- **Input & UX Improvements:** Added context-aware input checks that block hotkey activation while typing or using the terminal, and added instant HUD tips on state toggle.
- **Refactoring:** Removed legacy update logic and obsolete blocking patches for cleaner code execution.

---

## Authors & Credits

- **[Diman3012 / SHLUHA](https://github.com/Diman3012)** — Original author & project maintainer.
- **[invertigo260](https://github.com/invertigo260)** — Co-developer (v1.1.6+ independent camera architecture, target/zoom controls, QoL improvements).
- **[tyzeron](https://github.com/tyzeron/LethalCompanyMinimap)** — Original minimap concept inspiration.

---

## License

This project is licensed under the **GNU Affero General Public License v3.0**. See [LICENSE](LICENSE) for details.