using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.Quests;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace MoreCheckmarks
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MoreCheckmarksMod : BaseUnityPlugin
    {
        // BepinEx
        public const string pluginGuid = "VIP.TommySoucy.MoreCheckmarks";
        public const string pluginName = "MoreCheckmarks";
        public const string pluginVersion = "2.1.1";

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

            Init();
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
            Logger.LogInfo("Configs loaded");

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
                LogInfo("Assets loaded");
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
            const string profileTypeString = "Class308"; // Class303
            const string derivedTypeString = "Class1596"; // Class1470
            // Get assemblies
            Type profileSelector = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var t in assemblies)
            {
                if (t.GetName().Name.Equals("Assembly-CSharp"))
                {
                    // UPDATE: This is to know when a new profile is selected so we can load up to date data
                    // We want to do this when client makes request "/client/game/profile/select"
                    // Look for that string in dnspy, this creates a callback with a method_0, that is the method we want to postfix
                    profileSelector = t.GetType(profileTypeString).GetNestedType(derivedTypeString, BindingFlags.Public);
                }
            }

            var harmony = new Harmony("VIP.TommySoucy.MoreCheckmarks");
            harmony.PatchAll(); // Auto patch

            // Manual patch
            if (profileSelector != null)
            {
                var profileSelectorOriginal =
                    profileSelector.GetMethod("method_0", BindingFlags.Public | BindingFlags.Instance);

                var profileSelectorPostfix =
                    typeof(ProfileSelectionPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

                harmony.Patch(profileSelectorOriginal, null, new HarmonyMethod(profileSelectorPostfix));
            }
            else
            {
                LogError("Failed to Patch Profile Selector - Missing profileSelector");
            }
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
                var hideoutInstance = Singleton<HideoutClass>.Instance;
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
                HideoutClass hideoutInstance = Singleton<HideoutClass>.Instance;
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
                        int level = 0;
                        const int maxLevel = 100; // Safety limit
                        while (currentStage == null && level < maxLevel)
                        {
                            currentStage = ad.StageAt(level++);
                        }
                    }

                    if (currentStage != null)
                    {
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
                    }

                    if (currentStage == null)
                    {
                        continue;
                    }

                    // UPDATE: Class here is class used in AreaData.Stage.Production.Data array
                    if (currentStage.Production != null && currentStage.Production.Data != null)
                    {
                        bool areaNameAdded = false;
                        foreach (ProductionBuildAbstractClass productionData in currentStage.Production.Data)
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
        /// Gets all prerequisite quest IDs for a given quest (recursive, with caching)
        /// </summary>
        public static HashSet<string> GetAllPrerequisites(string questId)
        {
            // Return cached result if available
            if (DataLoader.prereqCache.TryGetValue(questId, out var cached))
                return cached;

            var result = new HashSet<string>();
            var queue = new Queue<string>();

            // Add direct prerequisites to queue
            if (DataLoader.questPrerequisites.TryGetValue(questId, out var directPrereqs))
            {
                foreach (var prereq in directPrereqs)
                    queue.Enqueue(prereq);
            }

            // BFS to find all prerequisites
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (result.Contains(current)) continue;

                result.Add(current);

                if (DataLoader.questPrerequisites.TryGetValue(current, out var prereqs))
                {
                    foreach (var prereq in prereqs)
                    {
                        if (!result.Contains(prereq))
                            queue.Enqueue(prereq);
                    }
                }
            }

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

        public static void LogError(string msg)
        {
            modInstance.Logger.LogError(msg);
        }
    }
}
