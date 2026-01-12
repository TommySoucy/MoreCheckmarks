using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using System;
using System.Collections.Generic;

namespace MoreCheckmarks
{
    public static class DataLoader
    {
        // Quest IDs and Names by items in their requirements
        public static Dictionary<string, QuestPair>
            questDataStartByItemTemplateID = new Dictionary<string, QuestPair>();

        public static Dictionary<string, Dictionary<string, int>> neededStartItemsByQuest =
            new Dictionary<string, Dictionary<string, int>>();

        public static Dictionary<string, QuestPair> questDataCompleteByItemTemplateID =
            new Dictionary<string, QuestPair>();

        public static Dictionary<string, Dictionary<string, int>> neededCompleteItemsByQuest =
            new Dictionary<string, Dictionary<string, int>>();

        // Quest prerequisite tracking
        public static Dictionary<string, HashSet<string>> questPrerequisites = new Dictionary<string, HashSet<string>>();
        public static Dictionary<string, HashSet<string>> prereqCache = new Dictionary<string, HashSet<string>>();
        public static HashSet<string> completedQuestIds = new HashSet<string>();

        // Flag to track if quest data needs to be reloaded (e.g., profile data wasn't available on first load)
        public static bool questDataNeedsReload = false;

        public static JObject itemData;
        public static JObject locales;

        public static Dictionary<string, string> productionEndProductByID = new Dictionary<string, string>();

        // Barter item name and amount of price by items in price
        public static List<Dictionary<string, List<KeyValuePair<string, int>>>> bartersByItemByTrader =
            new List<Dictionary<string, List<KeyValuePair<string, int>>>>();

        public static string[] traders =
        {
            "Prapor", "Therapist", "Fence", "Skier", "Peacekeeper", "Mechanic", "Ragman", "Jaeger", "Lighthouse keeper"
        };

        public static void LoadData()
        {
            MoreCheckmarksMod.LogInfo("Loading data");
            MoreCheckmarksMod.LogInfo("\tQuests");

            // Clear all quest data first - this ensures we start fresh even if loading fails
            questDataStartByItemTemplateID.Clear();
            neededStartItemsByQuest.Clear();
            questDataCompleteByItemTemplateID.Clear();
            neededCompleteItemsByQuest.Clear();
            questPrerequisites.Clear();
            prereqCache.Clear();
            completedQuestIds.Clear();

            JArray questData;
            try
            {
                var questResponse = RequestHandler.GetJson("/MoreCheckmarksRoutes/quests");
                if (string.IsNullOrEmpty(questResponse) || questResponse == "null")
                {
                    MoreCheckmarksMod.LogInfo("Quest data response was empty or null (new profile?). Quest checkmarks will be unavailable until data is loaded.");
                    questData = new JArray();
                }
                else
                {
                    questData = JArray.Parse(questResponse);
                }
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError($"Failed to parse quest data: {ex.Message}. Quest checkmarks will be unavailable.");
                questData = new JArray();
            }

            MoreCheckmarksMod.LogInfo($"Loaded {questData.Count} quests");

            // If quest data is empty, flag for reload on next item view (handles new profile case)
            questDataNeedsReload = questData.Count == 0;
            if (questDataNeedsReload)
            {
                MoreCheckmarksMod.LogInfo("Quest data empty - will reload when a quest is accepted");
            }

            // Process all quests
            foreach (var quest in questData)
            {
                // Process finish conditions
                if (quest["conditions"] != null && quest["conditions"]["AvailableForFinish"] != null)
                {
                    var finishConditions = quest["conditions"]["AvailableForFinish"] as JArray;
                    for (int j = 0; j < finishConditions.Count; ++j)
                    {
                        var conditionType = finishConditions[j]["conditionType"]?.ToString();
                        if (conditionType == null)
                        {
                            MoreCheckmarksMod.LogError($"Quest {quest["_id"]} finish condition {j} missing condition type");
                            continue;
                        }

                        // HandoverItem, LeaveItemAtLocation, PlaceBeacon use the same processing
                        if (conditionType == "HandoverItem" || conditionType == "LeaveItemAtLocation" || conditionType == "PlaceBeacon")
                        {
                            ProcessConditionTargets(quest, finishConditions[j], j, conditionType, "finish",
                                questDataCompleteByItemTemplateID, neededCompleteItemsByQuest);
                        }
                        // FindItem needs to check for duplicate HandoverItem conditions
                        else if (conditionType == "FindItem")
                        {
                            ProcessConditionTargets(quest, finishConditions[j], j, conditionType, "finish",
                                questDataCompleteByItemTemplateID, neededCompleteItemsByQuest, finishConditions);
                        }
                    }
                }
                else
                {
                    MoreCheckmarksMod.LogError($"Quest {quest["_id"]} missing finish conditions");
                }

                // Process start conditions
                if (quest["conditions"] != null && quest["conditions"]["AvailableForStart"] != null)
                {
                    var startConditions = quest["conditions"]["AvailableForStart"] as JArray;
                    for (int j = 0; j < startConditions.Count; ++j)
                    {
                        var conditionType = startConditions[j]["conditionType"]?.ToString();
                        if (conditionType == null)
                        {
                            MoreCheckmarksMod.LogError($"Quest {quest["_id"]} start condition {j} missing condition type");
                            continue;
                        }

                        // HandoverItem, LeaveItemAtLocation, PlaceBeacon use the same processing
                        if (conditionType == "HandoverItem" || conditionType == "LeaveItemAtLocation" || conditionType == "PlaceBeacon")
                        {
                            ProcessConditionTargets(quest, startConditions[j], j, conditionType, "start",
                                questDataStartByItemTemplateID, neededStartItemsByQuest);
                        }
                        // FindItem needs to check for duplicate HandoverItem conditions
                        else if (conditionType == "FindItem")
                        {
                            ProcessConditionTargets(quest, startConditions[j], j, conditionType, "start",
                                questDataStartByItemTemplateID, neededStartItemsByQuest, startConditions);
                        }
                    }
                }
                else
                {
                    MoreCheckmarksMod.LogError($"Quest {quest["_id"]} missing start conditions");
                }
            }

            // Build prerequisite map from quest conditions
            MoreCheckmarksMod.LogInfo("\tBuilding prerequisite map");
            foreach (var quest in questData)
            {
                var questId = quest["_id"]?.ToString();
                if (string.IsNullOrEmpty(questId)) continue;

                var prereqs = new HashSet<string>();
                var startConditions = quest["conditions"]?["AvailableForStart"] as JArray;
                if (startConditions != null)
                {
                    foreach (var condition in startConditions)
                    {
                        if (condition["conditionType"]?.ToString() == "Quest")
                        {
                            var targetQuestId = condition["target"]?.ToString();
                            if (!string.IsNullOrEmpty(targetQuestId))
                            {
                                prereqs.Add(targetQuestId);
                            }
                        }
                    }
                }
                questPrerequisites[questId] = prereqs;
            }
            MoreCheckmarksMod.LogInfo($"\tBuilt prerequisite map for {questPrerequisites.Count} quests");

            MoreCheckmarksMod.LogInfo("\tItems");
            var euro = "569668774bdc2da2298b4568";
            var rouble = "5449016a4bdc2d6f028b456f";
            var dollar = "5696686a4bdc2da3298b456a";
            if (itemData == null)
            {
                itemData = JObject.Parse(RequestHandler.GetJson("/MoreCheckmarksRoutes/items"));
            }

            MoreCheckmarksMod.LogInfo("\tAssorts");
            bartersByItemByTrader.Clear();
            try
            {
                var assortData = JArray.Parse(RequestHandler.GetJson("/MoreCheckmarksRoutes/assorts"));
                for (var i = 0; i < assortData.Count; ++i)
                {
                    bartersByItemByTrader.Add(new Dictionary<string, List<KeyValuePair<string, int>>>());
                    var items = assortData[i]["items"] as JArray;
                    if (items == null) continue;

                    for (var j = 0; j < items.Count; ++j)
                    {
                        if (items[j]["parentId"] != null && items[j]["parentId"].ToString().Equals("hideout"))
                        {
                            var barters = assortData[i]["barter_scheme"]?[items[j]["_id"]?.ToString()] as JArray;
                            if (barters == null) continue;

                            for (var k = 0; k < barters.Count; ++k)
                            {
                                var barter = barters[k] as JArray;
                                if (barter == null) continue;

                                for (var l = 0; l < barter.Count; ++l)
                                {
                                    var priceTPL = barter[l]["_tpl"]?.ToString();
                                    if (priceTPL == null) continue;

                                    if (!priceTPL.Equals(euro) && !priceTPL.Equals(rouble) && !priceTPL.Equals(dollar))
                                    {
                                        if (bartersByItemByTrader[i].TryGetValue(priceTPL,
                                                out var barterList))
                                        {
                                            barterList.Add(new KeyValuePair<string, int>(items[j]["_tpl"].ToString(),
                                                (int)(barter[l]["count"])));
                                        }
                                        else
                                        {
                                            bartersByItemByTrader[i].Add(priceTPL,
                                                new List<KeyValuePair<string, int>>()
                                                {
                                                    new KeyValuePair<string, int>(items[j]["_tpl"].ToString(),
                                                        (int)(barter[l]["count"]))
                                                });
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
                MoreCheckmarksMod.LogError($"Failed to parse assort data: {ex.Message}. Barter checkmarks will be unavailable.");
            }

            MoreCheckmarksMod.LogInfo("\tProductions");
            productionEndProductByID.Clear();
            try
            {
                var productionData = JObject.Parse(RequestHandler.GetJson("/MoreCheckmarksRoutes/productions"));
                var productionRecipes = productionData["recipes"] as JArray;
                if (productionRecipes != null)
                {
                    for (var i = 0; i < productionRecipes.Count; ++i)
                    {
                        var id = productionRecipes[i]["_id"]?.ToString();
                        var endProduct = productionRecipes[i]["endProduct"]?.ToString();
                        if (id != null && endProduct != null)
                        {
                            productionEndProductByID.Add(id, endProduct);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MoreCheckmarksMod.LogError($"Failed to parse production data: {ex.Message}. Craft checkmarks will be unavailable.");
            }
        }

        /// <summary>
        /// Process item targets from a quest condition and update the tracking dictionaries.
        /// </summary>
        private static void ProcessConditionTargets(
            JToken quest,
            JToken condition,
            int conditionIndex,
            string conditionType,
            string conditionPhase,
            Dictionary<string, QuestPair> questDataByItem,
            Dictionary<string, Dictionary<string, int>> neededItemsByQuest,
            JArray allConditions = null)
        {
            if (condition["target"] == null)
            {
                MoreCheckmarksMod.LogError($"Quest {quest["_id"]} {conditionPhase} condition {conditionIndex} of type {conditionType} missing target");
                return;
            }

            var targets = condition["target"] as JArray;
            for (var k = 0; k < targets.Count; ++k)
            {
                var targetId = targets[k].ToString();

                // For FindItem, skip if there's a matching HandoverItem condition
                if (conditionType == "FindItem" && allConditions != null)
                {
                    var foundInHandin = false;
                    for (var l = 0; l < allConditions.Count; ++l)
                    {
                        if (allConditions[l]["conditionType"]?.ToString() == "HandoverItem")
                        {
                            if (allConditions[l]["target"] is JArray handInTargets &&
                                StringJArrayContainsString(handInTargets, targetId) &&
                                (!int.TryParse(allConditions[l]["value"]?.ToString(), out var handoverValue) ||
                                 !int.TryParse(condition["value"]?.ToString(), out var findValue) ||
                                 handoverValue == findValue))
                            {
                                foundInHandin = true;
                                break;
                            }
                        }
                    }
                    if (foundInHandin) continue;
                }

                // Update questDataByItem dictionary
                if (questDataByItem.TryGetValue(targetId, out var quests))
                {
                    if (!quests.questData.ContainsKey(quest["name"].ToString()))
                    {
                        quests.questData.Add(quest["name"].ToString(),
                            (quest["QuestName"].ToString(), quest["_id"].ToString()));
                    }
                    int.TryParse(condition["value"]?.ToString(), out var parsedValue);
                    quests.count += parsedValue;
                }
                else
                {
                    var newPair = new QuestPair();
                    newPair.questData.Add(quest["name"].ToString(),
                        (quest["QuestName"].ToString(), quest["_id"].ToString()));
                    int.TryParse(condition["value"]?.ToString(), out var parsedValue);
                    newPair.count = parsedValue;
                    questDataByItem.Add(targetId, newPair);
                }

                // Update neededItemsByQuest dictionary
                var questId = quest["_id"].ToString();
                if (neededItemsByQuest.TryGetValue(questId, out var items))
                {
                    if (!items.ContainsKey(targetId))
                    {
                        items.Add(targetId, 0);
                    }
                    int.TryParse(condition["value"]?.ToString(), out var parsedValue);
                    items[targetId] += parsedValue;
                }
                else
                {
                    var newDict = new Dictionary<string, int>();
                    int.TryParse(condition["value"]?.ToString(), out var parsedValue);
                    newDict.Add(targetId, parsedValue);
                    neededItemsByQuest.Add(questId, newDict);
                }
            }
        }

        private static bool StringJArrayContainsString(JArray arr, string s)
        {
            for (var i = 0; i < arr.Count; ++i)
            {
                if (arr[i].ToString().Equals(s))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
