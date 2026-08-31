using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MIniMap
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class MinimapPatch
    {
        private static GameObject minimapObject;
        private static RawImage minimapImage;

        // CUBE-X UI элементы
        private static GameObject zoomCardObj;
        private static RectTransform zoomCardRt;
        private static RectTransform trackRt;
        private static RectTransform fillRt;
        private static RectTransform handleRt;
        private static TextMeshProUGUI valText;

        private static float f2HoldTimer = 0f;
        private static bool f2HoldTriggered = false;
        private static bool wasEditMode = false;

        private static bool isDraggingMap = false;
        private static bool isResizingMap = false;
        private static bool isDraggingSlider = false;

        private enum ResizeCorner { None, TopLeft, TopRight, BottomLeft, BottomRight }
        private static ResizeCorner activeCorner = ResizeCorner.None;

        private static bool tooltipInitialized = false;
        private static float originalTooltipY;

        // Генератор округлых текстур для UI в стиле CUBE-X
        private static Sprite CreateRoundedSprite(int width, int height, int radius, Color color)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = x < radius ? radius - x : (x >= width - radius ? x - (width - radius - 1) : 0);
                    float cy = y < radius ? radius - y : (y >= height - radius ? y - (height - radius - 1) : 0);
                    float distSq = cx * cx + cy * cy;

                    if (distSq > radius * radius)
                    {
                        colors[y * width + x] = Color.clear;
                    }
                    else
                    {
                        colors[y * width + x] = color;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        [HarmonyPatch("ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        private static void CreateMinimap()
        {
            if (minimapObject != null)
                return;

            minimapObject = new GameObject("MIniMap_UI");
            minimapImage = minimapObject.AddComponent<RawImage>();
            minimapImage.raycastTarget = false;

            RectTransform rt = minimapImage.rectTransform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(MinimalMinimap.Data.Size, MinimalMinimap.Data.Size);
            rt.anchoredPosition = new Vector2(MinimalMinimap.Data.XOffset, MinimalMinimap.Data.YOffset);

            minimapObject.transform.SetParent(HUDManager.Instance.playerScreenTexture.transform, false);

            CreateCubeXZoomUI();
            UpdateMinimapVisibility();
        }

        private static void CreateCubeXZoomUI()
        {
            TMP_FontAsset fontAsset = (HUDManager.Instance != null && HUDManager.Instance.controlTipLines != null && HUDManager.Instance.controlTipLines.Length > 0)
                ? HUDManager.Instance.controlTipLines[0].font
                : null;

            Sprite cardBgSprite = CreateRoundedSprite(120, 240, 12, new Color(0.094f, 0.106f, 0.157f, 0.95f));
            Sprite trackBgSprite = CreateRoundedSprite(24, 180, 12, new Color(0.063f, 0.071f, 0.102f, 1f));
            Sprite fillSprite = CreateRoundedSprite(24, 180, 12, new Color(0.529f, 0.761f, 1f, 1f));
            Sprite handleSprite = CreateRoundedSprite(32, 16, 6, new Color(1f, 1f, 1f, 1f));
            Sprite dividerSprite = CreateRoundedSprite(100, 4, 2, new Color(0.282f, 0.549f, 0.941f, 1f));

            // Главная карточка слева от карты
            zoomCardObj = new GameObject("CUBEX_Zoom_Card");
            zoomCardObj.transform.SetParent(minimapObject.transform, false);

            zoomCardRt = zoomCardObj.AddComponent<RectTransform>();
            zoomCardRt.anchorMin = new Vector2(0f, 0.5f);
            zoomCardRt.anchorMax = new Vector2(0f, 0.5f);
            zoomCardRt.pivot = new Vector2(1f, 0.5f);
            zoomCardRt.sizeDelta = new Vector2(54f, 210f);
            zoomCardRt.anchoredPosition = new Vector2(-10f, 0f);

            Image cardImg = zoomCardObj.AddComponent<Image>();
            cardImg.sprite = cardBgSprite;
            cardImg.type = Image.Type.Sliced;
            cardImg.raycastTarget = false;

            // Заголовок "Zoom"
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(zoomCardObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) titleText.font = fontAsset;
            titleText.text = "Zoom";
            titleText.fontSize = 13;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.353f, 0.635f, 1f, 1f);
            titleText.fontStyle = FontStyles.Bold;
            titleText.raycastTarget = false;

            RectTransform titleRt = titleText.rectTransform;
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -6f);
            titleRt.sizeDelta = new Vector2(0f, 18f);

            // Разделительная полоска под заголовком
            GameObject dividerObj = new GameObject("Divider");
            dividerObj.transform.SetParent(zoomCardObj.transform, false);
            Image dividerImg = dividerObj.AddComponent<Image>();
            dividerImg.sprite = dividerSprite;
            dividerImg.type = Image.Type.Sliced;
            dividerImg.raycastTarget = false;

            RectTransform divRt = dividerObj.GetComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0.1f, 1f);
            divRt.anchorMax = new Vector2(0.9f, 1f);
            divRt.pivot = new Vector2(0.5f, 1f);
            divRt.anchoredPosition = new Vector2(0f, -26f);
            divRt.sizeDelta = new Vector2(0f, 3f);

            // Траектория слайдера (Track)
            GameObject trackObj = new GameObject("Track");
            trackObj.transform.SetParent(zoomCardObj.transform, false);
            trackRt = trackObj.AddComponent<RectTransform>();
            trackRt.anchorMin = new Vector2(0.5f, 0.5f);
            trackRt.anchorMax = new Vector2(0.5f, 0.5f);
            trackRt.pivot = new Vector2(0.5f, 0.5f);
            trackRt.sizeDelta = new Vector2(16f, 135f);
            trackRt.anchoredPosition = new Vector2(0f, -5f);

            Image trackImg = trackObj.AddComponent<Image>();
            trackImg.sprite = trackBgSprite;
            trackImg.type = Image.Type.Sliced;
            trackImg.raycastTarget = false;

            // Заполнение (Fill)
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(trackObj.transform, false);
            fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 0f);
            fillRt.pivot = new Vector2(0.5f, 0f);
            fillRt.sizeDelta = new Vector2(0f, 0f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.raycastTarget = false;

            // Ползуночек (Handle)
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(trackObj.transform, false);
            handleRt = handleObj.AddComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0.5f, 0f);
            handleRt.anchorMax = new Vector2(0.5f, 0f);
            handleRt.pivot = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(24f, 10f);

            Image handleImg = handleObj.AddComponent<Image>();
            handleImg.sprite = handleSprite;
            handleImg.type = Image.Type.Sliced;
            handleImg.raycastTarget = false;

            // Значение Zoom снизу
            GameObject valObj = new GameObject("ValueText");
            valObj.transform.SetParent(zoomCardObj.transform, false);
            valText = valObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) valText.font = fontAsset;
            valText.text = "20m";
            valText.fontSize = 10;
            valText.alignment = TextAlignmentOptions.Center;
            valText.color = Color.white;
            valText.raycastTarget = false;

            RectTransform valRt = valText.rectTransform;
            valRt.anchorMin = new Vector2(0f, 0f);
            valRt.anchorMax = new Vector2(1f, 0f);
            valRt.pivot = new Vector2(0.5f, 0f);
            valRt.anchoredPosition = new Vector2(0f, 4f);
            valRt.sizeDelta = new Vector2(0f, 14f);

            zoomCardObj.SetActive(false);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void HandleHotkeys(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner || __instance != GameNetworkManager.Instance.localPlayerController) return;
            if (!__instance.isPlayerControlled && !__instance.isPlayerDead) return;

            bool isTyping = __instance.isTypingChat || __instance.inTerminalMenu;

            // F2: Режим редактирования
            if (Keyboard.current != null && Keyboard.current[Key.F2].isPressed && !isTyping)
            {
                f2HoldTimer += Time.deltaTime;
                if (f2HoldTimer >= 1.0f && !f2HoldTriggered)
                {
                    f2HoldTriggered = true;
                    MinimalMinimap.Data.IsEditMode = !MinimalMinimap.Data.IsEditMode;
                    HUDManager.Instance?.DisplayTip("Minimap Edit Mode", MinimalMinimap.Data.IsEditMode ? "ON: Drag, Resize & CUBE-X Zoom" : "OFF");
                }
            }

            if (Keyboard.current != null && Keyboard.current[Key.F2].wasReleasedThisFrame && !isTyping)
            {
                if (!f2HoldTriggered)
                {
                    bool newState = !MinimalMinimap.Instance.ConfigEnabled.Value;
                    MinimalMinimap.Instance.ConfigEnabled.Value = newState;
                }
                f2HoldTriggered = false;
                f2HoldTimer = 0f;
            }

            UpdateMinimapVisibility();

            if (MinimalMinimap.Data.IsEditMode != wasEditMode)
            {
                wasEditMode = MinimalMinimap.Data.IsEditMode;
                if (!wasEditMode)
                {
                    __instance.disableLookInput = false;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;

                    if (minimapImage != null)
                    {
                        MinimalMinimap.Data.XOffset = minimapImage.rectTransform.anchoredPosition.x;
                        MinimalMinimap.Data.YOffset = minimapImage.rectTransform.anchoredPosition.y;
                        MinimalMinimap.Data.Size = Mathf.RoundToInt(minimapImage.rectTransform.sizeDelta.x);

                        MinimalMinimap.Instance.ConfigXOffset.Value = MinimalMinimap.Data.XOffset;
                        MinimalMinimap.Instance.ConfigYOffset.Value = MinimalMinimap.Data.YOffset;
                        MinimalMinimap.Instance.ConfigSize.Value = MinimalMinimap.Data.Size;
                        MinimalMinimap.Instance.ConfigZoom.Value = MinimalMinimap.Data.Zoom;

                        MinimalMinimap.Instance.Config.Save();
                    }
                }
            }

            if (MinimalMinimap.Data.IsEditMode && minimapObject != null && minimapImage != null && Mouse.current != null)
            {
                __instance.disableLookInput = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                RectTransform mapRt = minimapImage.rectTransform;
                Canvas canvas = minimapImage.canvas;
                Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

                Vector2 mousePosScreen = Mouse.current.position.ReadValue();
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                float scale = (canvas != null && canvas.scaleFactor > 0) ? canvas.scaleFactor : 1f;

                UpdateCubeXSliderVisuals();

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (zoomCardRt != null && RectTransformUtility.RectangleContainsScreenPoint(zoomCardRt, mousePosScreen, uiCamera))
                    {
                        isDraggingSlider = true;
                        isDraggingMap = false;
                        isResizingMap = false;
                    }
                    else if (RectTransformUtility.RectangleContainsScreenPoint(mapRt, mousePosScreen, uiCamera))
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(mapRt, mousePosScreen, uiCamera, out Vector2 localPoint);

                        float cornerZone = 35f;
                        bool isNearLeft = localPoint.x < (mapRt.rect.xMin + cornerZone);
                        bool isNearRight = localPoint.x > (mapRt.rect.xMax - cornerZone);
                        bool isNearBottom = localPoint.y < (mapRt.rect.yMin + cornerZone);
                        bool isNearTop = localPoint.y > (mapRt.rect.yMax - cornerZone);

                        if (isNearTop && isNearLeft) activeCorner = ResizeCorner.TopLeft;
                        else if (isNearTop && isNearRight) activeCorner = ResizeCorner.TopRight;
                        else if (isNearBottom && isNearLeft) activeCorner = ResizeCorner.BottomLeft;
                        else if (isNearBottom && isNearRight) activeCorner = ResizeCorner.BottomRight;
                        else activeCorner = ResizeCorner.None;

                        if (activeCorner != ResizeCorner.None)
                        {
                            isResizingMap = true;
                            isDraggingMap = false;
                        }
                        else
                        {
                            isDraggingMap = true;
                            isResizingMap = false;
                        }
                        isDraggingSlider = false;
                    }
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    if (isDraggingSlider && trackRt != null)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRt, mousePosScreen, uiCamera, out Vector2 localMouse);

                        float trackHeight = trackRt.rect.height;
                        float minY = -trackHeight / 2f;
                        float maxY = trackHeight / 2f;

                        float clampedY = Mathf.Clamp(localMouse.y, minY, maxY);
                        float normalized = Mathf.InverseLerp(minY, maxY, clampedY);

                        MinimalMinimap.Data.Zoom = Mathf.Lerp(60f, 5f, normalized);
                    }
                    else if (isDraggingMap)
                    {
                        mapRt.anchoredPosition += mouseDelta / scale;
                    }
                    else if (isResizingMap)
                    {
                        float deltaSize = 0f;

                        switch (activeCorner)
                        {
                            case ResizeCorner.BottomLeft:
                                deltaSize = (-mouseDelta.x - mouseDelta.y) / 2f / scale;
                                break;
                            case ResizeCorner.TopRight:
                                deltaSize = (mouseDelta.x + mouseDelta.y) / 2f / scale;
                                break;
                            case ResizeCorner.TopLeft:
                                deltaSize = (-mouseDelta.x + mouseDelta.y) / 2f / scale;
                                break;
                            case ResizeCorner.BottomRight:
                                deltaSize = (mouseDelta.x - mouseDelta.y) / 2f / scale;
                                break;
                        }

                        float oldSize = mapRt.sizeDelta.x;
                        float newSize = Mathf.Clamp(oldSize + deltaSize, 50f, 800f);
                        float actualDelta = newSize - oldSize;

                        mapRt.sizeDelta = new Vector2(newSize, newSize);

                        switch (activeCorner)
                        {
                            case ResizeCorner.TopRight:
                                mapRt.anchoredPosition += new Vector2(actualDelta, actualDelta);
                                break;
                            case ResizeCorner.TopLeft:
                                mapRt.anchoredPosition += new Vector2(0f, actualDelta);
                                break;
                            case ResizeCorner.BottomRight:
                                mapRt.anchoredPosition += new Vector2(actualDelta, 0f);
                                break;
                            case ResizeCorner.BottomLeft:
                                break;
                        }
                    }
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    isDraggingMap = false;
                    isResizingMap = false;
                    isDraggingSlider = false;
                    activeCorner = ResizeCorner.None;
                }
            }

            if (minimapObject != null && minimapObject.activeSelf)
            {
                if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey) && !isTyping)
                {
                    SwitchTarget();
                }

                if (__instance.isPlayerDead)
                {
                    if (MinimalMinimap.Data.FreezeTarget)
                        MinimalMinimap.Data.FreezeTarget = false;

                    if (__instance.spectatedPlayerScript != null)
                        SetMapTargetToPlayer(__instance.spectatedPlayerScript);
                }
                else
                {
                    if (!MinimalMinimap.Data.FreezeTarget)
                    {
                        MinimalMinimap.Data.FreezeTarget = true;
                        SetMapTargetToPlayer(__instance);
                    }
                }
            }

            HandleTooltipOverlap();
        }

        private static void UpdateCubeXSliderVisuals()
        {
            if (trackRt == null || fillRt == null || handleRt == null) return;

            float trackHeight = trackRt.rect.height;
            float normalized = Mathf.InverseLerp(60f, 5f, MinimalMinimap.Data.Zoom);

            fillRt.sizeDelta = new Vector2(0f, normalized * trackHeight);
            handleRt.anchoredPosition = new Vector2(0f, normalized * trackHeight);

            if (valText != null)
            {
                valText.text = $"{Mathf.RoundToInt(MinimalMinimap.Data.Zoom)}m";
            }
        }

        private static void UpdateMinimapVisibility()
        {
            if (minimapObject == null) return;

            bool isOnMoon = StartOfRound.Instance != null && !StartOfRound.Instance.inShipPhase;
            bool shouldBeActive = MinimalMinimap.Data.IsEditMode || (MinimalMinimap.Instance.ConfigEnabled.Value && isOnMoon);

            if (minimapObject.activeSelf != shouldBeActive)
            {
                minimapObject.SetActive(shouldBeActive);
            }

            if (zoomCardObj != null)
            {
                zoomCardObj.SetActive(MinimalMinimap.Data.IsEditMode);
            }

            if (shouldBeActive && StartOfRound.Instance != null && StartOfRound.Instance.mapScreen != null && StartOfRound.Instance.mapScreen.cam != null)
            {
                Texture currentTexture = StartOfRound.Instance.mapScreen.cam.targetTexture;
                if (minimapImage.texture != currentTexture && currentTexture != null)
                {
                    minimapImage.texture = currentTexture;
                }
            }
        }

        private static void HandleTooltipOverlap()
        {
            if (HUDManager.Instance == null || HUDManager.Instance.controlTipLines == null || HUDManager.Instance.controlTipLines.Length == 0) return;

            var firstTip = HUDManager.Instance.controlTipLines[0];
            if (firstTip == null || firstTip.transform.parent == null) return;

            RectTransform tooltipRect = firstTip.transform.parent.GetComponent<RectTransform>();
            if (tooltipRect == null) return;

            if (!tooltipInitialized)
            {
                originalTooltipY = tooltipRect.anchoredPosition.y;
                tooltipInitialized = true;
            }

            float currentX = tooltipRect.anchoredPosition.x;

            if (minimapObject == null || !minimapObject.activeSelf)
            {
                if (tooltipRect.anchoredPosition.y != originalTooltipY)
                    tooltipRect.anchoredPosition = new Vector2(currentX, originalTooltipY);
                return;
            }

            Bounds mapBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(tooltipRect.parent, minimapImage.rectTransform);

            float tipMinX = float.MaxValue;
            float tipMaxX = float.MinValue;
            float tipMinY = float.MaxValue;
            float tipMaxY = float.MinValue;
            bool hasActiveTips = false;

            foreach (var tip in HUDManager.Instance.controlTipLines)
            {
                if (tip != null && tip.gameObject.activeInHierarchy && !string.IsNullOrEmpty(tip.text))
                {
                    Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(tooltipRect.parent, tip.rectTransform);

                    float realMinX = b.max.x - tip.preferredWidth;
                    float realMaxX = b.max.x;

                    if (realMinX < tipMinX) tipMinX = realMinX;
                    if (realMaxX > tipMaxX) tipMaxX = realMaxX;
                    if (b.min.y < tipMinY) tipMinY = b.min.y;
                    if (b.max.y > tipMaxY) tipMaxY = b.max.y;
                    hasActiveTips = true;
                }
            }

            if (!hasActiveTips) return;

            float currentShiftY = tooltipRect.anchoredPosition.y - originalTooltipY;
            float virtualTipMinY = tipMinY - currentShiftY;
            float virtualTipMaxY = tipMaxY - currentShiftY;

            bool isOverlapping =
                mapBounds.min.x < tipMaxX && mapBounds.max.x > tipMinX &&
                mapBounds.min.y < virtualTipMaxY && mapBounds.max.y > virtualTipMinY;

            if (isOverlapping)
            {
                float targetTopY = mapBounds.min.y - 10f;
                float requiredShiftDown = virtualTipMaxY - targetTopY;
                tooltipRect.anchoredPosition = new Vector2(currentX, originalTooltipY - requiredShiftDown);
            }
            else
            {
                if (tooltipRect.anchoredPosition.y != originalTooltipY)
                    tooltipRect.anchoredPosition = new Vector2(currentX, originalTooltipY);
            }
        }

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

                if (player == null) continue;
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
            SwitchTarget();
            return false;
        }
    }
}