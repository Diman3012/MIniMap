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
            // Проверка на локального игрока
            if (!__instance.IsOwner || __instance != GameNetworkManager.Instance.localPlayerController) return;

            // Позволяем коду работать, если игрок мертв или контролируется
            if (!__instance.isPlayerControlled && !__instance.isPlayerDead) return;

            // F2 - Вкл/Выкл миникарты
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

            // F3 - Freeze / Override
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.OverrideKey))
            {
                MinimalMinimap.Data.FreezeTarget = !MinimalMinimap.Data.FreezeTarget;
                HUDManager.Instance.DisplayTip("Minimap",
                    MinimalMinimap.Data.FreezeTarget ? "Override ON" : "Override OFF");
            }

            // F4 - Переключение вручную
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey))
            {
                if (MinimalMinimap.Data.FreezeTarget)
                    SwitchTarget();
            }

            // Логика слежения за целью спектатора после смерти
            if (__instance.isPlayerDead && __instance.spectatedPlayerScript != null)
            {
                // Если ручное управление выключено, синхронизируем карту с тем, на кого смотрим
                if (!MinimalMinimap.Data.FreezeTarget)
                {
                    var map = StartOfRound.Instance.mapScreen;
                    if (map != null && map.targetedPlayer != __instance.spectatedPlayerScript)
                    {
                        for (int i = 0; i < map.radarTargets.Count; i++)
                        {
                            var t = map.radarTargets[i];
                            if (t != null && t.transform != null)
                            {
                                PlayerControllerB p = t.transform.GetComponent<PlayerControllerB>();
                                if (p == __instance.spectatedPlayerScript)
                                {
                                    map.targetTransformIndex = i;
                                    map.targetedPlayer = p;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void SwitchTarget()
        {
            var map = StartOfRound.Instance.mapScreen;
            if (map == null || map.radarTargets == null) return;

            int count = map.radarTargets.Count;
            int next = map.targetTransformIndex;

            for (int i = 0; i < count; i++)
            {
                next = (next + 1) % count;

                var t = map.radarTargets[next];
                if (t == null || t.transform == null) continue;

                PlayerControllerB player = t.transform.GetComponent<PlayerControllerB>();

                // Фильтруем "фантомных" игроков (неактивные слоты)
                if (player != null && !player.isPlayerControlled && !player.isPlayerDead)
                    continue;

                // Устанавливаем цель (включая самого себя и радар-бустеры)
                map.targetTransformIndex = next;
                map.targetedPlayer = player;

                return;
            }
        }

        // Блокируем автообновление только когда Freeze включен
        [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
        [HarmonyPrefix]
        private static bool PreventAutoUpdate()
        {
            if (MinimalMinimap.Data.FreezeTarget)
                return false;

            return true;
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetForward")]
        [HarmonyPrefix]
        private static bool PreventForwardSwitch()
        {
            return !MinimalMinimap.Data.FreezeTarget;
        }
    }
}