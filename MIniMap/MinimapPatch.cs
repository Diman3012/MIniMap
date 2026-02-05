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

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void UpdateMinimap()
        {
            if (minimapImage == null || StartOfRound.Instance == null) return;

            minimapImage.gameObject.SetActive(MinimalMinimap.Data.Enabled);
            if (!MinimalMinimap.Data.Enabled) return;

            if (UnityInput.Current.GetKeyDown(MinimalMinimap.Data.SwitchKey))
            {
                SwitchTarget();
            }
        }

        private static void SwitchTarget()
        {
            ManualCameraRenderer mapScreen = StartOfRound.Instance.mapScreen;
            if (mapScreen == null) return;

            int nextIndex = CalculateValidTargetIndex(mapScreen.targetTransformIndex + 1);
            mapScreen.SwitchRadarTargetAndSync(nextIndex);
        }

        private static int CalculateValidTargetIndex(int startIndex)
        {
            ManualCameraRenderer map = StartOfRound.Instance.mapScreen;
            int totalTargets = map.radarTargets.Count;
            int currentIndex = startIndex % totalTargets;

            for (int i = 0; i < totalTargets; i++)
            {
                int targetIdx = (currentIndex + i) % totalTargets;
                var target = map.radarTargets[targetIdx];

                if (target != null)
                {
                    PlayerControllerB player = target.transform.GetComponent<PlayerControllerB>();
                    if (player != null)
                    {
                        if ((player.isPlayerControlled || player.isPlayerDead) && !player.isPlayerAlone)
                            return targetIdx;
                    }
                    else
                    {
                        return targetIdx; // Например, радар-бустер
                    }
                }
            }
            return 0;
        }
    }
}