using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;
using System.Linq;

namespace MIniMap
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class MinimalMinimap : BaseUnityPlugin
    {
        public static MinimalMinimap Instance;
        public static MinimapData Data;

        // Конфигурационные параметры BepInEx
        public ConfigEntry<bool> ConfigEnabled;
        public ConfigEntry<float> ConfigZoom;
        public ConfigEntry<int> ConfigSize;
        public ConfigEntry<float> ConfigXOffset;
        public ConfigEntry<float> ConfigYOffset;

        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            Data = new MinimapData();

            // Привязка настроек к файлу конфигурации
            ConfigEnabled = Config.Bind("General", "Enabled", true, "Enable or disable the minimap");
            ConfigZoom = Config.Bind("General", "Zoom", 20f, "Camera Orthographic Zoom ( lower = closer )");
            ConfigSize = Config.Bind("General", "Size", 200, "Minimap UI width and height");
            ConfigXOffset = Config.Bind("General", "XOffset", -10f, "Minimap X position offset");
            ConfigYOffset = Config.Bind("General", "YOffset", -10f, "Minimap Y position offset");

            // Инициализация стартовых значений из файла конфига
            Data.Zoom = ConfigZoom.Value;
            Data.Size = ConfigSize.Value;
            Data.XOffset = ConfigXOffset.Value;
            Data.YOffset = ConfigYOffset.Value;

            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            Logger.LogInfo($"Minimal Minimap ({MyPluginInfo.PLUGIN_NAME}) loaded successfully!");
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.diman3012.minimap";
        public const string PLUGIN_NAME = "Minimal Minimap";
        public const string PLUGIN_VERSION = "1.1.6";
    }

    public class MinimapData
    {
        // 🔧 НАСТРОЙКИ
        public int Size = 200;
        public float XOffset = -10f;
        public float YOffset = -10f;
        public float Zoom = 20f;
        public bool AutoRotate = true;

        // Флаг для режима перемещения
        public bool IsEditMode = false;

        // 🎮 УПРАВЛЕНИЕ
        public bool FreezeTarget = true;

        public KeyCode SwitchKey = KeyCode.F3;
        public KeyCode ToggleKey = KeyCode.F2;
    }

    [HarmonyPatch(typeof(NetworkManager))]
    internal static class NetworkPrefabPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NetworkManager.SetSingleton))]
        private static void RegisterPrefab()
        {
            var prefab = new GameObject(MyPluginInfo.PLUGIN_GUID + " Prefab");
            prefab.hideFlags |= HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(prefab);

            var networkObject = prefab.AddComponent<NetworkObject>();

            var fieldInfo = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(networkObject, GetHash(MyPluginInfo.PLUGIN_GUID));
            }

            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);
        }

        private static uint GetHash(string value)
        {
            return value?.Aggregate(17u, (current, c) => unchecked((current * 31) ^ c)) ?? 0u;
        }
    }
}