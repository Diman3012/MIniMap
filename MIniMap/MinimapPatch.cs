using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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

        // 📦 создаём объект миникарты
        minimapObject = new GameObject("MinimalMinimap");
        minimapImage = minimapObject.AddComponent<RawImage>();

        RectTransform rt = minimapImage.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(MinimalMinimap.Data.Size, MinimalMinimap.Data.Size);
        rt.anchoredPosition = new Vector2(MinimalMinimap.Data.XOffset, MinimalMinimap.Data.YOffset);

        // 🧠 берём текстуру камеры радара
        if (StartOfRound.Instance.mapScreen != null)
        {
            minimapImage.texture = StartOfRound.Instance.mapScreen.cam.targetTexture;
        }

        // 👁 добавляем в HUD
        minimapObject.transform.SetParent(
            HUDManager.Instance.playerScreenTexture.transform,
            false
        );
    }

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void UpdateMinimap()
    {
        if (minimapImage == null || StartOfRound.Instance == null)
            return;

        // Включение / выключение
        minimapImage.gameObject.SetActive(MinimalMinimap.Data.Enabled);
        if (!MinimalMinimap.Data.Enabled) return;

        // Обновление размеров (на случай изменения настроек)
        minimapImage.rectTransform.sizeDelta = new Vector2(MinimalMinimap.Data.Size, MinimalMinimap.Data.Size);
        minimapImage.rectTransform.anchoredPosition = new Vector2(MinimalMinimap.Data.XOffset, MinimalMinimap.Data.YOffset);

        // 🎮 ОБРАБОТКА НАЖАТИЯ КЛАВИШИ (Смена цели)
        // Используем стандартный Unity Input
        if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey))
        {
            SwitchTarget();
        }
    }

    // 🎯 ЛОГИКА ПЕРЕКЛЮЧЕНИЯ (Адаптировано из MinimapGUI.cs)
    private static void SwitchTarget()
    {
        ManualCameraRenderer mapScreen = StartOfRound.Instance.mapScreen;
        if (mapScreen == null) return;

        // Получаем следующий валидный индекс
        int nextIndex = CalculateValidTargetIndex(mapScreen.targetTransformIndex + 1);

        // Применяем переключение (это синхронизирует и монитор корабля)
        mapScreen.SwitchRadarTargetAndSync(nextIndex);

        // Подсказка в лог (опционально)
        Debug.Log($"[Minimap] Switched target to index: {nextIndex}");
    }

    // 🧮 ВАЛИДАЦИЯ ЦЕЛИ (Пропускаем мертвых и отключенных)
    private static int CalculateValidTargetIndex(int startIndex)
    {
        ManualCameraRenderer map = StartOfRound.Instance.mapScreen;
        int totalTargets = map.radarTargets.Count;
        int checkedCount = 0;
        int currentIndex = startIndex;

        // Зацикливаем индекс, если вышли за пределы
        if (currentIndex >= totalTargets) currentIndex = 0;

        while (checkedCount < totalTargets)
        {
            Transform targetTransform = map.radarTargets[currentIndex].transform;
            bool isValid = false;

            if (targetTransform != null)
            {
                // Проверяем, игрок ли это
                PlayerControllerB player = targetTransform.GetComponent<PlayerControllerB>();

                if (player != null)
                {
                    // Игрок валиден, если он управляем, не мертв и подключен
                    if ((player.isPlayerControlled || player.isPlayerDead) && !player.isPlayerAlone)
                    {
                        isValid = true;
                    }

                    // ❌ DELETE OR COMMENT OUT THIS BLOCK ❌
                    // "redirectToEnemyPower" does not exist in PlayerControllerB
                    /* if (player.redirectToEnemyPower != null)
                    {
                        isValid = true;
                    }
                    */
                }
                else
                {
                    // Если это не игрок (например, радар-бустер), считаем валидным
                    isValid = true;
                }
            }

            if (isValid)
            {
                return currentIndex;
            }

            // Идем к следующему
            currentIndex++;
            if (currentIndex >= totalTargets) currentIndex = 0;
            checkedCount++;
        }

        // Если ничего не нашли, возвращаем исходный
        return startIndex >= totalTargets ? 0 : startIndex;
    }
}