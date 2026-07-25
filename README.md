# MIniMap

[![Thunderstore](https://img.shields.io/badge/Thunderstore-minimapa%20diman3012-blue)](https://thunderstore.io/c/lethal-company/p/SHLUHA/minimapa_diman3012/)
[![License: AGPL-3.0](https://img.shields.io/badge/License-AGPL--3.0-blue)](LICENSE)
[![Game: Lethal Company](https://img.shields.io/badge/Game-Lethal%20Company-red)](https://store.steampowered.com/app/1966720/Lethal_Company/)
[![BepInEx 5](https://img.shields.io/badge/BepInEx-5.x-green)](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/)

Minimalist minimap mod for **Lethal Company**. Displays the ship radar directly on your HUD so you can track teammates, scrap, and threats without returning to the ship monitor.

Inspired by [LethalCompanyMinimap](https://github.com/tyzeron/LethalCompanyMinimap) by **tyzeron**; reworked into a lightweight HUD overlay with BepInEx config and custom target-switching logic.

**[Download on Thunderstore](https://thunderstore.io/c/lethal-company/p/SHLUHA/minimapa_diman3012/)** · **[GitHub](https://github.com/Diman3012/MIniMap)**

---

## Features

- **HUD overlay** — radar view in the top-right corner of your screen.
- **Persistent state** — toggle on/off with `F2`; preference saved in BepInEx config.
- **Auto-rotate** — map rotates with the current target's view direction.
- **Icon correction** — map icons and compass rose stay upright when the map rotates.
- **Target locking** — prevents the game from auto-switching your radar target while the minimap is active.
- **Manual cycling** — switch between valid radar targets with a hotkey.
- **Death support** — follows your spectated player when dead; returns to you on respawn.

## Controls

| Action | Key | Description |
| --- | --- | --- |
| Toggle minimap | `F2` | Show/hide minimap and save state to config |
| Switch target | `F3` | Cycle to the next valid radar target |

> Minimap is **disabled by default**. Press `F2` once after installing to enable it.

## Installation

### Thunderstore Mod Manager (recommended)

1. Install [r2modman](https://thunderstore.io/package/ebkr/r2modman/) or [Gale Mod Manager](https://thunderstore.io/c/lethal-company/p/Kastraliss/GaleModManager/).
2. Search for **minimapa diman3012** by **SHLUHA**.
3. Install and launch the game through the mod manager.

### Manual

1. Install [BepInEx Pack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/) for Lethal Company.
2. Download `MIniMap.dll` from [Thunderstore](https://thunderstore.io/c/lethal-company/p/SHLUHA/minimapa_diman3012/) or build from source.
3. Place the DLL in:

```text
Lethal Company/BepInEx/plugins/
```

4. Launch the game once to generate the config:

```text
BepInEx/config/com.diman3012.minimap.cfg
```

## Configuration

| Option | Default | Description |
| --- | --- | --- |
| `Enabled` | `false` | Whether the minimap is visible (also toggled with `F2`) |

Additional settings (size, zoom, offsets, hotkeys) are defined in code in `MinimapData` inside `MinimalMinimap.cs`.

## Building from source

Requires **.NET Framework 4.8** and a local Lethal Company install with BepInEx.

1. Open `MIniMap.slnx` in Visual Studio or Rider.
2. Update `HintPath` references in `MIniMap/MIniMap.csproj` to point to your game folder.
3. Build in **Release** configuration.
4. Copy `MIniMap/bin/Release/MIniMap.dll` to `BepInEx/plugins/`.

## Technical details

| | |
| --- | --- |
| Plugin GUID | `com.diman3012.minimap` |
| Plugin name | `Minimal Minimap` |
| Version | `1.1.0` |
| Namespace | `MIniMap` |

**Key files:**

- `MIniMap/MinimalMinimap.cs` — BepInEx plugin, config, network prefab registration.
- `MIniMap/MinimapPatch.cs` — HUD overlay, hotkeys, target switching, death/spectator logic.
- `MIniMap/ManualCameraRendererPatch.cs` — map camera zoom, auto-rotate, icon correction.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## License

This project is licensed under the **GNU Affero General Public License v3.0**. See [LICENSE](LICENSE) for details.

---

## Русский

Минималистичная миникарта для **Lethal Company**. Радар корабля выводится прямо в HUD — можно следить за картой, не возвращаясь к монитору на корабле.

### Возможности

- Миникарта в правом верхнем углу экрана.
- Включение/выключение по **F2**, состояние сохраняется в конфиге.
- Автоповорот карты по направлению взгляда цели.
- Фиксация цели радара — игра не переключает её сама.
- **F3** — ручное переключение между игроками.
- При смерти карта следует за наблюдаемым игроком; после возрождения возвращается к вам.

### Установка

1. Установите [BepInEx Pack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/).
2. Скачайте мод с [Thunderstore](https://thunderstore.io/c/lethal-company/p/SHLUHA/minimapa_diman3012/) или соберите из исходников.
3. Положите `MIniMap.dll` в `Lethal Company/BepInEx/plugins/`.
4. Запустите игру и нажмите **F2**, чтобы включить миникарту.

Конфиг: `BepInEx/config/com.diman3012.minimap.cfg`

---

**Author:** [Diman3012](https://github.com/Diman3012) · **Based on:** [LethalCompanyMinimap](https://github.com/tyzeron/LethalCompanyMinimap) by tyzeron
