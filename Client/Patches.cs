using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Quests;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoreCheckmarks
{
    [HarmonyPatch]
    class QuestItemViewPanelShowPatch
    {
        // Replaces the original QuestItemViewPanel.Show() to use custom checkmark colors and tooltips
        [HarmonyPatch(typeof(QuestItemViewPanel), nameof(QuestItemViewPanel.Show))]
        static bool Prefix(
            Profile profile,
            Item item,
            SimpleTooltip tooltip,
            QuestItemViewPanel __instance,
            ref Image ____questIconImage,
            ref Sprite ____foundInRaidSprite,
            ref string ___string_5,
            ref SimpleTooltip ___simpleTooltip_0,
            TextMeshProUGUI ____questItemLabel)
        {
            try
            {
                var possessedCount = 0;
                var possessedQuestCount = 0;

                __instance.HideGameObject();

                if (profile != null)
                {
                    var inventoryItems = Singleton<HideoutClass>.Instance.AllStashItems.Where(x => x.TemplateId == item.TemplateId);
                    foreach (var currentItem in inventoryItems)
                    {
                        if (currentItem.MarkedAsSpawnedInSession)
                        {
                            possessedQuestCount += currentItem.StackObjectsCount;
                        }

                        possessedCount += currentItem.StackObjectsCount;
                    }
                }
                else
                {
                    MoreCheckmarksMod.LogError("Profile null for item " + item.Template.Name);
                }

                var areaNames = new List<string>();
                var neededStruct = MoreCheckmarksMod.GetNeeded(item.TemplateId, ref areaNames);
                DataLoader.questDataStartByItemTemplateID.TryGetValue(item.TemplateId,
                    out var startQuests);
                DataLoader.questDataCompleteByItemTemplateID.TryGetValue(item.TemplateId,
                    out var completeQuests);

                var wishlist = ItemUiContext.Instance.WishlistManager.IsInWishlist(item.TemplateId, true, out _);

                var craftTooltip = "";
                var craftRequired = MoreCheckmarksConfig.showCraft &&
                                     MoreCheckmarksMod.GetNeededCraft(item.TemplateId, ref craftTooltip);

                var questItem = (item.MarkedAsSpawnedInSession || MoreCheckmarksConfig.showQuestCheckmarksNonFIR) &&
                                ((item.QuestItem || MoreCheckmarksConfig.includeFutureQuests)
                                    ? (startQuests != null && startQuests.questData.Count > 0) ||
                                      (completeQuests != null && completeQuests.questData.Count > 0)
                                    : ___string_5 != null && ___string_5.Contains("quest"));


                if (____questItemLabel != null)
                {
                    // Since being quest item could be set by future quests, need to make sure we have "QUEST ITEM" label
                    if (questItem)
                    {
                        ____questItemLabel.text = "QUEST ITEM";
                    }

                    ____questItemLabel.gameObject.SetActive(questItem);
                }

                var bartersByTrader = MoreCheckmarksMod.GetBarters(item.TemplateId);
                var gotBarters = false;
                if (bartersByTrader != null)
                {
                    if (bartersByTrader.Any(t => t != null && t.Count > 0))
                    {
                        gotBarters = true;
                    }
                }

                MoreCheckmarksConfig.neededFor[0] = questItem;
                MoreCheckmarksConfig.neededFor[1] = neededStruct.foundNeeded || neededStruct.foundFulfilled;
                MoreCheckmarksConfig.neededFor[2] = wishlist;
                MoreCheckmarksConfig.neededFor[3] = gotBarters;
                MoreCheckmarksConfig.neededFor[4] = craftRequired;

                var currentNeeded = -1;
                var currentHighest = -1;

                for (var i = 0; i < 5; ++i)
                {
                    if (!MoreCheckmarksConfig.neededFor[i] || MoreCheckmarksConfig.priorities[i] <= currentHighest) continue;
                    currentNeeded = i;
                    currentHighest = MoreCheckmarksConfig.priorities[i];
                }

                if (currentNeeded > -1)
                {
                    // Handle special case of areas
                    if (currentNeeded == 1)
                    {
                        if (neededStruct.foundNeeded) // Need more
                        {
                            SetCheckmark(__instance, ____questIconImage, ____foundInRaidSprite,
                                MoreCheckmarksConfig.needMoreColor);
                        }
                        else if (neededStruct.foundFulfilled) // We have enough for at least one upgrade
                        {
                            if (MoreCheckmarksConfig.fulfilledAnyCanBeUpgraded) // We want to know when have enough for at least one upgrade
                            {
                                SetCheckmark(__instance, ____questIconImage, ____foundInRaidSprite,
                                    MoreCheckmarksConfig.fulfilledColor);
                            }
                            else // We only want fulfilled checkmark when ALL requiring this item can be upgraded
                            {
                                // Check if we truly do not need more of this item for now
                                if (neededStruct.possessedCount >= neededStruct.requiredCount)
                                {
                                    SetCheckmark(__instance, ____questIconImage, ____foundInRaidSprite,
                                        MoreCheckmarksConfig.fulfilledColor);
                                }
                                else // Still need more
                                {
                                    SetCheckmark(__instance, ____questIconImage, ____foundInRaidSprite,
                                        MoreCheckmarksConfig.needMoreColor);
                                }
                            }
                        }
                    }
                    else // Not area, just set color
                    {
                        SetCheckmark(__instance, ____questIconImage, ____foundInRaidSprite,
                            MoreCheckmarksConfig.colors[currentNeeded]);
                    }
                }
                else if (item.MarkedAsSpawnedInSession) // Item not needed for anything but found in raid
                {
                    SetCheckmark(__instance, ____questIconImage, ____foundInRaidSprite, Color.white);
                }

                SetTooltip(profile, areaNames, ref ___string_5, ref ___simpleTooltip_0, ref tooltip, item, startQuests,
                    completeQuests, possessedCount, possessedQuestCount, neededStruct.requiredCount, wishlist,
                    bartersByTrader, gotBarters, craftRequired, craftTooltip);

                return false;
            }
            catch (Exception e)
            {
                MoreCheckmarksMod.LogError(
                    "Failed to show checkmark for item " + item.Template.Name + " - " + e.Message);
                return true;
            }
        }

        private static void SetCheckmark(QuestItemViewPanel instance, Image questIconImage,
            Sprite sprite, Color color)
        {
            try
            {
                // Following calls base class method ShowGameObject()
                instance.ShowGameObject();
                questIconImage.sprite = sprite;
                questIconImage.color = color;
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError($"SetCheckmark failed: {ex.Message}");
            }
        }

        private static void SetTooltip(Profile profile, List<string> areaNames, ref string ___string_5,
            ref SimpleTooltip ___simpleTooltip_0, ref SimpleTooltip tooltip,
            Item item, QuestPair startQuests,
            QuestPair completeQuests,
            int possessedCount, int possessedQuestCount, int requiredCount, bool wishlist,
            List<List<KeyValuePair<string, int>>> bartersByTrader, bool gotBarters,
            bool craftRequired, string craftTooltip)
        {
            try
            {
                // Reset string
                ___string_5 = "STASH".Localized(null) + ": <color=#dd831a>" + possessedQuestCount + "</color>/" +
                              possessedCount;

                // Show found in raid if found in raid
                if (item.MarkedAsSpawnedInSession)
                {
                    ___string_5 += "\n" + "Item found in raid".Localized(null);
                }

                // Add quests
                var gotQuest = false;
                if (item.MarkedAsSpawnedInSession || MoreCheckmarksConfig.showQuestCheckmarksNonFIR)
                {
                    if (MoreCheckmarksConfig.includeFutureQuests)
                    {
                        var questStartString = "<color=#dd831a>";
                        var gotStartQuests = false;
                        var gotMoreThanOneStartQuest = false;
                        var totalItemCount = 0;
                        if (startQuests != null)
                        {
                            // Filter out completed quests
                            var filteredStartQuests = startQuests.questData
                                .Where(q => !MoreCheckmarksMod.IsQuestCompleted(q.Value.questId, profile));

                            // Only compute and sort by prereq counts if feature is enabled
                            var startQuestsWithPrereqs = MoreCheckmarksConfig.showPrerequisiteQuests
                                ? filteredStartQuests
                                    .Select(q => new { Entry = q, PrereqCount = MoreCheckmarksMod.GetRemainingPrerequisiteCount(q.Value.questId, profile) })
                                    .OrderBy(q => q.PrereqCount)
                                    .ToList()
                                : filteredStartQuests
                                    .Select(q => new { Entry = q, PrereqCount = 0 })
                                    .ToList();
                            var count = startQuestsWithPrereqs.Count;

                            if (count > 0)
                            {
                                gotStartQuests = true;
                                totalItemCount = startQuests.count;
                            }

                            if (count > 1)
                            {
                                gotMoreThanOneStartQuest = true;
                            }

                            var index = 0;
                            foreach (var questWithPrereq in startQuestsWithPrereqs)
                            {
                                var questEntry = questWithPrereq.Entry;
                                var localizedName = questEntry.Key.Localized(null);
                                if (questEntry.Key.Equals(localizedName))
                                {
                                    // Could not localize name, just use default name
                                    if (string.IsNullOrEmpty(questEntry.Value.questName))
                                    {
                                        questStartString += "Unknown Quest";
                                    }
                                    else
                                    {
                                        questStartString += questEntry.Value.questName;
                                    }
                                }
                                else
                                {
                                    questStartString += localizedName;
                                }

                                // Add prerequisite status (using pre-computed count, empty string if disabled)
                                questStartString += MoreCheckmarksMod.GetPrerequisiteStatusString(questWithPrereq.PrereqCount);

                                if (index != count - 1)
                                {
                                    questStartString += ",\n  ";
                                }
                                else
                                {
                                    questStartString += "</color>";
                                }

                                ++index;
                            }
                        }

                        if (gotStartQuests)
                        {
                            gotQuest = true;
                            ___string_5 = "\nNeeded (" + possessedQuestCount + "/" + totalItemCount +
                                          ") to start quest" + (gotMoreThanOneStartQuest ? "s" : "") + ":\n  " +
                                          questStartString;
                        }

                        var questCompleteString = "<color=#dd831a>";
                        var gotCompleteQuests = false;
                        var gotMoreThanOneCompleteQuest = false;
                        if (completeQuests != null)
                        {
                            // Filter out completed quests
                            var filteredCompleteQuests = completeQuests.questData
                                .Where(q => !MoreCheckmarksMod.IsQuestCompleted(q.Value.questId, profile));

                            // Only compute and sort by prereq counts if feature is enabled
                            var completeQuestsWithPrereqs = MoreCheckmarksConfig.showPrerequisiteQuests
                                ? filteredCompleteQuests
                                    .Select(q => new { Entry = q, PrereqCount = MoreCheckmarksMod.GetRemainingPrerequisiteCount(q.Value.questId, profile) })
                                    .OrderBy(q => q.PrereqCount)
                                    .ToList()
                                : filteredCompleteQuests
                                    .Select(q => new { Entry = q, PrereqCount = 0 })
                                    .ToList();
                            var count = completeQuestsWithPrereqs.Count;

                            if (count > 0)
                            {
                                gotCompleteQuests = true;
                                totalItemCount = completeQuests.count;
                            }

                            if (count > 1)
                            {
                                gotMoreThanOneCompleteQuest = true;
                            }

                            var index = 0;
                            foreach (var questWithPrereq in completeQuestsWithPrereqs)
                            {
                                var questEntry = questWithPrereq.Entry;
                                var localizedName = questEntry.Key.Localized(null);
                                if (questEntry.Key.Equals(localizedName))
                                {
                                    // Could not localize name, just use default name
                                    if (string.IsNullOrEmpty(questEntry.Value.questName))
                                    {
                                        questCompleteString += "Unknown Quest";
                                    }
                                    else
                                    {
                                        questCompleteString += questEntry.Value.questName;
                                    }
                                }
                                else
                                {
                                    questCompleteString += localizedName;
                                }

                                // Add prerequisite status (using pre-computed count, empty string if disabled)
                                questCompleteString += MoreCheckmarksMod.GetPrerequisiteStatusString(questWithPrereq.PrereqCount);

                                if (index != count - 1)
                                {
                                    questCompleteString += ",\n  ";
                                }
                                else
                                {
                                    questCompleteString += "</color>";
                                }

                                ++index;
                            }
                        }

                        if (gotCompleteQuests)
                        {
                            gotQuest = true;
                            ___string_5 += "\nNeeded (" + possessedQuestCount + "/" + totalItemCount +
                                           ") to complete quest" + (gotMoreThanOneCompleteQuest ? "s" : "") + ":\n  " +
                                           questCompleteString;
                        }
                    }
                    else // Don't include future quests, do as vanilla
                    {
                        RawQuestClass RawQuestClass = null;
                        ConditionItem conditionItem = null;
                        foreach (QuestDataClass questDataClass in profile.QuestsData)
                        {
                            if (questDataClass.Status == EQuestStatus.Started && questDataClass.Template != null)
                            {
                                // UPDATE: Look for the type used in QuestDataClass's Template var of type RawQuestClass with QuestConditionsList, for the value
                                foreach (KeyValuePair<EQuestStatus, GClass1631> kvp in questDataClass.Template.Conditions)
                                {
                                    kvp.Deconstruct(out EQuestStatus equestStatus, out GClass1631 gclass);
                                    foreach (Condition condition in gclass)
                                    {
                                        ConditionItem conditionItem2;
                                        if (!questDataClass.CompletedConditions.Contains(condition.id) &&
                                            (conditionItem2 = (condition as ConditionItem)) != null &&
                                            conditionItem2.target.Contains(item.StringTemplateId))
                                        {
                                            RawQuestClass = questDataClass.Template;
                                            conditionItem = conditionItem2;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (RawQuestClass != null)
                        {
                            string arg = "<color=#dd831a>" + RawQuestClass.Name + "</color>";
                            if (item.QuestItem)
                            {
                                gotQuest = true;
                                ___string_5 += string.Format("\nItem is related to an active {0} quest".Localized(null),
                                    arg);
                            }

                            Weapon weapon;
                            ConditionWeaponAssembly condition;
                            if (!gotQuest && (weapon = (item as Weapon)) != null &&
                                (condition = (conditionItem as ConditionWeaponAssembly)) != null &&
                                Inventory.IsWeaponFitsCondition(weapon, condition, false))
                            {
                                gotQuest = true;
                                ___string_5 +=
                                    string.Format("\nItem fits the active {0} quest requirements".Localized(null), arg);
                            }

                            if (!gotQuest && item.MarkedAsSpawnedInSession)
                            {
                                gotQuest = true;
                                ___string_5 +=
                                    string.Format(
                                        "\nItem that has been found in raid for the {0} quest".Localized(null), arg);
                            }
                        }
                    }
                }

                // Add areas
                var gotAreas = areaNames.Count > 0;
                var areaNamesString = "";
                for (var i = 0; i < areaNames.Count; ++i)
                {
                    areaNamesString += "\n  " + areaNames[i];
                }

                if (!areaNamesString.Equals(""))
                {
                    ___string_5 +=
                        string.Format("\nNeeded ({1}/{2}) for area" + (areaNames.Count == 1 ? "" : "s") + ":{0}",
                            areaNamesString, possessedCount, requiredCount);
                }

                // Add wishlist
                if (wishlist)
                {
                    ___string_5 += string.Format("\nOn {0}",
                        "<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.wishlistColor) +
                        ">Wish List</color>");
                }

                // Add craft
                if (craftRequired)
                {
                    ___string_5 += string.Format("\nNeeded for crafting:{0}", craftTooltip);
                }

                // Add barters
                if (gotBarters)
                {
                    var firstBarter = false;
                    if (bartersByTrader != null)
                    {
                        for (var i = 0; i < bartersByTrader.Count; ++i)
                        {
                            if (bartersByTrader[i] != null && bartersByTrader[i].Count > 0)
                            {
                                if (!firstBarter)
                                {
                                    ___string_5 += "\n" + "Barter".Localized(null) + ":";
                                    firstBarter = true;
                                }

                                var bartersString = "\n With " + (DataLoader.traders.Length > i
                                    ? DataLoader.traders[i]
                                    : "Custom Trader " + i) + ":";
                                for (var j = 0; j < bartersByTrader[i].Count; ++j)
                                {
                                    bartersString += "\n  <color=#" +
                                                     ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.barterColor) + ">" +
                                                     (bartersByTrader[i][j].Key + " Name").Localized() + "</color> (" +
                                                     bartersByTrader[i][j].Value + ")";
                                }

                                ___string_5 += bartersString;
                            }
                        }
                    }
                }

                if (gotQuest || gotAreas || wishlist || gotBarters || craftRequired || item.MarkedAsSpawnedInSession)
                {
                    // If this is not a quest item or found in raid, the original returns and the tooltip never gets set, so we need to set it ourselves
                    ___simpleTooltip_0 = tooltip;
                }
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError($"SetTooltip failed: {ex.Message}");
            }
        }
    }

    [HarmonyPatch]
    class ItemSpecificationPanelShowPatch
    {
        // This postfix will run after the inspect window sets its checkmark if there is one
        // If there is one, the postfix for the QuestItemViewPanel will always have run before
        // This patch just changes the sprite to a default white one so we can set its color to whatever we need
        [HarmonyPatch(typeof(ItemSpecificationPanel), "method_2")]
        static void Postfix(ref Item ___item_0, ref QuestItemViewPanel ____questItemViewPanel)
        {
            try
            {
                // If the checkmark exists and if the color of the checkmark is custom
                if (____questItemViewPanel != null)
                {
                    // Get access to QuestItemViewPanel's private _questIconImage
                    var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                    BindingFlags.Static;
                    var iconImageField = typeof(QuestItemViewPanel).GetField("_questIconImage", bindFlags);
                    var _questIconImage = iconImageField.GetValue(____questItemViewPanel) as Image;

                    if (_questIconImage != null)
                    {
                        _questIconImage.sprite = MoreCheckmarksMod.whiteCheckmark;
                    }
                }
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError($"ItemSpecificationPanelShowPatch failed: {ex.Message}");
            }
        }
    }

    [HarmonyPatch]
    class AvailableActionsPatch
    {
        // This postfix will run after we get a list of all actions available to interact with the item we are pointing at
        [HarmonyPatch(typeof(GetActionsClass), "smethod_8")]
        static void Postfix(GamePlayerOwner owner, LootItem lootItem, ref ActionsReturnClass __result)
        {
            try
            {
                foreach (ActionsTypesClass action in __result.Actions)
                {
                    if (action.Name.Equals("Take"))
                    {
                        List<string> nullAreaNames = null;
                        NeededStruct neededStruct = MoreCheckmarksMod.GetNeeded(lootItem.TemplateId, ref nullAreaNames);
                        string craftTooltip = "";
                        bool craftRequired =
                            MoreCheckmarksMod.GetNeededCraft(lootItem.TemplateId, ref craftTooltip, false);
                        bool wishlist =
                            ItemUiContext.Instance.WishlistManager.IsInWishlist(lootItem.TemplateId, true,
                                out EWishlistGroup group);
                        DataLoader.questDataStartByItemTemplateID.TryGetValue(lootItem.TemplateId,
                            out QuestPair startQuests);
                        DataLoader.questDataCompleteByItemTemplateID.TryGetValue(lootItem.TemplateId,
                            out QuestPair completeQuests);
                        bool questItem = (lootItem.Item.MarkedAsSpawnedInSession || MoreCheckmarksConfig.showQuestCheckmarksNonFIR) && (lootItem.Item.QuestItem ||
                            (MoreCheckmarksConfig.includeFutureQuests &&
                             (startQuests != null && startQuests.questData.Count > 0) ||
                             (completeQuests != null && completeQuests.questData.Count > 0)));
                        List<List<KeyValuePair<string, int>>> bartersByTrader =
                            MoreCheckmarksMod.GetBarters(lootItem.TemplateId);
                        bool gotBarters = false;
                        if (bartersByTrader != null)
                        {
                            for (int i = 0; i < bartersByTrader.Count; ++i)
                            {
                                if (bartersByTrader[i] != null && bartersByTrader[i].Count > 0)
                                {
                                    gotBarters = true;
                                    break;
                                }
                            }
                        }

                        bool[] currentNeededFor =
                        {
                            questItem, neededStruct.foundNeeded || neededStruct.foundFulfilled, wishlist, gotBarters,
                            craftRequired
                        };
                        // Find needed with highest priority
                        int currentNeeded = -1;
                        int currentHighest = -1;
                        for (int i = 0; i < 5; ++i)
                        {
                            if (currentNeededFor[i] && MoreCheckmarksConfig.priorities[i] > currentHighest)
                            {
                                currentNeeded = i;
                                currentHighest = MoreCheckmarksConfig.priorities[i];
                            }
                        }

                        if (currentNeeded != -1)
                        {
                            // Handle special case of areas
                            if (currentNeeded == 1)
                            {
                                if (neededStruct.foundNeeded) // Need more
                                {
                                    action.Name = "<font=\"BenderBold\"><color=#" +
                                                  ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.needMoreColor) +
                                                  ">Take</color></font>";
                                }
                                else if (neededStruct.foundFulfilled) // We have enough for at least one upgrade
                                {
                                    if (MoreCheckmarksConfig.fulfilledAnyCanBeUpgraded) // We want to know when have enough for at least one upgrade
                                    {
                                        action.Name = "<font=\"BenderBold\"><color=#" +
                                                      ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.fulfilledColor) +
                                                      ">Take</color></font>";
                                    }
                                    else // We only want fulfilled checkmark when ALL requiring this item can be upgraded
                                    {
                                        // Check if we trully do not need more of this item for now
                                        if (neededStruct.possessedCount >= neededStruct.requiredCount)
                                        {
                                            action.Name = "<font=\"BenderBold\"><color=#" +
                                                          ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.fulfilledColor) + ">Take</color></font>";
                                        }
                                        else // Still need more
                                        {
                                            action.Name = "<font=\"BenderBold\"><color=#" +
                                                          ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.needMoreColor) + ">Take</color></font>";
                                        }
                                    }
                                }
                            }
                            else // Not area, just set color
                            {
                                action.Name = "<font=\"BenderBold\"><color=#" +
                                              ColorUtility.ToHtmlStringRGB(MoreCheckmarksConfig.colors[currentNeeded]) +
                                              ">Take</color></font>";
                            }
                        }
                        //else leave it as it is

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError("Failed to process available actions for loose item: " + ex.Message + "\n" +
                                           ex.StackTrace);
            }
        }
    }


    [HarmonyPatch]
    class QuestClassStatusPatch
    {
        private static EQuestStatus preStatus;

        // This prefix will run before a quest's status has been set
        [HarmonyPatch(typeof(QuestClass), "SetStatus")]
        static void Prefix(QuestClass __instance)
        {
            preStatus = __instance.QuestStatus;
        }

        // This postfix will run after a quest's status has been set
        [HarmonyPatch(typeof(QuestClass), "SetStatus")]
        static void Postfix(QuestClass __instance)
        {
            if (__instance == null)
            {
                MoreCheckmarksMod.LogError("Attempted setting queststatus but instance is null");
                return;
            }

            if (__instance.Template == null)
            {
                return;
            }

            MoreCheckmarksMod.LogInfo("Quest " + __instance.Template.Name + " queststatus set to " +
                                      __instance.QuestStatus);

            try
            {
                if (__instance.QuestStatus != preStatus)
                {
                    switch (__instance.QuestStatus)
                    {
                        case EQuestStatus.Started:
                            if (preStatus == EQuestStatus.AvailableForStart)
                            {
                                if (DataLoader.neededStartItemsByQuest.TryGetValue(__instance.Template.Id,
                                        out Dictionary<string, int> startItems))
                                {
                                    foreach (KeyValuePair<string, int> itemEntry in startItems)
                                    {
                                        if (DataLoader.questDataStartByItemTemplateID.TryGetValue(itemEntry.Key,
                                                out QuestPair questList))
                                        {
                                            // Find the key that matches this quest ID (key is locale name, value contains questId)
                                            string keyToRemove = null;
                                            foreach (var kvp in questList.questData)
                                            {
                                                if (kvp.Value.questId == __instance.Template.Id)
                                                {
                                                    keyToRemove = kvp.Key;
                                                    break;
                                                }
                                            }
                                            if (keyToRemove != null)
                                            {
                                                questList.questData.Remove(keyToRemove);
                                                questList.count -= itemEntry.Value;
                                                if (questList.questData.Count == 0)
                                                {
                                                    DataLoader.questDataStartByItemTemplateID.Remove(itemEntry.Key);
                                                }
                                            }
                                        }
                                    }

                                    DataLoader.neededStartItemsByQuest.Remove(__instance.Template.Id);
                                }
                            }

                            // If quest data was incomplete (profile wasn't ready), reload now that a quest has been accepted
                            if (DataLoader.questDataNeedsReload)
                            {
                                MoreCheckmarksMod.LogInfo("Quest accepted - reloading quest data");
                                DataLoader.LoadData();
                            }

                            break;
                        case EQuestStatus.Success:
                        case EQuestStatus.Expired:
                        case EQuestStatus.Fail:
                            if (DataLoader.neededCompleteItemsByQuest.TryGetValue(__instance.Template.Id,
                                    out Dictionary<string, int> completeItems))
                            {
                                foreach (KeyValuePair<string, int> itemEntry in completeItems)
                                {
                                    if (DataLoader.questDataCompleteByItemTemplateID.TryGetValue(itemEntry.Key,
                                            out QuestPair questList))
                                    {
                                        // Find the key that matches this quest ID (key is locale name, value contains questId)
                                        string keyToRemove = null;
                                        foreach (var kvp in questList.questData)
                                        {
                                            if (kvp.Value.questId == __instance.Template.Id)
                                            {
                                                keyToRemove = kvp.Key;
                                                break;
                                            }
                                        }
                                        if (keyToRemove != null)
                                        {
                                            questList.questData.Remove(keyToRemove);
                                            questList.count -= itemEntry.Value;
                                            if (questList.questData.Count == 0)
                                            {
                                                DataLoader.questDataCompleteByItemTemplateID.Remove(itemEntry.Key);
                                            }
                                        }
                                    }
                                }

                                DataLoader.neededCompleteItemsByQuest.Remove(__instance.Template.Id);
                            }

                            // Also add to completed quests cache for immediate filtering
                            DataLoader.completedQuestIds.Add(__instance.Template.Id);

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError("Failed to process change in status for quest " + __instance.Template.Name +
                                           " to " + __instance.QuestStatus + ": " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }


    class ProfileSelectionPatch
    {
        // This postfix will run right after a profile has been selected
        static void Postfix()
        {
            DataLoader.LoadData();
        }
    }
}
