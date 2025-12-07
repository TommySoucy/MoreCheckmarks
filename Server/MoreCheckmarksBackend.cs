using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace MoreCheckmarks;

/// <summary>
/// This is the replacement for the former package.json data. This is required for all mods.
///
/// This is where we define all the metadata associated with this mod.
/// You don't have to do anything with it, other than fill it out.
/// All properties must be overriden, properties you don't use may be left null.
/// It is read by the mod loader when this mod is loaded.
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "custom-static-MoreCheckmarksRoutes";
    public override string Name { get; init; } = "MoreCheckmarksBackend";
    public override string Author { get; init; } = "VIP";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");

    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader + 90000)]
public class MoreCheckmarksServer(
    ISptLogger<MoreCheckmarksServer> logger,
    // DynamicRouter dynamicRouter,
    CustomStaticRouter customStaticRouter,
    ProfileHelper profileHelper,
    QuestHelper questHelper,
    ConfigServer configServer,
    // TraderHelper traderHelper,
    DatabaseServer databaseServer,
    FenceService fenceService
    ) : IOnLoad
{
    private readonly QuestConfig questConfig = configServer.GetConfig<QuestConfig>();

    public Task OnLoad()
    {
        customStaticRouter.Set(this);
        customStaticRouter.Set(logger);
        logger.Success("MoreCheckmarks data loaded.");
        return Task.CompletedTask;
    }

    public Quest[] HandleQuests(MongoId sessionId) 
    {
        logger.Info("MoreCheckmarks making quest data request");
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
            logger.Info("MoreCheckmarks: No quest data (new profile). Returning all quests as incomplete.");
            foreach (var quest in allQuests)
            {
                if (!QuestIsForOtherSide(profileSide, quest.Id))
                {
                    quests.Add(quest);
                }
            }
            logger.Info($"Got {quests.Count} quests for MoreCheckmarks (all quests for new profile)");
            return quests.ToArray();
        }

        // Profile has quest data - filter by completion status
        foreach (var quest in allQuests)
        {
            if (QuestIsForOtherSide(profileSide, quest.Id))
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
        logger.Info($"Got {quests.Count} quests for MoreCheckmarks");
        return quests.ToArray();
    }

    public TraderAssort[]? HandleAssorts()
    {
        logger.Info("MoreCheckmarks making trader assort data request");
        TraderAssort[] assorts = [];
        try
        {
            var fenceAssorts = fenceService.GetRawFenceAssorts();
            Type traderType = typeof(Traders);
            var traderFields = traderType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach (var traderField in traderFields)
            {
                var traderValue = traderField.GetValue(null) as MongoId?;
                if (traderValue!.Value == Traders.FENCE && fenceAssorts != null)
                {
                    _ = assorts.Append(fenceAssorts);
                    continue;
                }
                var traderDBEntry = databaseServer.GetTables().Traders[traderValue!.Value];
                if (traderDBEntry != null && traderDBEntry.Assort != null)
                {
                    _ = assorts.Append(traderDBEntry.Assort);
                }
            }
        }
        catch
        { 
            logger.Error("Exception caught when trying to generate assorts.");
            return null;
        }

        logger.Info("Finished fetching assorts for MoreCheckmarks");
        return assorts;
    }

    public Dictionary<MongoId, TemplateItem>? HandleItems()
    {
        logger.Info("MoreCheckmarks making item data request");
        try
        {
            return databaseServer.GetTables().Templates.Items;
        }
        catch
        {
            logger.Error("Could not get tables from database when trying to do the item data request.");
            return null;
        }
    }

    public LocaleBase? HandleLocales()
    {
        logger.Info("MoreCheckmarks making locale request");
        try
        {
            return databaseServer.GetTables().Locales;
        }
        catch
        {
            logger.Error("Could not get tables from database when trying to get locales.");
            return null;
        }
    }

    public HideoutProductionData? HandleProductions()
    {
        logger.Info("MoreCheckmarks making productions request");
        try {
            return databaseServer.GetTables().Hideout.Production;
        }
        catch 
        {
            logger.Error("Could not get tables from database when trying to access hideout and production.");
            return null;
        }
    }

    private bool QuestIsForOtherSide(string playerSide, string questId)
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
