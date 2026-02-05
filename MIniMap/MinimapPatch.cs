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
        if (!MinimalMinimap.Data.Enabled)
            return;

        if (minimapObject != null)
            return;

        // 📦 создаём объект миникарты
        minimapObject = new GameObject("MinimalMinimap");
        minimapImage = minimapObject.AddComponent<RawImage>();

        RectTransform rt = minimapImage.rectTransform;
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(
            MinimalMinimap.Data.Size,
            MinimalMinimap.Data.Size
        );
        rt.anchoredPosition = new Vector2(
            MinimalMinimap.Data.XOffset,
            MinimalMinimap.Data.YOffset
        );

        // 🧠 берём текстуру камеры радара
        minimapImage.texture = StartOfRound.Instance.mapScreen.cam.targetTexture;

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
        if (minimapImage == null)
            return;

        // включение / выключение
        minimapImage.gameObject.SetActive(MinimalMinimap.Data.Enabled);

        // размер
        minimapImage.rectTransform.sizeDelta =
            new Vector2(MinimalMinimap.Data.Size, MinimalMinimap.Data.Size);

        // позиция
        minimapImage.rectTransform.anchoredPosition =
            new Vector2(
                MinimalMinimap.Data.XOffset,
                MinimalMinimap.Data.YOffset
            );
    }
}
