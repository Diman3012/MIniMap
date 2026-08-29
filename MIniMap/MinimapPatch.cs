using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace MIniMap
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MinimapPatch
    {
        private static GameObject minimapObject;
        private static RawImage minimapImage;

        public static Camera minimapCamera;
        public static RenderTexture minimapTexture;
        private static bool cameraInitialized;
        private static int framesSinceLanded;

        private static float f2HoldTimer = 0f;
        private static bool f2HoldTriggered = false;
        private static bool wasEditMode = false;

        private static bool isDragging = false;
        private static bool isResizing = false;
        private static Vector2 dragStartMousePos;
        private static Vector2 initialAnchoredPos;
        private static float initialSize;

        [HarmonyPatch("ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void CreateMinimap()
        {
            if (minimapObject != null) return;

            minimapObject = new GameObject("MIniMap_UI");
            minimapImage = minimapObject.AddComponent<RawImage>();

            RectTransform rt = minimapImage.rectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f); // Пивот в правом верхнем углу
            rt.sizeDelta = new Vector2(MinimalMinimap.Data.Size, MinimalMinimap.Data.Size);
            rt.anchoredPosition = new Vector2(MinimalMinimap.Data.XOffset, MinimalMinimap.Data.YOffset);

            InitializeCamera();

            minimapObject.transform.SetParent(HUDManager.Instance.playerScreenTexture.transform, false);
            minimapObject.SetActive(MinimalMinimap.Instance.ConfigEnabled.Value);
        }

        private static void InitializeCamera()
        {
            if (StartOfRound.Instance.mapScreen != null && StartOfRound.Instance.mapScreen.cam != null)
            {
                if (minimapCamera != null) Object.Destroy(minimapCamera.gameObject);
                if (minimapTexture != null) minimapTexture.Release();

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

            bool isTyping = __instance.isTypingChat || __instance.inTerminalMenu;

            if (UnityInput.Current.GetKey(MinimalMinimap.Data.ToggleKey) && !isTyping)
            {
                f2HoldTimer += Time.deltaTime;
                if (f2HoldTimer >= 2.0f && !f2HoldTriggered)
                {
                    f2HoldTriggered = true;
                    MinimalMinimap.Data.IsEditMode = !MinimalMinimap.Data.IsEditMode;

                    HUDManager.Instance?.DisplayTip(
                        "Minimap Edit Mode",
                        MinimalMinimap.Data.IsEditMode ? "ENABLED (Drag/Resize)" : "DISABLED"
                    );
                }
            }

            if (UnityInput.Current.GetKeyUp(MinimalMinimap.Data.ToggleKey) && !isTyping)
            {
                if (f2HoldTriggered)
                {
                    f2HoldTriggered = false;
                }
                else if (!MinimalMinimap.Data.IsEditMode)
                {
                    bool newState = !MinimalMinimap.Instance.ConfigEnabled.Value;
                    MinimalMinimap.Instance.ConfigEnabled.Value = newState;

                    HUDManager.Instance?.DisplayTip("Minimal Minimap", newState ? "Enabled" : "Disabled");

                    if (minimapObject != null) minimapObject.SetActive(newState);
                    if (!newState && minimapCamera != null) minimapCamera.enabled = false;
                }
                f2HoldTimer = 0f;
            }

            if (MinimalMinimap.Data.IsEditMode != wasEditMode)
            {
                wasEditMode = MinimalMinimap.Data.IsEditMode;
                if (!wasEditMode)
                {
                    __instance.disableLookInput = false;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            // Логика перемещения и изменения размера с учетом UI Canvas
            if (MinimalMinimap.Data.IsEditMode && minimapObject != null && minimapImage != null && Mouse.current != null)
            {
                __instance.disableLookInput = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                RectTransform rt = minimapImage.rectTransform;
                Canvas canvas = minimapImage.canvas;
                Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                Vector2 mousePosScreen = Mouse.current.position.ReadValue();

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    // Проверяем попадание курсора в RectTransform с учетом нужной камеры
                    if (UnityEngine.RectTransformUtility.RectangleContainsScreenPoint(rt, mousePosScreen, uiCamera))
                    {
                        dragStartMousePos = mousePosScreen;
                        initialAnchoredPos = rt.anchoredPosition;
                        initialSize = MinimalMinimap.Data.Size;

                        UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePosScreen, uiCamera, out Vector2 localPoint);

                        // Так как пивот (1,1), координаты localPoint идут от -Width до 0.
                        float margin = 40f;
                        bool nearEdge = (localPoint.x < -rt.rect.width + margin || localPoint.x > -margin ||
                                         localPoint.y < -rt.rect.height + margin || localPoint.y > -margin);

                        if (nearEdge)
                        {
                            isResizing = true;
                            isDragging = false;
                        }
                        else
                        {
                            isDragging = true;
                            isResizing = false;
                        }
                    }
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    float scaleFactor = canvas.scaleFactor > 0 ? canvas.scaleFactor : 1f;
                    Vector2 delta = mousePosScreen - dragStartMousePos;

                    if (isDragging)
                    {
                        rt.anchoredPosition = initialAnchoredPos + (delta / scaleFactor);
                        MinimalMinimap.Data.XOffset = rt.anchoredPosition.x;
                        MinimalMinimap.Data.YOffset = rt.anchoredPosition.y;
                    }
                    else if (isResizing)
                    {
                        // Движение мыши влево-вниз увеличивает карту (т.к. якорь в правом верхнем углу)
                        float deltaSize = -(delta.x + delta.y) / 2f / scaleFactor;
                        float newSize = Mathf.Clamp(initialSize + deltaSize, 80f, 800f);

                        rt.sizeDelta = new Vector2(newSize, newSize);
                        MinimalMinimap.Data.Size = Mathf.RoundToInt(newSize);
                    }
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    isDragging = false;
                    isResizing = false;
                }
            }

            if (!MinimalMinimap.Instance.ConfigEnabled.Value) return;

            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey) && !isTyping)
            {
                SwitchTarget();
                InitializeCamera();
            }

            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.ZoomKey) && !isTyping)
            {
                MinimalMinimap.Data.currentZoomIndex = (MinimalMinimap.Data.currentZoomIndex + 1) % MinimalMinimap.Data.ZoomLevels.Length;
                MinimalMinimap.Data.Zoom = MinimalMinimap.Data.ZoomLevels[MinimalMinimap.Data.currentZoomIndex];
            }

            if (__instance.isPlayerDead)
            {
                if (MinimalMinimap.Data.FreezeTarget) MinimalMinimap.Data.FreezeTarget = false;
                if (__instance.spectatedPlayerScript != null) SetMapTargetToPlayer(__instance.spectatedPlayerScript);
            }
            else if (!MinimalMinimap.Data.FreezeTarget)
            {
                MinimalMinimap.Data.FreezeTarget = true;
                SetMapTargetToPlayer(__instance);
            }

            if (MinimalMinimap.CustomTarget == null)
            {
                SetMapTargetToPlayer(__instance);
            }

            UpdateMinimapCamera();
        }

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

            if (minimapCamera == null || MinimalMinimap.CustomTarget == null) return;

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

        private static void SwitchTarget()
        {
            var map = StartOfRound.Instance.mapScreen;
            if (map == null || map.radarTargets == null || map.radarTargets.Count == 0) return;

            int count = map.radarTargets.Count;
            int next = MinimalMinimap.CustomTargetIndex;

            for (int i = 0; i < count; i++)
            {
                next = (next + 1) % count;

                var t = map.radarTargets[next];
                if (t == null || t.transform == null) continue;

                PlayerControllerB player = t.transform.GetComponent<PlayerControllerB>();

                if (player == null) continue;
                if (!player.isPlayerControlled && !player.isPlayerDead) continue;

                MinimalMinimap.CustomTargetIndex = next;
                MinimalMinimap.CustomTarget = player;

                return;
            }
        }
    }
}