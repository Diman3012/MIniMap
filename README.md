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
* **Auto-Rotate:** The map aligns with your character's view direction for intuitive navigation.
* **Smart Icons:** Player markers and terminal objects (turrets, mines, doors) remain correctly oriented and don't "flip" when the camera rotates.
* **Target Locking (Override):** Prevent the game or other players from changing your radar target automatically.
* **Manual Switching:** Quickly cycle through radar targets directly from your HUD.

## 🎮 Controls
| Action | Key | Description |
| :--- | :--- | :--- |
| **Toggle Override** | `F3` | Locks the current target (Prevents auto-switching) |
| **Switch Target** | `F4` | Manually cycles to the next radar target |

## 🛠️ Installation
1. Install [BepInEx Pack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/).
2. Download the **MIniMap.dll**.
3. Place the file into `Lethal Company/BepInEx/plugins`.

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
* **Авто-поворот:** Карта вращается вслед за направлением взгляда вашего персонажа.
* **Умные иконки:** Маркеры игроков и объектов (турели, мины, двери) сохраняют правильную ориентацию и не "кувыркаются" при повороте карты.
* **Блокировка цели (Override):** Позволяет зафиксировать камеру на определенном объекте, запрещая игре или другим игрокам менять вашу цель.
* **Ручное переключение:** Листайте цели радара прямо на ходу.

## 🎮 Управление
| Действие | Клавиша | Описание |
| :--- | :--- | :--- |
| **Блокировка (Override)** | `F3` | Фиксирует текущую цель (защита от авто-переключения) |
| **Смена цели** | `F4` | Вручную переключает радар на следующий объект |

## 🛠️ Установка
1. Установите [BepInEx Pack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/).
2. Скачайте файл **MIniMap.dll**.
3. Поместите файл в папку `Lethal Company/BepInEx/plugins`.

</details>

---

### 🏗️ Technical Details
* **Namespace:** `MIniMap`
* **Hooks:** Patches `PlayerControllerB` for UI and `ManualCameraRenderer` for rotation/target logic.
* **Network Sync:** Includes a `NetworkPrefabPatch` to ensure identification across clients using `Unity.Netcode`.

Created by [Diman3012](https://github.com/Diman3012)
