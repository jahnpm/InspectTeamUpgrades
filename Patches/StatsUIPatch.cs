using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace InspectTeamUpgrades.Patches
{
    /// <summary>
    /// Extends the Tab-map upgrades list (<see cref="StatsUI"/>) with one page per player.
    /// Page 1 is always the local player; use arrow keys/scroll wheel to cycle through everyone else.
    /// Upgrade data for all players is already synced to every client by the game
    /// (PunManager.SyncAllDictionaries / upgrade RPCs), so no networking is needed.
    /// </summary>
    [HarmonyPatch(typeof(StatsUI))]
    internal static class StatsUIPatch
    {
        private const int MaxNameLength = 16;

        private struct Page
        {
            public string SteamId;
            public string Title;

            /// <summary>Non-null only for debug test pages; used instead of StatsManager data.</summary>
            public Dictionary<string, int> FakeUpgrades;
        }

        private static readonly List<Page> Pages = new List<Page>();

        /// <summary>Steam ID of the page being viewed; null means the local player.</summary>
        private static string _selectedSteamId;

        /// <summary>The header text from the prefab (e.g. "UPGRADES"), used for the local page.</summary>
        private static string _originalHeader;

        private static bool _wasVisible;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StatsUI.Start))]
        private static void StartPostfix(StatsUI __instance)
        {
            _originalHeader = __instance.upgradesHeader.text;
            _selectedSteamId = null;
        }

        /// <summary>Picking up an upgrade pops the list open - always show your own page then.</summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StatsUI.ShowStats))]
        private static void ShowStatsPrefix()
        {
            _selectedSteamId = null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StatsUI.Update))]
        private static void UpdatePostfix(StatsUI __instance)
        {
            bool visible = __instance.showTimer > 0f;
            if (!visible)
            {
                if (_wasVisible && Plugin.ResetPageOnClose.Value)
                {
                    _selectedSteamId = null;
                }
                _wasVisible = false;
                return;
            }
            _wasVisible = true;

            if (!SemiFunc.NoTextInputsActive())
            {
                return;
            }

            int direction = 0;

            if (Plugin.EnableScrollWheel.Value)
            {
                Mouse mouse = Mouse.current;
                if (mouse != null)
                {
                    float scrollY = mouse.scroll.ReadValue().y;
                    if (scrollY > 0.1f)
                    {
                        direction += Plugin.InvertScrollWheel.Value ? 1 : -1;
                    }
                    else if (scrollY < -0.1f)
                    {
                        direction += Plugin.InvertScrollWheel.Value ? -1 : 1;
                    }
                }
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (WasPressed(keyboard, Plugin.NextPageKey.Value)) direction++;
                if (WasPressed(keyboard, Plugin.PrevPageKey.Value)) direction--;
            }

            if (direction == 0)
            {
                return;
            }

            BuildPages();
            if (Pages.Count <= 1)
            {
                return;
            }

            int index = ((SelectedIndex() + direction) % Pages.Count + Pages.Count) % Pages.Count;
            _selectedSteamId = index == 0 ? null : Pages[index].SteamId;

            __instance.Fetch();
            __instance.SemiUISpringShakeX(5f, 10f, 0.15f);
        }

        /// <summary>
        /// Replaces the vanilla list rendering when there is more than one player.
        /// Single-player / solo lobbies fall through to vanilla untouched.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StatsUI.Fetch))]
        private static bool FetchPrefix(StatsUI __instance)
        {
            if (_originalHeader == null)
            {
                _originalHeader = __instance.upgradesHeader.text;
            }

            BuildPages();
            if (Pages.Count <= 1)
            {
                _selectedSteamId = null;
                __instance.upgradesHeader.text = _originalHeader;
                return true;
            }

            int index = SelectedIndex();
            Page page = Pages[index];

            var names = new StringBuilder();
            var numbers = new StringBuilder();
            foreach (KeyValuePair<string, int> upgrade in
                     (page.FakeUpgrades ?? StatsManager.instance.FetchPlayerUpgrades(page.SteamId))
                         .Where(u => u.Value > 0)
                         .OrderBy(u => StatsUI.GetUpgradeDisplayName(u.Key)))
            {
                names.Append(StatsUI.GetUpgradeDisplayName(upgrade.Key)).Append('\n');
                numbers.Append("<b>").Append(upgrade.Value).Append("</b>\n");
            }

            if (names.Length == 0)
            {
                names.Append("<alpha=#88><i>No upgrades</i>");
            }

            __instance.Text.text = names.ToString();
            __instance.textNumbers.text = numbers.ToString();
            __instance.upgradesHeader.text =
                $"{page.Title} <size=60%><noparse><</noparse> {index + 1}/{Pages.Count} <noparse>></noparse></size>";
            __instance.upgradesHeader.enabled = true;
            __instance.scanlineObject.SetActive(true);
            return false;
        }

        private static bool WasPressed(Keyboard keyboard, Key key)
        {
            return key != Key.None && keyboard[key].wasPressedThisFrame;
        }

        private static int SelectedIndex()
        {
            if (_selectedSteamId == null)
            {
                return 0;
            }
            int index = Pages.FindIndex(p => p.SteamId == _selectedSteamId);
            if (index < 0)
            {
                // Selected player left the game.
                _selectedSteamId = null;
                return 0;
            }
            return index;
        }

        /// <summary>Local player first, then everyone else in lobby order.</summary>
        private static void BuildPages()
        {
            Pages.Clear();

            string localSteamId = PlayerController.instance ? PlayerController.instance.playerSteamID : null;
            if (string.IsNullOrEmpty(localSteamId))
            {
                return;
            }
            Pages.Add(new Page { SteamId = localSteamId, Title = _originalHeader });

            List<PlayerAvatar> players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (PlayerAvatar player in players)
                {
                    if (!player || string.IsNullOrEmpty(player.steamID) || player.steamID == localSteamId)
                    {
                        continue;
                    }
                    string name = player.playerName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = SemiFunc.PlayerGetName(player);
                    }
                    Pages.Add(new Page { SteamId = player.steamID, Title = FormatName(name) });
                }
            }

            AddTestPages();
        }

        // ---- Debug test pages -------------------------------------------------------------

        private static readonly string[] TestNames =
        {
            "TestPlayer",
            "AVeryLongTestPlayerName", // exercises name truncation
            "EmptyPlayer",             // exercises the "No upgrades" state
        };

        private static readonly List<Dictionary<string, int>> TestUpgrades = new List<Dictionary<string, int>>();

        /// <summary>Appends fake pages (config: Debug.TestPages) so paging can be tried solo.</summary>
        private static void AddTestPages()
        {
            int count = Plugin.TestPages.Value;
            if (count <= 0 || !StatsManager.instance)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (i >= TestUpgrades.Count)
                {
                    TestUpgrades.Add(MakeTestUpgrades(i));
                }
                string name = TestNames[i % TestNames.Length] + (i >= TestNames.Length ? (i + 1).ToString() : "");
                Pages.Add(new Page
                {
                    SteamId = "test-page-" + i,
                    Title = FormatName(name),
                    FakeUpgrades = TestUpgrades[i],
                });
            }
        }

        /// <summary>Deterministic pseudo-random levels for every real upgrade type the game knows about.</summary>
        private static Dictionary<string, int> MakeTestUpgrades(int pageIndex)
        {
            var result = new Dictionary<string, int>();
            if (pageIndex % TestNames.Length == 2)
            {
                return result; // the "EmptyPlayer" page
            }

            int j = 0;
            foreach (string key in StatsManager.instance.dictionaryOfDictionaries.Keys)
            {
                if (!key.StartsWith("playerUpgrade"))
                {
                    continue;
                }
                // Long-name page gets lots of upgrades to test list height; the first gets a few.
                int level = pageIndex % TestNames.Length == 1
                    ? 1 + (j * 5 + pageIndex) % 6
                    : (j * 7 + pageIndex * 3) % 4 == 0 ? 1 + j % 3 : 0;
                result[key] = level;
                j++;
            }
            return result;
        }

        private static string FormatName(string rawName)
        {
            string name = string.IsNullOrEmpty(rawName) ? "Player" : rawName;
            if (name.Length > MaxNameLength)
            {
                name = name.Substring(0, MaxNameLength - 2) + "..";
            }
            // noparse so names containing '<' can't inject rich-text tags.
            return "<noparse>" + name.ToUpperInvariant().Replace("</noparse>", "") + "</noparse>";
        }
    }
}
