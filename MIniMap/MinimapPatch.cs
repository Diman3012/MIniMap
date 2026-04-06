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
            if (!__instance.IsOwner || __instance != GameNetworkManager.Instance.localPlayerController) return;
            if (!__instance.isPlayerControlled && !__instance.isPlayerDead) return;

            // F2 - Вкл/Выкл самой миникарты
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.ToggleKey))
            {
                bool newState = !MinimalMinimap.Instance.ConfigEnabled.Value;
                MinimalMinimap.Instance.ConfigEnabled.Value = newState;
                if (minimapObject != null) minimapObject.SetActive(newState);
            }

            if (!MinimalMinimap.Instance.ConfigEnabled.Value) return;

            // Кнопка F3 больше не переключает режим, так как он всегда ON.
            // Но мы оставляем логику F4 для ручного переключения целей.
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey))
            {
                // Мы убрали проверку if (FreezeTarget), так как теперь мы всегда в этом режиме
                SwitchTarget();
            }

            // Логика поведения при смерти и возрождении
            if (__instance.isPlayerDead)
            {
                // Если умерли — выключаем заморозку, чтобы следить за живыми
                if (MinimalMinimap.Data.FreezeTarget)
                    MinimalMinimap.Data.FreezeTarget = false;

                if (__instance.spectatedPlayerScript != null)
                    SetMapTargetToPlayer(__instance.spectatedPlayerScript);
            }
            else
            {
                // Если возродились, а заморозка еще выключена — включаем обратно и центрируем на себе
                if (!MinimalMinimap.Data.FreezeTarget)
                {
                    MinimalMinimap.Data.FreezeTarget = true;
                    SetMapTargetToPlayer(__instance);
                }
            }
        }

        // Вспомогательный метод для поиска игрока (используется при смерти/возрождении)
        private static void SetMapTargetToPlayer(PlayerControllerB target)
        {
            var map = StartOfRound.Instance.mapScreen;
            if (map == null || target == null || map.targetedPlayer == target) return;

            for (int i = 0; i < map.radarTargets.Count; i++)
            {
                var t = map.radarTargets[i];
                if (t != null && t.transform != null)
                {
                    if (t.transform.GetComponent<PlayerControllerB>() == target)
                    {
                        map.targetTransformIndex = i;
                        map.targetedPlayer = target;
                        break;
                    }
                }
            }
        }

        private static void SwitchTarget()
        {
            var map = StartOfRound.Instance.mapScreen;
            if (map == null || map.radarTargets == null || map.radarTargets.Count == 0)
                return;

            int count = map.radarTargets.Count;
            int next = map.targetTransformIndex;

            for (int i = 0; i < count; i++)
            {
                next = (next + 1) % count;

                var t = map.radarTargets[next];
                if (t == null || t.transform == null) continue;

                PlayerControllerB player = t.transform.GetComponent<PlayerControllerB>();

                // Пропускаем мусор
                if (player == null) continue;

                // 👇 ВАЖНО: фильтр живых/валидных
                if (!player.isPlayerControlled && !player.isPlayerDead)
                    continue;

                map.targetTransformIndex = next;
                map.targetedPlayer = player;

                return;
            }
        }

        private static bool allowOneUpdate = false;

        [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
        [HarmonyPrefix]
        private static bool PreventAutoUpdate()
        {
            if (allowOneUpdate)
            {
                allowOneUpdate = false;
                return true;
            }

            return !MinimalMinimap.Data.FreezeTarget;
        }

        [HarmonyPatch(typeof(ManualCameraRenderer), "SwitchRadarTargetForward")]
        [HarmonyPrefix]
        private static bool BlockOriginalSwitch()
        {
            SwitchTarget(); // твоя логика
            return false;   // полностью блокируем игру
        }
    }
}