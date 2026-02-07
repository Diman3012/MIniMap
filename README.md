# MIniMap 🛰️

<div align="center">

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Game: Lethal Company](https://img.shields.io/badge/Game-Lethal%20Company-red)](https://store.steampowered.com/app/1966720/Lethal_Company/)

**Choose Language / Выберите язык**
</div>

---

<details open>
<summary><b>🇬🇧 English Description (Click to expand)</b></summary>

> 🧩 This mod was inspired by and originally based on the work of  
> [LethalCompanyMinimap](https://github.com/tyzeron/LethalCompanyMinimap) by **tyzeron**. 
> The codebase has been significantly reworked for a minimalist HUD-based implementation with enhanced target control.

## Description
A minimalist mod for **Lethal Company** that adds a functional radar minimap directly to your HUD. Monitor your surroundings, teammates, and threats without returning to the ship.

## ✨ Features
* **Always Active:** The radar camera stays enabled even when you are far from the ship.
* **Toggle Visibility:** You can now hide or show the minimap at any time.
* **State Persistence:** The mod remembers your last choice. If you leave the map enabled, it will be enabled when you restart the game.
* **Disabled by Default:** On the first run, the map is hidden. Use `F2` to activate it.
* **Auto-Rotate:** The map aligns with your character's view direction for intuitive navigation.
* **Smart Icons:** Player markers and terminal objects remain correctly oriented during camera rotation.
* **Target Locking (Override):** Prevent the game or other players from changing your radar target automatically.
* **Manual Switching:** Quickly cycle through radar targets directly from your HUD.

## 🎮 Controls
| Action | Key | Description |
| :--- | :--- | :--- |
| **Toggle Map** | `F2` | Shows or hides the minimap HUD (Saves state) |
| **Toggle Override** | `F3` | Locks the current target (Prevents auto-switching) |
| **Switch Target** | `F4` | Manually cycles to the next radar target |

## 🛠️ Installation
1. Install [BepInEx Pack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/).
2. Download the **MIniMap.dll**.
3. Place the file into `Lethal Company/BepInEx/plugins`.
4. Run the game once to generate the config file in `BepInEx/config/com.diman3012.minimap.cfg`.

</details>

---

<details>
<summary><b>🇷🇺 Русское описание (Нажмите, чтобы развернуть)</b></summary>

> 🧩 Данный мод был вдохновлён и изначально основан на проекте  
> [LethalCompanyMinimap](https://github.com/tyzeron/LethalCompanyMinimap) от **tyzeron**. 
> Код был существенно переработан для создания минималистичной миникарты в HUD с расширенным управлением целями.

## Описание
Минималистичный мод для **Lethal Company**, который добавляет функциональный радар прямо в ваш HUD. Следите за окружением, союзниками и угрозами, не возвращаясь на корабль.

## ✨ Особенности
* **Постоянная работа:** Камера радара активна всегда, даже если вы глубоко в комплексе.
* **Переключение видимости:** Вы можете скрывать или показывать миникарту по желанию.
* **Сохранение состояния:** Мод запоминает ваш выбор. Если вы выключили карту, она останется выключенной при следующем запуске игры.
* **Выключено по умолчанию:** При первом запуске карта скрыта. Нажмите `F2`, чтобы включить её.
* **Авто-поворот:** Карта вращается вслед за направлением взгляда вашего персонажа.
* **Умные иконки:** Маркеры игроков и объектов сохраняют правильную ориентацию при повороте карты.
* **Блокировка цели (Override):** Позволяет зафиксировать камеру на определенном объекте.
* **Ручное переключение:** Листайте цели радара прямо на ходу.

## 🎮 Управление
| Действие | Клавиша | Описание |
| :--- | :--- | :--- |
| **Вкл/Выкл карту** | `F2` | Скрыть/показать миникарту (настройка сохраняется) |
| **Блокировка (Override)** | `F3` | Фиксирует текущую цель |
| **Смена цели** | `F4` | Вручную переключает радар на следующий объект |

## 🛠️ Установка
1. Установите [BepInEx Pack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/).
2. Скачайте файл **MIniMap.dll**.
3. Поместите файл в папку `Lethal Company/BepInEx/plugins`.
4. Запустите игру один раз, чтобы создался файл конфигурации в `BepInEx/config/com.diman3012.minimap.cfg`.

</details>

---

### 🏗️ Technical Details
* **Namespace:** `MIniMap`
* **Configuration:** Uses BepInEx `Config.Bind` for persistent settings.
* **Hooks:** Patches `PlayerControllerB` for UI and `ManualCameraRenderer` for logic.
* **Network Sync:** Includes a `NetworkPrefabPatch` for client identification using `Unity.Netcode`.

Created by [Diman3012](https://github.com/Diman3012)