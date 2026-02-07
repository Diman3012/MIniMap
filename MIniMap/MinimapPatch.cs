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
            if (minimapObject != null)
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

            bool isEnabled = MinimalMinimap.Instance.ConfigEnabled.Value;
            minimapObject.SetActive(isEnabled);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void HandleHotkeys(PlayerControllerB __instance)
        {
            // Проверка: выполняем логику только для локального игрока, которым мы управляем
            if (!__instance.IsOwner || !__instance.isPlayerControlled) return;

            // Дополнительная проверка на сетевой менеджер (защита от багов)
            if (__instance != GameNetworkManager.Instance.localPlayerController) return;

            // F2 - Вкл/Выкл отображение миникарты
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.ToggleKey))
            {
                bool newState = !MinimalMinimap.Instance.ConfigEnabled.Value;
                MinimalMinimap.Instance.ConfigEnabled.Value = newState;

                if (minimapObject != null)
                {
                    minimapObject.SetActive(newState);
                    HUDManager.Instance.DisplayTip("Minimap", newState ? "Visible" : "Hidden");
                }
            }

            if (!MinimalMinimap.Instance.ConfigEnabled.Value) return;

            // F3 - Переключение режима блокировки
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

        private static void SwitchTarget()
        {
            ManualCameraRenderer map = StartOfRound.Instance.mapScreen;
            if (map == null || map.radarTargets == null) return;

            int nextIndex = (map.targetTransformIndex + 1) % map.radarTargets.Count;
            map.SwitchRadarTargetAndSync(nextIndex);
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
        [HarmonyPrefix]
        private static bool PreventAutoUpdate(ManualCameraRenderer __instance)
        {
            return !MinimalMinimap.Data.FreezeTarget;
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetForward")]
        [HarmonyPrefix]
        private static bool PreventForwardSwitch()
        {
            return !MinimalMinimap.Data.FreezeTarget;
        }
    }
}