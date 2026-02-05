using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(ManualCameraRenderer))]
internal class ManualCameraRendererPatch
{
    // Стандартный угол наклона камеры (90 градусов вниз)
    private static Vector3 defaultEulerAngles = new Vector3(90f, 0f, 0f);

    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    private static void MapCameraLogic(
        ref Camera ___mapCamera,
        ref PlayerControllerB ___targetedPlayer,
        ref Image ___compassRose)
    {
        // 1. Проверка: включен ли мод и существует ли камера
        if (!MinimalMinimap.Data.Enabled || ___mapCamera == null)
            return;

        // 2. Включаем камеру всегда (чтобы работала вне корабля)
        ___mapCamera.enabled = true;

        // 3. Синхронизируем зум
        if (___mapCamera.orthographicSize != MinimalMinimap.Data.Zoom)
        {
            ___mapCamera.orthographicSize = MinimalMinimap.Data.Zoom;
        }

        // 4. Логика ВРАЩЕНИЯ (AutoRotate)
        if (MinimalMinimap.Data.AutoRotate && ___targetedPlayer != null)
        {
            // Если автоповорот включен: камера крутится вместе с игроком (по оси Y)
            ___mapCamera.transform.eulerAngles = new Vector3(
                defaultEulerAngles.x,
                ___targetedPlayer.transform.eulerAngles.y,
                defaultEulerAngles.z
            );
        }
        else
        {
            // Если выключен: держим строгий север (сбрасываем поворот)
            if (___mapCamera.transform.eulerAngles != defaultEulerAngles)
            {
                ___mapCamera.transform.eulerAngles = defaultEulerAngles;
            }
        }

        // 5. Поворот иконок на карте (турели, двери, мины)
        // Чтобы иконки не крутились вместе с картой, а оставались "стоять" правильно относительно игрока
        TerminalAccessibleObject[] mapObjects = Object.FindObjectsOfType<TerminalAccessibleObject>();
        for (int i = 0; i < mapObjects.Length; i++)
        {
            mapObjects[i].mapRadarObject.transform.eulerAngles = new Vector3(
                defaultEulerAngles.x,
                ___mapCamera.transform.eulerAngles.y,
                defaultEulerAngles.z
            );
        }

        // 6. Поворот Компаса (если он есть в UI)
        if (___compassRose != null)
        {
            ___compassRose.rectTransform.localEulerAngles = new Vector3(
                0f,
                0f,
                ___mapCamera.transform.eulerAngles.y
            );
        }
    }
}