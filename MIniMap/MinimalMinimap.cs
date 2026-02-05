using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MIniMap
{
    [BepInPlugin("com.diman3012.minimap", "Minimal Minimap", "1.0.0")]
    public class MinimalMinimap : BaseUnityPlugin
    {
        public static MinimalMinimap Instance;
        public static MinimapData Data;
        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            Data = new MinimapData();

            harmony = new Harmony("com.diman3012.minimap");
            harmony.PatchAll();

            Logger.LogInfo("Minimal Minimap (MIniMap) loaded successfully!");
        }
    }

    public class MinimapData
    {
        // 🔧 НАСТРОЙКИ
        public bool Enabled = true;
        public int Size = 200;
        public float XOffset = -10f;
        public float YOffset = -10f;
        public float Zoom = 20f;
        public bool AutoRotate = true;

        // 🎮 УПРАВЛЕНИЕ
        public KeyCode SwitchKey = KeyCode.F2;
    }
}