using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MIniMap
{
    [HarmonyPatch(typeof(ManualCameraRenderer))]
    internal class ManualCameraRendererPatch
    {
        private static Vector3 defaultEulerAngles = new Vector3(90f, 0f, 0f);

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void MapCameraLogic(
            ref Camera ___mapCamera,
            ref PlayerControllerB ___targetedPlayer,
            ref Image ___compassRose)
        {
            // Заменили Data.Enabled на Instance.ConfigEnabled.Value
            if (!MinimalMinimap.Instance.ConfigEnabled.Value || ___mapCamera == null)
                return;

            ___mapCamera.enabled = true;

            if (___mapCamera.orthographicSize != MinimalMinimap.Data.Zoom)
            {
                ___mapCamera.orthographicSize = MinimalMinimap.Data.Zoom;
            }

            // Логика автоповорота
            if (MinimalMinimap.Data.AutoRotate && ___targetedPlayer != null)
            {
                ___mapCamera.transform.eulerAngles = new Vector3(
                    defaultEulerAngles.x,
                    ___targetedPlayer.transform.eulerAngles.y,
                    defaultEulerAngles.z
                );
            }
            else
            {
                if (___mapCamera.transform.eulerAngles != defaultEulerAngles)
                {
                    ___mapCamera.transform.eulerAngles = defaultEulerAngles;
                }
            }

            // Исправление вращения иконок объектов
            TerminalAccessibleObject[] mapObjects = Object.FindObjectsOfType<TerminalAccessibleObject>();
            for (int i = 0; i < mapObjects.Length; i++)
            {
                if (mapObjects[i].mapRadarObject != null)
                {
                    mapObjects[i].mapRadarObject.transform.eulerAngles = new Vector3(
                        defaultEulerAngles.x,
                        ___mapCamera.transform.eulerAngles.y,
                        defaultEulerAngles.z
                    );
                }
            }

            // Поворот компаса
            if (___compassRose != null)
            {
                ___compassRose.rectTransform.localEulerAngles = new Vector3(
                    0f, 0f, ___mapCamera.transform.eulerAngles.y
                );
            }
        }
    }
}