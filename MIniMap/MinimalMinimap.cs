using BepInEx;
using HarmonyLib;
using UnityEngine;
using Unity.Netcode; // Добавлено для работы с NetworkObject
using System.Reflection; // Добавлено для Reflection
using System.Linq; // Добавлено для Aggregate

namespace MIniMap
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class MinimalMinimap : BaseUnityPlugin
    {
        public static MinimalMinimap Instance;
        public static MinimapData Data;
        private Harmony harmony;

        private void Awake()
        {
            Instance = this;
            Data = new MinimapData();

            harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            Logger.LogInfo($"Minimal Minimap ({MyPluginInfo.PLUGIN_NAME}) loaded successfully!");
        }
    }

    // Автоматически генерируемый класс с инфой о плагине (стандарт BepInEx)
    public static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.diman3012.minimap";
        public const string PLUGIN_NAME = "Minimal Minimap";
        public const string PLUGIN_VERSION = "1.0.0";
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
        public bool FreezeTarget = false; // Состояние F3 (Override)
        public KeyCode OverrideKey = KeyCode.F3;
        public KeyCode SwitchKey = KeyCode.F4;
    }

    // КЛАСС ДЛЯ ПРОВЕРКИ НАЛИЧИЯ МОДА У ВСЕХ ИГРОКОВ
    [HarmonyPatch(typeof(NetworkManager))]
    internal static class NetworkPrefabPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NetworkManager.SetSingleton))]
        private static void RegisterPrefab()
        {
            // Создаем невидимый объект, который Unity.Netcode будет искать у всех клиентов
            var prefab = new GameObject(MyPluginInfo.PLUGIN_GUID + " Prefab");
            prefab.hideFlags |= HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(prefab);

            var networkObject = prefab.AddComponent<NetworkObject>();

            // Используем Reflection для установки Hash, чтобы не зависеть от Publicizer
            var fieldInfo = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(networkObject, GetHash(MyPluginInfo.PLUGIN_GUID));
            }

            // Регистрируем префикс в сетевом менеджере
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(prefab);
        }

        // Хеширование GUID для создания уникального ID префаба
        private static uint GetHash(string value)
        {
            return value?.Aggregate(17u, (current, c) => unchecked((current * 31) ^ c)) ?? 0u;
        }
    }
}