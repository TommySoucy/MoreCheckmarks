using BepInEx.Configuration;
using UnityEngine;

namespace MoreCheckmarks
{
    public static class MoreCheckmarksConfig
    {
        // Config Entries (BepInEx F12 menu)
        public static ConfigEntry<bool> configFulfilledAnyCanBeUpgraded;
        public static ConfigEntry<int> configQuestPriority;
        public static ConfigEntry<int> configHideoutPriority;
        public static ConfigEntry<int> configWishlistPriority;
        public static ConfigEntry<int> configBarterPriority;
        public static ConfigEntry<int> configCraftPriority;
        public static ConfigEntry<bool> configShowFutureModulesLevels;
        public static ConfigEntry<bool> configShowBarter;
        public static ConfigEntry<bool> configShowCraft;
        public static ConfigEntry<bool> configShowFutureCraft;
        public static ConfigEntry<Color> configNeedMoreColor;
        public static ConfigEntry<Color> configFulfilledColor;
        public static ConfigEntry<Color> configWishlistColor;
        public static ConfigEntry<Color> configBarterColor;
        public static ConfigEntry<Color> configCraftColor;
        public static ConfigEntry<bool> configIncludeFutureQuests;
        public static ConfigEntry<bool> configShowPrerequisiteQuests;
        public static ConfigEntry<bool> configShowQuestCheckmarksNonFIR;

        // Config settings (derived from ConfigEntry values)
        public static bool fulfilledAnyCanBeUpgraded => configFulfilledAnyCanBeUpgraded.Value;
        public static int questPriority => configQuestPriority.Value;
        public static int hideoutPriority => configHideoutPriority.Value;
        public static int wishlistPriority => configWishlistPriority.Value;
        public static int barterPriority => configBarterPriority.Value;
        public static int craftPriority => configCraftPriority.Value;
        public static bool showFutureModulesLevels => configShowFutureModulesLevels.Value;
        public static bool showBarter => configShowBarter.Value;
        public static bool showCraft => configShowCraft.Value;
        public static bool showFutureCraft => configShowFutureCraft.Value;
        public static bool includeFutureQuests => configIncludeFutureQuests.Value;
        public static bool showPrerequisiteQuests => configShowPrerequisiteQuests.Value;
        public static bool showQuestCheckmarksNonFIR => configShowQuestCheckmarksNonFIR.Value;

        // Parsed colors (updated when config changes)
        public static Color needMoreColor = new Color(1, 0.37255f, 0.37255f);
        public static Color fulfilledColor = new Color(0.30588f, 1, 0.27843f);
        public static Color wishlistColor = new Color(0, 0, 1);
        public static Color barterColor = new Color(1, 0, 1);
        public static Color craftColor = new Color(0, 1, 1);

        // Priority and color arrays
        public static int[] priorities = { 0, 1, 2, 3, 4 };
        public static bool[] neededFor = new bool[5];
        public static Color[] colors = { Color.yellow, needMoreColor, wishlistColor, barterColor, craftColor };

        public static void Bind(ConfigFile config)
        {
            // Note about changes requiring menu refresh
            config.Bind(
                "0. Important Note",
                "Refresh Required",
                "Switch menus to apply changes",
                new ConfigDescription("Changes don't apply immediately. To see updates: leave your current menu (e.g. stash), go to main menu, then return.", null, new ConfigurationManagerAttributes { ReadOnly = true, HideDefaultButton = true }));

            // Hideout Settings
            configFulfilledAnyCanBeUpgraded = config.Bind(
                "Hideout",
                "Fulfilled Any Can Be Upgraded",
                true,
                "When TRUE, shows fulfilled checkmark when AT LEAST ONE hideout module can be upgraded. When FALSE, shows fulfilled only when ALL modules can be upgraded.");

            configShowFutureModulesLevels = config.Bind(
                "Hideout",
                "Show Future Module Levels",
                true,
                "Show requirements for future hideout module levels instead of only the next one.");

            // Quest Settings
            configIncludeFutureQuests = config.Bind(
                "Quests",
                "Include Future Quests",
                true,
                "Consider future quests when checking which quests an item is required for. If false, behaves like vanilla.");

            configShowPrerequisiteQuests = config.Bind(
                "Quests",
                "Show Prerequisite Count",
                true,
                "Show the number of prerequisite quests needed before each quest becomes available. Quests are sorted by prerequisite count with color coding: Green (0 prereqs), Yellow (1-9), Gray (10+).");

            configShowQuestCheckmarksNonFIR = config.Bind(
                "Quests",
                "Show Quest Checkmarks for Non-FIR Items",
                false,
                "When enabled, quest checkmarks will appear on items even if they aren't found in raid. Useful if your SPT is configured to accept non-FIR items for quest turn-ins.");

            // Barter & Craft Settings
            configShowBarter = config.Bind(
                "Barter & Craft",
                "Show Barter",
                true,
                "Show checkmark and tooltip for barters/trades this item is needed for.");

            configShowCraft = config.Bind(
                "Barter & Craft",
                "Show Craft",
                true,
                "Show checkmark and tooltip for crafting recipes this item is needed for.");

            configShowFutureCraft = config.Bind(
                "Barter & Craft",
                "Show Future Craft",
                true,
                "Show crafting recipes that are not yet unlocked.");

            // Priority Settings (higher = takes precedence, ordered by default priority)
            configQuestPriority = config.Bind(
                "Priority",
                "Quest Priority",
                4,
                new ConfigDescription("Priority for quest checkmarks. Higher number = higher priority when item is needed for multiple things.",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes { Order = 5 }));

            configHideoutPriority = config.Bind(
                "Priority",
                "Hideout Priority",
                3,
                new ConfigDescription("Priority for hideout checkmarks. Higher number = higher priority.",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes { Order = 4 }));

            configWishlistPriority = config.Bind(
                "Priority",
                "Wishlist Priority",
                2,
                new ConfigDescription("Priority for wishlist checkmarks. Higher number = higher priority.",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes { Order = 3 }));

            configBarterPriority = config.Bind(
                "Priority",
                "Barter Priority",
                1,
                new ConfigDescription("Priority for barter checkmarks. Higher number = higher priority.",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes { Order = 2 }));

            configCraftPriority = config.Bind(
                "Priority",
                "Craft Priority",
                0,
                new ConfigDescription("Priority for craft checkmarks. Higher number = higher priority.",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes { Order = 1 }));

            // Color Settings (RGB sliders in F12 menu)
            configNeedMoreColor = config.Bind(
                "Colors",
                "Need More Color",
                new Color(1f, 0.37255f, 0.37255f),
                "Color for items where you need more (default: light red)");

            configFulfilledColor = config.Bind(
                "Colors",
                "Fulfilled Color",
                new Color(0.30588f, 1f, 0.27843f),
                "Color for items where requirement is fulfilled (default: light green)");

            configWishlistColor = config.Bind(
                "Colors",
                "Wishlist Color",
                new Color(0.23137f, 0.93725f, 1f),
                "Color for wishlist items (default: light blue)");

            configBarterColor = config.Bind(
                "Colors",
                "Barter Color",
                new Color(1f, 0f, 1f),
                "Color for barter items (default: magenta)");

            configCraftColor = config.Bind(
                "Colors",
                "Craft Color",
                new Color(0f, 1f, 1f),
                "Color for craft items (default: cyan)");

            // Subscribe to config changes
            configNeedMoreColor.SettingChanged += (s, e) => UpdateColors();
            configFulfilledColor.SettingChanged += (s, e) => UpdateColors();
            configWishlistColor.SettingChanged += (s, e) => UpdateColors();
            configBarterColor.SettingChanged += (s, e) => UpdateColors();
            configCraftColor.SettingChanged += (s, e) => UpdateColors();
            configQuestPriority.SettingChanged += (s, e) => UpdatePriorities();
            configHideoutPriority.SettingChanged += (s, e) => UpdatePriorities();
            configWishlistPriority.SettingChanged += (s, e) => UpdatePriorities();
            configBarterPriority.SettingChanged += (s, e) => UpdatePriorities();
            configCraftPriority.SettingChanged += (s, e) => UpdatePriorities();

            // Initialize colors and priorities
            UpdateColors();
            UpdatePriorities();
        }

        public static void UpdateColors()
        {
            needMoreColor = configNeedMoreColor.Value;
            fulfilledColor = configFulfilledColor.Value;
            wishlistColor = configWishlistColor.Value;
            barterColor = configBarterColor.Value;
            craftColor = configCraftColor.Value;

            // Update the colors array
            colors[1] = needMoreColor;
            colors[2] = wishlistColor;
            colors[3] = barterColor;
            colors[4] = craftColor;
        }

        public static void UpdatePriorities()
        {
            priorities[0] = questPriority;
            priorities[1] = hideoutPriority;
            priorities[2] = wishlistPriority;
            priorities[3] = barterPriority;
            priorities[4] = craftPriority;
        }
    }

    /// <summary>
    /// Class to pass settings to the ConfigurationManager.
    /// Used by BepInEx.ConfigurationManager to control how settings appear in the F12 menu.
    /// Fields are assigned via reflection by ConfigurationManager, not directly in code.
    /// </summary>
#pragma warning disable CS0649 // Field is never assigned to
    internal sealed class ConfigurationManagerAttributes
    {
        public bool? ReadOnly;
        public bool? HideDefaultButton;
        public bool? HideSettingName;
        public string Category;
        public int? Order;
        public bool? Browsable;
        public string Description;
        public object DefaultValue;
        public bool? IsAdvanced;
    }
#pragma warning restore CS0649
}
