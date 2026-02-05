using BepInEx;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("minimal.minimap", "Minimal Minimap", "1.0.0")]
public class MinimalMinimap : BaseUnityPlugin
{
    public static MinimalMinimap Instance;
    public static MinimapData Data;

    private Harmony harmony;

    private void Awake()
    {
        Instance = this;
        Data = new MinimapData();

        harmony = new Harmony("minimal.minimap");
        harmony.PatchAll();

        Logger.LogInfo("Minimal Minimap loaded");
    }
}

public class MinimapData
{
    // 🔧 НАСТРОЙКИ МИНИКАРТЫ
    public bool Enabled = true;
    public int Size = 200;
    public float XOffset = -10f;
    public float YOffset = -10f;
    public float Zoom = 20f;
    public bool AutoRotate = true;

    // 🎮 УПРАВЛЕНИЕ
    public KeyCode SwitchKey = KeyCode.F2; // Клавиша смены игрока
}