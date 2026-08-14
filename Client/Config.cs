using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace MoreCheckmarks
{
    public static class MoreCheckmarksConfig
    {
        // Config Entries (BepInEx F12 menu)
        public static ConfigEntry<string> configLanguage;
        public static ConfigEntry<string> configRefreshRequired;
        public static ConfigEntry<bool> configFulfilledAnyCanBeUpgraded;
        public static ConfigEntry<bool> configOnlyShowHideoutCheckmarkOnFIR;
        public static ConfigEntry<bool> configShowHideoutCheckmarks;
        public static ConfigEntry<bool> configShowQuestCheckmarks;
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
        public static ConfigEntry<bool> configOnlyFirRequiredQuests;

        // Config settings (derived from ConfigEntry values)
        // Null-safe: the language is asked for while this very entry is still being bound
        public static string language => configLanguage?.Value;
        public static bool fulfilledAnyCanBeUpgraded => configFulfilledAnyCanBeUpgraded.Value;
        public static bool onlyShowHideoutCheckmarkOnFIR => configOnlyShowHideoutCheckmarkOnFIR.Value;
        public static bool showHideoutCheckmarks => configShowHideoutCheckmarks.Value;
        public static bool showQuestCheckmarks => configShowQuestCheckmarks.Value;
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
        public static bool onlyFirRequiredQuests => configOnlyFirRequiredQuests.Value;

        // Parsed colors (updated when config changes)
        public static Color needMoreColor = new Color(1, 0.37255f, 0.37255f);
        public static Color fulfilledColor = new Color(0.30588f, 1, 0.27843f);
        public static Color wishlistColor = new Color(0, 0, 1);
        public static Color barterColor = new Color(1, 0, 1);
        public static Color craftColor = new Color(0, 1, 1);

        // Priority and color arrays
        public static int[] priorities = { 0, 1, 2, 3, 4 };
        public static Color[] colors = { Color.yellow, needMoreColor, wishlistColor, barterColor, craftColor };

        // The attribute objects handed to ConfigurationManager, kept so their labels can be
        // refreshed once the game tells us which language it is running in
        private static readonly List<LocalizedLabel> labels = new List<LocalizedLabel>();

        private struct LocalizedLabel
        {
            public readonly ConfigurationManagerAttributes attributes;
            public readonly string sectionId;
            public readonly string settingId;

            public LocalizedLabel(ConfigurationManagerAttributes attributes, string sectionId, string settingId)
            {
                this.attributes = attributes;
                this.sectionId = sectionId;
                this.settingId = settingId;
            }
        }

        public static void Bind(ConfigFile config, List<string> availableLanguages)
        {
            labels.Clear();

            // Bound before anything else, and deliberately without going through Describe: asking for
            // a translated label settles which language to use, and that answer depends on this very
            // setting. Its labels are registered for the refresh below instead.
            var choices = new List<string> { Localization.automaticLanguage };
            choices.AddRange(availableLanguages);

            var languageAttributes = new ConfigurationManagerAttributes();
            labels.Add(new LocalizedLabel(languageAttributes, "language", "language"));

            configLanguage = config.Bind(
                "Language",
                "Language",
                Localization.automaticLanguage,
                new ConfigDescription(
                    Localization.GetEnglish("config.language.description"),
                    new AcceptableValueList<string>(choices.ToArray()),
                    languageAttributes));

            // The setting can be read now, so settle the language before binding anything that wants
            // a translated label
            Localization.RefreshLanguage();

            // Note about changes requiring menu refresh
            configRefreshRequired = config.Bind(
                "0. Important Note",
                "Refresh Required",
                Localization.GetEnglish("config.note.value"),
                Describe("note", "note", readOnly: true, hideDefaultButton: true));

            // Hideout Settings
            configShowHideoutCheckmarks = config.Bind(
                "Hideout",
                "Show Hideout Checkmarks",
                true,
                Describe("hideout", "hideout.showCheckmarks"));

            configFulfilledAnyCanBeUpgraded = config.Bind(
                "Hideout",
                "Fulfilled Any Can Be Upgraded",
                true,
                Describe("hideout", "hideout.fulfilledAnyCanBeUpgraded"));

            configShowFutureModulesLevels = config.Bind(
                "Hideout",
                "Show Future Module Levels",
                true,
                Describe("hideout", "hideout.showFutureModuleLevels"));

            configOnlyShowHideoutCheckmarkOnFIR = config.Bind(
                "Hideout",
                "Only Show Hideout Checkmark On FIR Items",
                true,
                Describe("hideout", "hideout.onlyShowCheckmarkOnFir"));

            // Quest Settings
            configShowQuestCheckmarks = config.Bind(
                "Quests",
                "Show Quest Checkmarks",
                true,
                Describe("quests", "quests.showCheckmarks"));

            configIncludeFutureQuests = config.Bind(
                "Quests",
                "Include Future Quests",
                true,
                Describe("quests", "quests.includeFuture"));

            configShowPrerequisiteQuests = config.Bind(
                "Quests",
                "Show Prerequisite Count",
                true,
                Describe("quests", "quests.showPrerequisiteCount"));

            configShowQuestCheckmarksNonFIR = config.Bind(
                "Quests",
                "Show Quest Checkmarks for Non-FIR Items",
                false,
                Describe("quests", "quests.showCheckmarksNonFir"));

            configOnlyFirRequiredQuests = config.Bind(
                "Quests",
                "Only Show FiR-Required Quests",
                false,
                Describe("quests", "quests.onlyFirRequired"));

            // Barter & Craft Settings
            configShowBarter = config.Bind(
                "Barter & Craft",
                "Show Barter",
                true,
                Describe("barterCraft", "barterCraft.showBarter"));

            configShowCraft = config.Bind(
                "Barter & Craft",
                "Show Craft",
                true,
                Describe("barterCraft", "barterCraft.showCraft"));

            configShowFutureCraft = config.Bind(
                "Barter & Craft",
                "Show Future Craft",
                true,
                Describe("barterCraft", "barterCraft.showFutureCraft"));

            // Priority Settings (higher = takes precedence, ordered by default priority)
            configQuestPriority = config.Bind(
                "Priority",
                "Quest Priority",
                4,
                Describe("priority", "priority.quest", new AcceptableValueRange<int>(0, 10), order: 5));

            configHideoutPriority = config.Bind(
                "Priority",
                "Hideout Priority",
                3,
                Describe("priority", "priority.hideout", new AcceptableValueRange<int>(0, 10), order: 4));

            configWishlistPriority = config.Bind(
                "Priority",
                "Wishlist Priority",
                2,
                Describe("priority", "priority.wishlist", new AcceptableValueRange<int>(0, 10), order: 3));

            configBarterPriority = config.Bind(
                "Priority",
                "Barter Priority",
                1,
                Describe("priority", "priority.barter", new AcceptableValueRange<int>(0, 10), order: 2));

            configCraftPriority = config.Bind(
                "Priority",
                "Craft Priority",
                0,
                Describe("priority", "priority.craft", new AcceptableValueRange<int>(0, 10), order: 1));

            // Color Settings (RGB sliders in F12 menu)
            configNeedMoreColor = config.Bind(
                "Colors",
                "Need More Color",
                new Color(1f, 0.37255f, 0.37255f),
                Describe("colors", "colors.needMore"));

            configFulfilledColor = config.Bind(
                "Colors",
                "Fulfilled Color",
                new Color(0.30588f, 1f, 0.27843f),
                Describe("colors", "colors.fulfilled"));

            configWishlistColor = config.Bind(
                "Colors",
                "Wishlist Color",
                new Color(0.23137f, 0.93725f, 1f),
                Describe("colors", "colors.wishlist"));

            configBarterColor = config.Bind(
                "Colors",
                "Barter Color",
                new Color(1f, 0f, 1f),
                Describe("colors", "colors.barter"));

            configCraftColor = config.Bind(
                "Colors",
                "Craft Color",
                new Color(0f, 1f, 1f),
                Describe("colors", "colors.craft"));

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

        /// <summary>
        /// Builds the description for a setting from the language files.
        ///
        /// The text handed to BepInEx is always the English one, because that is what gets written as
        /// a comment into the .cfg file and existing files should keep reading the same way. The
        /// translated name, category and description are passed alongside as ConfigurationManager
        /// attributes, which only affect what the F12 menu draws - the section and key that identify
        /// the setting are untouched, so existing .cfg files keep working.
        /// </summary>
        private static ConfigDescription Describe(string sectionId, string settingId,
            AcceptableValueBase acceptableValues = null, int? order = null,
            bool? readOnly = null, bool? hideDefaultButton = null)
        {
            var attributes = new ConfigurationManagerAttributes
            {
                Order = order,
                ReadOnly = readOnly,
                HideDefaultButton = hideDefaultButton
            };

            labels.Add(new LocalizedLabel(attributes, sectionId, settingId));
            Apply(labels[labels.Count - 1]);

            return new ConfigDescription(
                Localization.GetEnglish("config." + settingId + ".description"),
                acceptableValues,
                attributes);
        }

        /// <summary>
        /// Re-reads every F12 label from the language files. Settings are bound while the game is
        /// still starting, before it can say which language it runs in, so the labels are filled in
        /// with English and corrected here once the answer is known. ConfigurationManager reads these
        /// attributes when it builds its window, which is always long after that.
        /// </summary>
        public static void RefreshLabels()
        {
            foreach (LocalizedLabel label in labels)
            {
                Apply(label);
            }

            // The note's value is displayed text rather than a stored preference, so it follows the
            // language too. Assigning it rewrites the .cfg entry, which is harmless for a note.
            if (configRefreshRequired != null)
            {
                configRefreshRequired.Value = Localization.Get("config.note.value");
            }
        }

        private static void Apply(LocalizedLabel label)
        {
            label.attributes.Category = Localization.Get("config.section." + label.sectionId);
            label.attributes.DispName = Localization.Get("config." + label.settingId + ".name");
            label.attributes.Description = Localization.Get("config." + label.settingId + ".description");
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
        // Localized labels. These only change what the F12 menu draws; the section and key that
        // identify a setting in the .cfg file are set when binding and are not affected.
        public string DispName;
        public string Category;
        public string Description;
        public bool? ReadOnly;
        public bool? HideDefaultButton;
        public int? Order;
    }
#pragma warning restore CS0649
}
