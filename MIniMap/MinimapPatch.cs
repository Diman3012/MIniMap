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

        // Переменные для независимой камеры
        public static Camera minimapCamera;
        public static RenderTexture minimapTexture;
        private static bool cameraInitialized;
        private static int framesSinceLanded;

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

            // Инициализация собственной камеры вместо использования камеры корабля
            InitializeCamera();

            minimapObject.transform.SetParent(HUDManager.Instance.playerScreenTexture.transform, false);

            bool isEnabled = MinimalMinimap.Instance.ConfigEnabled.Value;
            minimapObject.SetActive(isEnabled);
        }

        // НОВЫЙ МЕТОД: Создание независимой текстуры и камеры
        private static void InitializeCamera()
        {
            if (StartOfRound.Instance.mapScreen != null && StartOfRound.Instance.mapScreen.cam != null)
            {
                if (minimapCamera != null)
                {
                    Object.Destroy(minimapCamera.gameObject);
                }
                if (minimapTexture != null)
                {
                    minimapTexture.Release();
                }

                GameObject obj = Object.Instantiate(StartOfRound.Instance.mapScreen.cam.gameObject, StartOfRound.Instance.mapScreen.cam.transform.parent);
                obj.name = "MinimapCamera";
                obj.SetActive(true);

                minimapCamera = obj.GetComponent<Camera>();
                minimapCamera.enabled = true;

                minimapTexture = new RenderTexture(480, 480, 24);
                minimapTexture.name = "MinimapRenderTexture";
                minimapTexture.Create();

                minimapCamera.targetTexture = minimapTexture;
                minimapImage.texture = minimapTexture;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void HandleHotkeys(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || __instance != GameNetworkManager.Instance.localPlayerController) return;

            // Блокировка ввода, если игрок в чате или в терминале
            bool isTyping = __instance.isTypingChat || __instance.inTerminalMenu;

            // F2 - Вкл/Выкл самой миникарты
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.ToggleKey) && !isTyping)
            {
                // Переключаем состояние
                bool newState = !MinimalMinimap.Instance.ConfigEnabled.Value;
                MinimalMinimap.Instance.ConfigEnabled.Value = newState;

                // Выводим отчет на экран с помощью HUDManager (как вы и просили)
                HUDManager.Instance?.DisplayTip("Minimal Minimap", newState ? "Enabled" : "Disabled");

                // Включаем или выключаем сам UI миникарты
                if (minimapObject != null)
                {
                    minimapObject.SetActive(newState);
                }

                // Если карту выключили, отключаем и камеру, чтобы она не тратила ресурсы
                if (!newState && minimapCamera != null)
                {
                    minimapCamera.enabled = false;
                }
            }

            if (!MinimalMinimap.Instance.ConfigEnabled.Value) return;

            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey) && !isTyping)
            {
                SwitchTarget();
                InitializeCamera(); // Пересоздаем камеру для надежности при смене цели
            }

            // Логика зума
            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.ZoomKey) && !isTyping)
            {
                MinimalMinimap.Data.currentZoomIndex = (MinimalMinimap.Data.currentZoomIndex + 1) % MinimalMinimap.Data.ZoomLevels.Length;
                MinimalMinimap.Data.Zoom = MinimalMinimap.Data.ZoomLevels[MinimalMinimap.Data.currentZoomIndex];
            }

            if (__instance.isPlayerDead)
            {
                if (MinimalMinimap.Data.FreezeTarget)
                    MinimalMinimap.Data.FreezeTarget = false;

                if (__instance.spectatedPlayerScript != null)
                    SetMapTargetToPlayer(__instance.spectatedPlayerScript);
            }
            else if (!MinimalMinimap.Data.FreezeTarget)
            {
                MinimalMinimap.Data.FreezeTarget = true;
                SetMapTargetToPlayer(__instance);
            }

            // Если цель сбросилась - возвращаем на себя
            if (MinimalMinimap.CustomTarget == null)
            {
                SetMapTargetToPlayer(__instance);
            }

            // Обновляем позицию камеры каждый кадр
            UpdateMinimapCamera();
        }

        // НОВЫЙ МЕТОД: Управление позицией и вращением камеры
        private static void UpdateMinimapCamera()
        {
            if (minimapCamera == null && !cameraInitialized && StartOfRound.Instance.mapScreen != null && StartOfRound.Instance.mapScreen.cam != null && !StartOfRound.Instance.inShipPhase)
            {
                framesSinceLanded++;
                if (framesSinceLanded > 100 || StartOfRound.Instance.mapScreen.cam.enabled)
                {
                    InitializeCamera();
                    cameraInitialized = true;
                }
            }

            if (minimapCamera == null || MinimalMinimap.CustomTarget == null)
                return;

            if (StartOfRound.Instance.inShipPhase)
            {
                minimapCamera.enabled = false;
                if (minimapImage != null) minimapImage.enabled = false;
                return;
            }

            if (minimapImage != null) minimapImage.enabled = true;

            minimapCamera.gameObject.SetActive(true);
            minimapCamera.enabled = true;
            minimapCamera.transform.position = MinimalMinimap.CustomTarget.transform.position + Vector3.up * 3.636f;

            if (StartOfRound.Instance.mapScreen != null && StartOfRound.Instance.mapScreen.cam != null)
            {
                minimapCamera.nearClipPlane = StartOfRound.Instance.mapScreen.cam.nearClipPlane;
                minimapCamera.farClipPlane = StartOfRound.Instance.mapScreen.cam.farClipPlane;
            }

            minimapCamera.orthographicSize = MinimalMinimap.Data.Zoom;
            Vector3 defaultEuler = new Vector3(90f, 0f, 0f);

            if (MinimalMinimap.Data.AutoRotate)
            {
                minimapCamera.transform.eulerAngles = new Vector3(defaultEuler.x, MinimalMinimap.CustomTarget.transform.eulerAngles.y, defaultEuler.z);
            }
            else
            {
                minimapCamera.transform.eulerAngles = defaultEuler;
            }

            TerminalAccessibleObject[] array = Object.FindObjectsOfType<TerminalAccessibleObject>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].mapRadarObject != null)
                {
                    array[i].mapRadarObject.transform.eulerAngles = new Vector3(defaultEuler.x, minimapCamera.transform.eulerAngles.y, defaultEuler.z);
                }
            }
        }

        // ОБНОВЛЕННЫЙ МЕТОД: Работает только с CustomTarget, не трогая радар корабля
        private static void SetMapTargetToPlayer(PlayerControllerB target)
        {
            var map = StartOfRound.Instance.mapScreen;
            if (map == null || target == null || MinimalMinimap.CustomTarget == target) return;

            for (int i = 0; i < map.radarTargets.Count; i++)
            {
                var t = map.radarTargets[i];
                if (t != null && t.transform != null)
                {
                    if (t.transform.GetComponent<PlayerControllerB>() == target)
                    {
                        MinimalMinimap.CustomTargetIndex = i;
                        MinimalMinimap.CustomTarget = target;
                        break;
                    }
                }
            }
        }

        // ОБНОВЛЕННЫЙ МЕТОД: Работает только с CustomTarget
        private static void SwitchTarget()
        {
            var map = StartOfRound.Instance.mapScreen;
            if (map == null || map.radarTargets == null || map.radarTargets.Count == 0)
                return;

            int count = map.radarTargets.Count;
            int next = MinimalMinimap.CustomTargetIndex;

            for (int i = 0; i < count; i++)
            {
                next = (next + 1) % count;

                var t = map.radarTargets[next];
                if (t == null || t.transform == null) continue;

                PlayerControllerB player = t.transform.GetComponent<PlayerControllerB>();

                if (player == null) continue;

                if (!player.isPlayerControlled && !player.isPlayerDead)
                    continue;

                MinimalMinimap.CustomTargetIndex = next;
                MinimalMinimap.CustomTarget = player;

                return;
            }
        }
    }
}