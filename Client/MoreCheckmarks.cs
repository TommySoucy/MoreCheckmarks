using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.Quests;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MoreCheckmarks
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInDependency(SptCoreGuid, "4.1.0")]
    public class MoreCheckmarksMod : BaseUnityPlugin
    {
        // BepinEx
        public const string pluginGuid = "VIP.TommySoucy.MoreCheckmarks";
        public const string pluginName = "MoreCheckmarks";
        // Generated from $(ModVersion) in the csproj - see the GeneratePluginInfo target.
        public const string pluginVersion = PluginInfo.Version;
        private const string SptCoreGuid = "com.SPT.core";

        // Assets
        public static Sprite whiteCheckmark;
        private static TMP_FontAsset benderBold;
        public static string modPath;

        // Live
        public static MoreCheckmarksMod modInstance;

        private void Start()
        {
            Logger.LogInfo("MoreCheckmarks Started");

            modInstance = this;

            // The [BepInDependency] above pins a minimum SPT. This pins a maximum: 4.1 de-obfuscated
            // large parts of Assembly-CSharp, and this build reads members by their 4.1 names. On a
            // newer SPT those reads would silently return defaults rather than fail, so refuse to
            // patch at all instead of misbehaving quietly.
            if (!SptVersionSupported())
            {
                enabled = false;
                return;
            }

            Init();
        }

        /// <summary>
        /// True when the running SPT is 4.1.x. Patches target 4.1 member names, so a newer
        /// minor version is refused rather than trusted.
        /// </summary>
        private bool SptVersionSupported()
        {
            if (!Chainloader.PluginInfos.TryGetValue(SptCoreGuid, out var sptCore) || sptCore == null)
            {
                Logger.LogError("Could not determine the SPT version (" + SptCoreGuid +
                                " not loaded) - MoreCheckmarks will not patch.");
                return false;
            }

            var version = sptCore.Metadata.Version;
            if (version.Major != 4 || version.Minor != 1)
            {
                Logger.LogError("MoreCheckmarks " + pluginVersion + " supports SPT 4.1.x but found SPT " +
                                version + " - not patching. Install a build of MoreCheckmarks made for this SPT version.");
                return false;
            }

            return true;
        }

        private void Init()
        {
            modPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(MoreCheckmarksMod)).Location);
            if (modPath == null)
            {
                Logger.LogError("MoreCheckmarks Mod Path is null");
                return;
            }

            modPath = modPath.Replace('\\', '/');

            MoreCheckmarksConfig.Bind(Config);
            Logger.LogDebug("Configs loaded");

            LoadAssets();

            DataLoader.LoadData();

            DoPatching();
        }

        private void LoadAssets()
        {
            var assetBundle = AssetBundle.LoadFromFile(modPath + "/MoreCheckmarksAssets");

            if (assetBundle == null)
            {
                LogError("Failed to load assets, inspect window checkmark may be miscolored");
            }
            else
            {
                whiteCheckmark = assetBundle.LoadAsset<Sprite>("WhiteCheckmark");
                benderBold = assetBundle.LoadAsset<TMP_FontAsset>("BenderBold");
                TMP_Text.OnFontAssetRequest += TMP_Text_onFontAssetRequest;
                LogDebug("Assets loaded");
            }
        }

        public static TMP_FontAsset TMP_Text_onFontAssetRequest(int hash, string name)
        {
            if (name.Equals("BENDERBOLD"))
            {
                return benderBold;
            }
            else
            {
                return null;
            }
        }

        private static void DoPatching()
        {
            // This is to know when a new profile is selected so we can load up to date data.
            // EftClientBackendSession.SetMainProfile sends "/client/game/profile/select"; the method we
            // want is method_0 on its compiler-generated callback closure, which runs once the request
            // has succeeded and the new profile is live. Postfixing SetMainProfile itself would fire at
            // the first await, before the profile is set.
            var profileSelector =
                typeof(EftClientBackendSession).GetNestedType("CG_SetMainProfile", BindingFlags.Public);

            var harmony = new Harmony("VIP.TommySoucy.MoreCheckmarks");
            harmony.PatchAll(); // Auto patch

            // Manual patch
            var profileSelectorOriginal =
                profileSelector?.GetMethod("method_0", BindingFlags.Public | BindingFlags.Instance);

            if (profileSelectorOriginal != null)
            {
                var profileSelectorPostfix =
                    typeof(ProfileSelectionPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

                harmony.Patch(profileSelectorOriginal, null, new HarmonyMethod(profileSelectorPostfix));
            }
            else
            {
                LogError("Failed to Patch Profile Selector - EftClientBackendSession.CG_SetMainProfile.method_0 not found");
            }
        }

        /// <summary>
        /// True when a raid is in progress. Out of raid the game singleton is absent.
        /// </summary>
        public static bool IsInRaid()
        {
            return Singleton<AbstractGame>.Instance?.InRaid ?? false;
        }

        /// <summary>
        /// Live count of an item across the stash and the character ("on you").
        /// Out of raid the stash is read live from the profile inventory (so moves are
        /// reflected immediately). In raid the stash is taken from the frozen
        /// AllStashItems snapshot (it cannot change mid-raid); only the small equipment
        /// set is queried live.
        /// </summary>
        public static ItemCounts GetItemCounts(Profile profile, string templateId)
        {
            // On you: always live. Works in and out of raid.
            var (equipFir, equipTotal) = InventoryCounts.SumStacks(
                profile.Inventory.GetPlayerItems(EPlayerItems.Equipment)
                    .Where(x => x.TemplateId == templateId)
                    .Select(x => (x.StackObjectsCount, x.MarkedAsSpawnedInSession)));

            // Stash: live out of raid; frozen snapshot in raid.
            IEnumerable<Item> stashItems = IsInRaid()
                ? Singleton<HideoutRepresentation>.Instance?.AllStashItems ?? Enumerable.Empty<Item>()
                : profile.Inventory.GetPlayerItems(
                      EPlayerItems.Stash | EPlayerItems.HideoutStashes | EPlayerItems.SortingTable);

            var (stashFir, stashTotal) = InventoryCounts.SumStacks(
                stashItems
                    .Where(x => x.TemplateId == templateId)
                    .Select(x => (x.StackObjectsCount, x.MarkedAsSpawnedInSession)));

            return new ItemCounts
            {
                stashFir = stashFir,
                stashTotal = stashTotal,
                equipmentFir = equipFir,
                equipmentTotal = equipTotal,
            };
        }

        public static NeededStruct GetNeeded(string itemTemplateID, ref List<string> areaNames)
        {
            var neededStruct = new NeededStruct
            {
                possessedCount = 0,
                requiredCount = 0
            };

            try
            {
                var hideoutInstance = Singleton<HideoutRepresentation>.Instance;
                if (hideoutInstance?.AreaDatas == null)
                {
                    return neededStruct;
                }

                foreach (var ad in hideoutInstance.AreaDatas)
                {
                    // Skip if don't have area data
                    if (ad == null || ad.Template == null || ad.Template.Name == null || ad.NextStage == null)
                    {
                        continue;
                    }

                    // Skip if the area has no future upgradez
                    if (ad.Status == EAreaStatus.NoFutureUpgrades)
                    {
                        continue;
                    }

                    // Collect all future stages
                    var futureStages = new List<Stage>();
                    var lastStage = ad.CurrentStage;
                    while ((lastStage = ad.StageAt(lastStage.Level + 1)) != null && lastStage.Level != 0)
                    {
                        // Don't want to check requirements for an area we are currently constructing/upgrading
                        if (ad.Status == EAreaStatus.Constructing ||
                            ad.Status == EAreaStatus.Upgrading)
                        {
                            continue;
                        }

                        futureStages.Add(lastStage);

                        // If only want next level requirements, skip the rest
                        if (!MoreCheckmarksConfig.showFutureModulesLevels)
                        {
                            break;
                        }
                    }

                    // Skip are if no stages were found to check requirements for
                    if (futureStages.Count == 0)
                    {
                        continue;
                    }

                    // Check requirements
                    foreach (var stage in futureStages)
                    {
                        var requirements = stage.Requirements;

                        try
                        {
                            foreach (var requirement in requirements)
                            {
                                if (!(requirement is ItemRequirement itemRequirement)) continue;
                                var requirementTemplate = itemRequirement.TemplateId;
                                if (itemTemplateID != requirementTemplate) continue;
                                // Sum up the total amount of this item required in entire hideout and update possessed amount
                                neededStruct.requiredCount += itemRequirement.IntCount;
                                neededStruct.possessedCount = itemRequirement.UserItemsCount;

                                // A requirement but already have the amount we need
                                if (requirement.Fulfilled)
                                {
                                    // Even if we have enough of this item to fulfill a requirement in one area
                                    // we might still need it, and if thats the case we want to show that color, not fulfilled color, so you know you still need more of it
                                    // So only set color to fulfilled if not needed
                                    if (!neededStruct.foundNeeded && !neededStruct.foundFulfilled)
                                    {
                                        neededStruct.foundFulfilled = true;
                                    }

                                    areaNames?.Add("<color=#" +
                                                   ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.fulfilledColor) + ">" +
                                                   ad.Template.Name +
                                                   " lvl" + stage.Level + "</color>");
                                }
                                else
                                {
                                    if (!neededStruct.foundNeeded)
                                    {
                                        neededStruct.foundNeeded = true;
                                    }

                                    areaNames?.Add("<color=#" +
                                                   ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.needMoreColor) + ">" +
                                                   ad.Template.Name +
                                                   " lvl" + stage.Level + "</color>");
                                }
                            }
                        }
                        catch (Exception)
                        {
                            LogError("Failed to get whether item " + itemTemplateID +
                                     " was needed for hideout area: " + ad.Template.Name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError("Failed to find out if item needed for upgrade - - - -" + e.StackTrace);
                LogError("Failed to get whether item " + itemTemplateID + " was needed for hideout upgrades.");
            }

            return neededStruct;
        }

        public static bool GetNeededCraft(string itemTemplateID, ref string tooltip, bool needTooltip = true)
        {
            bool required = false;
            bool gotTooltip = false;
            try
            {
                HideoutRepresentation hideoutInstance = Singleton<HideoutRepresentation>.Instance;
                if (hideoutInstance?.AreaDatas == null)
                {
                    return false;
                }

                foreach (AreaData ad in hideoutInstance.AreaDatas)
                {
                    // Skip if don't have area data
                    if (ad == null || ad.Template == null || ad.Template.Name == null)
                    {
                        continue;
                    }

                    // Get stage to check productions of
                    // Productions are cumulative, a stage will have productions of all previous stages
                    Stage currentStage = ad.CurrentStage;
                    if (currentStage == null)
                    {
                        continue;
                    }

                    Stage newStage = ad.StageAt(currentStage.Level + 1);
                    while (newStage != null && newStage.Level != 0)
                    {
                        if (newStage.Level > ad.CurrentLevel && !MoreCheckmarksConfig.showFutureCraft)
                        {
                            break;
                        }

                        currentStage = newStage;
                        newStage = ad.StageAt(currentStage.Level + 1);
                    }

                    // UPDATE: Class here is class used in AreaData.Stage.Production.Data array
                    if (currentStage.Production != null && currentStage.Production.Data != null)
                    {
                        bool areaNameAdded = false;
                        foreach (BaseHideoutScheme productionData in currentStage.Production.Data)
                        {
                            Requirement[] requirements = productionData.requirements;

                            foreach (Requirement baseReq in requirements)
                            {
                                if (baseReq.Type == ERequirementType.Item)
                                {
                                    ItemRequirement itemRequirement = baseReq as ItemRequirement;

                                    if (itemTemplateID == itemRequirement.TemplateId)
                                    {
                                        required = true;

                                        if (needTooltip)
                                        {
                                            if (DataLoader.productionEndProductByID.TryGetValue(productionData._id,
                                                    out string product))
                                            {
                                                gotTooltip = true;
                                                if (!areaNameAdded)
                                                {
                                                    tooltip += "\n  " + ad.Template.Name.Localized();
                                                    areaNameAdded = true;
                                                }

                                                tooltip += "\n    <color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.craftColor) +
                                                           ">" + (product + " Name").Localized() + " lvl" +
                                                           productionData.Level + "</color> (" +
                                                           itemRequirement.IntCount + ")";
                                            }
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to get whether item " + itemTemplateID +
                         " was needed for crafting: " + ex.Message);
            }

            return required && gotTooltip;
        }

        public static List<List<KeyValuePair<string, int>>> GetBarters(string ID)
        {
            var bartersByTrader = new List<List<KeyValuePair<string, int>>>();

            if (MoreCheckmarksConfig.showBarter)
            {
                for (var i = 0; i < DataLoader.bartersByItemByTrader.Count; ++i)
                {
                    List<KeyValuePair<string, int>> current = null;

                    DataLoader.bartersByItemByTrader[i]?.TryGetValue(ID, out current);

                    if (current == null)
                    {
                        current = new List<KeyValuePair<string, int>>();
                    }

                    bartersByTrader.Add(current);
                }
            }

            return bartersByTrader;
        }

        /// <summary>
        /// Resolves which checkmark color an item should get based on what it's needed for and
        /// the configured priorities. Returns false if the item isn't needed for anything.
        /// </summary>
        public static bool TryGetCheckmarkColor(bool questItem, NeededStruct neededStruct, bool wishlist,
            bool gotBarters, bool craftRequired, bool itemIsFir, out Color color)
        {
            color = Color.white;

            var hideoutNeeded = MoreCheckmarksConfig.showHideoutCheckmarks &&
                                (neededStruct.foundNeeded || neededStruct.foundFulfilled) &&
                                (!MoreCheckmarksConfig.onlyShowHideoutCheckmarkOnFIR || itemIsFir);

            var neededFor = new[]
            {
                questItem,
                hideoutNeeded,
                wishlist,
                gotBarters,
                craftRequired
            };

            var currentNeeded = -1;
            var currentHighest = -1;
            for (var i = 0; i < 5; ++i)
            {
                if (!neededFor[i] || MoreCheckmarksConfig.priorities[i] <= currentHighest) continue;
                currentNeeded = i;
                currentHighest = MoreCheckmarksConfig.priorities[i];
            }

            if (currentNeeded == -1)
            {
                return false;
            }

            // Handle special case of hideout areas
            if (currentNeeded == 1)
            {
                if (neededStruct.foundNeeded) // Need more
                {
                    color = MoreCheckmarksConfig.needMoreColor;
                }
                // We have enough for at least one upgrade. Show fulfilled if we either only care
                // about a single upgrade being possible, or we truly have enough across the board.
                else if (MoreCheckmarksConfig.fulfilledAnyCanBeUpgraded ||
                         neededStruct.possessedCount >= neededStruct.requiredCount)
                {
                    color = MoreCheckmarksConfig.fulfilledColor;
                }
                else // Still need more
                {
                    color = MoreCheckmarksConfig.needMoreColor;
                }
            }
            else // Not area, just use its color
            {
                color = MoreCheckmarksConfig.colors[currentNeeded];
            }

            return true;
        }

        /// <summary>
        /// Gets all prerequisite quest IDs for a given quest (recursive, with caching)
        /// </summary>
        public static HashSet<string> GetAllPrerequisites(string questId)
        {
            // Return cached result if available
            if (DataLoader.prereqCache.TryGetValue(questId, out var cached))
                return cached;

            var result = QuestPrerequisites.ComputeAll(questId, DataLoader.questPrerequisites);

            // Cache and return
            DataLoader.prereqCache[questId] = result;
            return result;
        }

        /// <summary>
        /// Gets the count of remaining (incomplete) prerequisite quests
        /// </summary>
        public static int GetRemainingPrerequisiteCount(string questId, Profile profile)
        {
            var allPrereqs = GetAllPrerequisites(questId);
            int remaining = 0;

            foreach (var prereqId in allPrereqs)
            {
                // Use the cached IsQuestCompleted check
                if (!IsQuestCompleted(prereqId, profile))
                {
                    remaining++;
                }
            }

            return remaining;
        }

        /// <summary>
        /// Checks if a quest is completed (Success status). Caches true results since quests can't be un-completed.
        /// </summary>
        public static bool IsQuestCompleted(string questId, Profile profile)
        {
            // Check cache first - if we've seen this quest completed before, it's still completed
            if (DataLoader.completedQuestIds.Contains(questId))
                return true;

            foreach (var questDataClass in profile.QuestsData)
            {
                if (questDataClass.Template != null && questDataClass.Template.Id == questId)
                {
                    if (questDataClass.Status == EQuestStatus.Success)
                    {
                        // Cache the result - quest completion is permanent
                        DataLoader.completedQuestIds.Add(questId);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the prerequisite status string for display in tooltip (uses pre-computed count)
        /// </summary>
        public static string GetPrerequisiteStatusString(int remaining)
        {
            // If feature is disabled, return empty string
            if (!MoreCheckmarksConfig.showPrerequisiteQuests)
            {
                return "";
            }

            if (remaining == 0)
            {
                // Green for available quests
                return " <color=#00ff00>(0 prereqs)</color>";
            }
            else if (remaining < 10)
            {
                // Yellow for quests with few prerequisites (1-9)
                return $" <color=#ffff00>({remaining} prereq{(remaining == 1 ? "" : "s")})</color>";
            }
            else
            {
                // Gray for quests with many prerequisites (10+)
                return $" <color=#888888>({remaining} prereqs)</color>";
            }
        }

        public static void LogInfo(string msg)
        {
            modInstance.Logger.LogInfo(msg);
        }

        public static void LogDebug(string msg)
        {
            modInstance.Logger.LogDebug(msg);
        }

        public static void LogError(string msg)
        {
            modInstance.Logger.LogError(msg);
        }
    }
}
