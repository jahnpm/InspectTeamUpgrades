using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace InspectTeamUpgrades
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "jahnpm.InspectTeamUpgrades";
        public const string PluginName = "InspectTeamUpgrades";
        public const string PluginVersion = "0.9.0";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> EnableScrollWheel;
        internal static ConfigEntry<bool> InvertScrollWheel;
        internal static ConfigEntry<Key> NextPageKey;
        internal static ConfigEntry<Key> PrevPageKey;
        internal static ConfigEntry<bool> ResetPageOnClose;
        internal static ConfigEntry<int> TestPages;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            EnableScrollWheel = Config.Bind("Controls", "EnableScrollWheel", true,
                "Cycle player upgrade pages with the mouse scroll wheel while the map (Tab) is open.");
            InvertScrollWheel = Config.Bind("Controls", "InvertScrollWheel", false,
                "Invert the mouse scroll wheel direction for cycling pages (default: scroll down = next page).");
            NextPageKey = Config.Bind("Controls", "NextPageKey", Key.RightArrow,
                "Key to show the next player's upgrades while the map (Tab) is open.");
            PrevPageKey = Config.Bind("Controls", "PrevPageKey", Key.LeftArrow,
                "Key to show the previous player's upgrades while the map (Tab) is open.");
            ResetPageOnClose = Config.Bind("General", "ResetPageOnClose", true,
                "Jump back to your own upgrades every time the map is closed.");
            TestPages = Config.Bind("Debug", "TestPages", 0,
                new ConfigDescription(
                    "Number of fake player pages to append, for testing the paging UI without other players. Set to 0 for normal play.",
                    new AcceptableValueRange<int>(0, 10)));

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }
    }
}
