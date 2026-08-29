using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode;
using System.Reflection;
using System.Linq;
using GameNetcodeStuff;

namespace MIniMap
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class MinimalMinimap : BaseUnityPlugin
    {
        public static MinimalMinimap Instance;
        public static MinimapData Data;

        public static PlayerControllerB CustomTarget;
        public static int CustomTargetIndex;

        public ConfigEntry<bool> ConfigEnabled;

        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            Data = new MinimapData();

            ConfigEnabled = Config.Bind("General", "Enabled", false, "Enable or disable the minimap");

            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            Logger.LogInfo($"Minimal Minimap ({MyPluginInfo.PLUGIN_NAME}) loaded successfully!");
        }
    }

    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.diman3012.minimap";
        public const string PLUGIN_NAME = "Minimal Minimap";
        public const string PLUGIN_VERSION = "1.0.0";
    }

    public class MinimapData
    {
        // 🔧 НАСТРОЙКИ
        public int Size = 200;
        public float XOffset = -10f;
        public float YOffset = -10f;
        public float Zoom = 20f;
        public bool AutoRotate = true;

        // 🎮 УПРАВЛЕНИЕ
        public bool FreezeTarget = true;
        public bool IsEditMode = false; // Режим редактирования

        public KeyCode SwitchKey = KeyCode.F3;
        public KeyCode ToggleKey = KeyCode.F2;

        // 🔍 НОВЫЕ НАСТРОЙКИ ЗУМА
        public KeyCode ZoomKey = KeyCode.F4;
        public float[] ZoomLevels = new float[3] { 60f, 40f, 20f };
        public int currentZoomIndex;
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