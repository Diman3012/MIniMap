using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MIniMap
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MinimapPatch
    {
        private static GameObject minimapObject;
        private static RawImage minimapImage;

        [HarmonyPatch("ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void CreateMinimap()
        {
            if (!MinimalMinimap.Data.Enabled || minimapObject != null)
                return;

            minimapObject = new GameObject("MIniMap_UI");
            minimapImage = minimapObject.AddComponent<RawImage>();

            RectTransform rt = minimapImage.rectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(MinimalMinimap.Data.Size, MinimalMinimap.Data.Size);
            rt.anchoredPosition = new Vector2(MinimalMinimap.Data.XOffset, MinimalMinimap.Data.YOffset);

            if (StartOfRound.Instance.mapScreen != null)
            {
                minimapImage.texture = StartOfRound.Instance.mapScreen.cam.targetTexture;
            }

            minimapObject.transform.SetParent(HUDManager.Instance.playerScreenTexture.transform, false);
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void HandleHotkeys(PlayerControllerB __instance)
        {
            if (!__instance.isPlayerControlled || !MinimalMinimap.Data.Enabled) return;

            // F2 - Вкл/Выкл отображение миникарты
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.ToggleKey))
            {
                if (minimapObject != null)
                {
                    bool newState = !minimapObject.activeSelf;
                    minimapObject.SetActive(newState);

                    // Опционально: выводим подсказку, как при F3
                    HUDManager.Instance.DisplayTip("Minimap", newState ? "Visible" : "Hidden");
                }
            }

            // F3 - Переключение режима блокировки (Override)
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.OverrideKey))
            {
                MinimalMinimap.Data.FreezeTarget = !MinimalMinimap.Data.FreezeTarget;
                HUDManager.Instance.DisplayTip("Minimap", MinimalMinimap.Data.FreezeTarget ? "Override ON" : "Override OFF");
            }

            // F4 - Ручное переключение цели
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey))
            {
                SwitchTarget();
            }
        }

        // Логика переключения цели из референса
        private static void SwitchTarget()
        {
            ManualCameraRenderer map = StartOfRound.Instance.mapScreen;
            if (map == null || map.radarTargets == null) return;

            int nextIndex = (map.targetTransformIndex + 1) % map.radarTargets.Count;

            // Используем встроенный метод игры для синхронизации
            map.SwitchRadarTargetAndSync(nextIndex);
        }

        // ПАТЧИ ДЛЯ БЛОКИРОВКИ АВТО-ПЕРЕКЛЮЧЕНИЯ

        [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
        [HarmonyPrefix]
        private static bool PreventAutoUpdate(ManualCameraRenderer __instance)
        {
            // Если включена блокировка (F3), запрещаем игре менять цель автоматически
            return !MinimalMinimap.Data.FreezeTarget;
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetForward")]
        [HarmonyPrefix]
        private static bool PreventForwardSwitch()
        {
            // Запрещает переключение при нажатии кнопки на панели в корабле, если включен Freeze
            return !MinimalMinimap.Data.FreezeTarget;
        }
    }
}