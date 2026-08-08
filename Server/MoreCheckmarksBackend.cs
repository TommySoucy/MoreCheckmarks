using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Quest;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Commerce;
using System.Reflection;
using Path = System.IO.Path;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace MoreCheckmarks;

/// <summary>
/// This is the replacement for the former package.json data. This is required for all mods.
/// </summary>
public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "custom-static-MoreCheckmarksRoutes";
    public string Name { get; init; } = "MoreCheckmarksBackend";
    public string Author { get; init; } = "VIP";
    public List<string>? Contributors { get; init; }
    public Version Version { get; init; } = new("2.4.0");
    public Range SptVersion { get; init; } = new("~4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
}

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 1)]
public class MoreCheckmarksServer(
    ISptLogger<MoreCheckmarksServer> logger,
    CustomStaticRouter customStaticRouter,
    ProfileHelper profileHelper,
    QuestHelper questHelper,
    QuestConfig questConfig,
    TradersTable tradersTable,
    HideoutTable hideoutTable,
    FenceService fenceService,
    ModHelper modHelper
    ) : IOnLoad
{
    private MoreCheckmarksServerConfig modConfig = new();
    private string modFolder = "";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        customStaticRouter.Set(this);
        customStaticRouter.Set(logger);
        modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        modConfig = MoreCheckmarksServerConfig.LoadOrCreate(modFolder, msg => logger.Error(msg));
        WriteQuestReference();
        logger.Success("MoreCheckmarks data loaded.");
        return Task.CompletedTask;
    }

    public Quest[] HandleQuests(MongoId sessionId)
    {
        logger.Debug("MoreCheckmarks making quest data request");
        var quests = new List<Quest>();
        var allQuests = questHelper.GetQuestsFromDb();
        var profile = profileHelper.GetPmcProfile(sessionId);

        if (profile == null)
        {
            logger.Warning("MoreCheckmarks: Profile is null (not loaded yet?). Returning empty quest list.");
            return [];
        }

        var profileInfo = profile.Info;
        if (profileInfo == null)
        {
            logger.Warning("MoreCheckmarks: Profile Info is null. Returning empty quest list.");
            return [];
        }
        var profileSide = profileInfo.Side;
        if (profileSide == null)
        {
            logger.Warning("MoreCheckmarks: Profile Side is null. Returning empty quest list.");
            return [];
        }

        // Check if profile has quest data (new profiles may not have any yet)
        var profileQuests = profile.Quests;
        bool hasQuestData = profileQuests != null && profileQuests.Count > 0;

        if (!hasQuestData)
        {
            // New profile with no quest data - return ALL quests for their side
            // (since they haven't completed any, all quests should show checkmarks)
            logger.Debug("MoreCheckmarks: No quest data (new profile). Returning all quests as incomplete.");
            foreach (var quest in allQuests)
            {
                if (ShouldHideQuest(quest.Id))
                {
                    continue;
                }
                if (!QuestIsForOtherSide(profileSide, quest.Id))
                {
                    quests.Add(quest);
                }
            }
            logger.Debug($"Got {quests.Count} quests for MoreCheckmarks (all quests for new profile)");
            return quests.ToArray();
        }

        // Profile has quest data - filter by completion status
        foreach (var quest in allQuests)
        {
            if (QuestIsForOtherSide(profileSide, quest.Id))
            {
                continue;
            }
            if (ShouldHideQuest(quest.Id))
            {
                continue;
            }
            var questStatus = profile.GetQuestStatus(quest.Id);

            // Include ALL quests the player hasn't completed yet (for "includeFutureQuests" feature)
            // This includes Locked quests (future quests where prerequisites aren't met yet)
            // Only exclude quests that are done: Success, Fail, FailRestartable, MarkedAsFailed, Expired
            if (questStatus != QuestStatusEnum.Success
                && questStatus != QuestStatusEnum.Fail
                && questStatus != QuestStatusEnum.FailRestartable
                && questStatus != QuestStatusEnum.MarkedAsFailed
                && questStatus != QuestStatusEnum.Expired)
            {
                quests.Add(quest);
            }
        }
        logger.Debug($"Got {quests.Count} quests for MoreCheckmarks");
        return quests.ToArray();
    }

    private List<(string name, TraderAssort assort)> GetOrderedTraderAssorts()
    {
        var result = new List<(string name, TraderAssort assort)>();
        var fenceAssorts = fenceService.GetRawFenceAssorts();
        var traderFields = typeof(Traders).GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var traderField in traderFields)
        {
            if (traderField.GetValue(null) is not MongoId traderId)
            {
                continue;
            }

            if (traderId == Traders.FENCE && fenceAssorts != null)
            {
                result.Add(("Fence", fenceAssorts));
                continue;
            }

            var traderDBEntry = tradersTable.GetTrader(traderId);
            if (traderDBEntry != null && traderDBEntry.Assort != null)
            {
                var name = traderDBEntry.Base?.Nickname ?? traderField.Name;
                result.Add((name, traderDBEntry.Assort));
            }
        }
        return result;
    }

    public TraderAssort[]? HandleAssorts()
    {
        logger.Debug("MoreCheckmarks making trader assort data request");
        try
        {
            var ordered = GetOrderedTraderAssorts();
            logger.Debug($"Finished fetching {ordered.Count} assorts for MoreCheckmarks");
            return ordered.Select(x => x.assort).ToArray();
        }
        catch
        {
            logger.Error("Exception caught when trying to generate assorts.");
            return null;
        }
    }

    public string[]? HandleTraderNames()
    {
        logger.Debug("MoreCheckmarks making trader names request");
        try
        {
            return GetOrderedTraderAssorts().Select(x => x.name).ToArray();
        }
        catch
        {
            logger.Error("Exception caught when trying to generate trader names.");
            return null;
        }
    }

    public HideoutProductionData? HandleProductions()
    {
        logger.Debug("MoreCheckmarks making productions request");
        try
        {
            return hideoutTable.Production;
        }
        catch
        {
            logger.Error("Could not get hideout production data.");
            return null;
        }
    }

    private bool ShouldHideQuest(MongoId questId)
    {
        if (modConfig.ExcludedQuestIds != null && modConfig.ExcludedQuestIds.Contains(questId.ToString()))
        {
            return true;
        }
        if (modConfig.HideInactiveEventQuests && !questHelper.ShowEventQuestToPlayer(questId))
        {
            return true;
        }
        return false;
    }

    private void WriteQuestReference()
    {
        if (string.IsNullOrEmpty(modFolder))
        {
            return;
        }
        try
        {
            var traderNames = BuildTraderNameMap();
            var lines = new List<string>
            {
                "# MoreCheckmarks quest reference - auto-generated on server start.",
                "# Format: Quest Name [Trader] = questId",
                "# Paste a questId into excludedQuestIds in config.json to hide it.",
                ""
            };

            foreach (var quest in questHelper.GetQuestsFromDb())
            {
                var name = string.IsNullOrEmpty(quest.QuestName) ? quest.Name : quest.QuestName;
                var trader = traderNames.TryGetValue(quest.TraderId, out var tn) ? tn : quest.TraderId.ToString();
                lines.Add($"{name} [{trader}] = {quest.Id}");
            }

            lines.Sort(StringComparer.OrdinalIgnoreCase);
            File.WriteAllLines(Path.Combine(modFolder, "quest-id-reference.txt"), lines);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to write quest-id-reference.txt: {ex.Message}");
        }
    }

    private Dictionary<MongoId, string> BuildTraderNameMap()
    {
        var map = new Dictionary<MongoId, string>();
        try
        {
            foreach (var kvp in tradersTable)
            {
                var nickname = kvp.Value?.Base?.Nickname;
                if (!string.IsNullOrEmpty(nickname))
                {
                    map[kvp.Key] = nickname!;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to build trader name map: {ex.Message}");
        }
        return map;
    }

    private bool QuestIsForOtherSide(string playerSide, MongoId questId)
    {
        bool isUsec = string.Equals("usec", playerSide, StringComparison.OrdinalIgnoreCase);
        if (isUsec)
        {
            // player is usec, skip if quest is bear only
            return questConfig.BearOnlyQuests.Contains(questId);
        }
        // player is bear, skip if quest is usec only
        return questConfig.UsecOnlyQuests.Contains(questId);
    }
}
