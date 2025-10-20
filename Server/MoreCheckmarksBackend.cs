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
    public override SemanticVersioning.Version Version { get; init; } = new("1.6.0");
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

    public Quest[]? HandleQuests(MongoId sessionId) 
    {
        logger.Info("MoreCheckmarks making quest data request");
        Quest[] quests = [];
        var allQuests = questHelper.GetQuestsFromDb();
        var profile = profileHelper.GetPmcProfile(sessionId);

        if (profile == null)
        {
            logger.Error("Unable to fetch quests for MoreCheckmarks: Profile is null.");
            return null;
        }
        var profileQuests = profile.Quests;
        if (profileQuests == null)
        {
            logger.Error("Unable to fetch quests for MoreCheckmarks: Profile quests are null.");
            return null;
        }
        if (profileQuests.Count == 0)
        {
            logger.Error("Unable to fetch quests for MoreCheckmarks: No quests available.");
            return null;
        }
        var profileInfo = profile.Info;
        if (profileInfo == null)
        {
            logger.Error("Unable to fetch quests for MoreCheckmarks: Profile Info is null.");
            return null;
        }
        var profileSide = profileInfo.Side;
        if (profileSide == null)
        {
            logger.Error("Unable to fetch quests for MoreCheckmarks: Profile Side is null.");
            return null;
        }

        foreach (var quest in allQuests)
        {
            if (QuestIsForOtherSide(profileSide, quest.Id))
            {
                continue;
            }
            var questStatus = profile.GetQuestStatus(quest.Id);
            /*
                Locked = 0,
                AvailableForStart = 1,
                Started = 2,
                AvailableForFinish = 3,
                Success = 4,
                Fail = 5,
                FailRestartable = 6,
                MarkedAsFailed = 7,
                Expired = 8,
                AvailableAfter = 9
            */
            if (questStatus > QuestStatusEnum.AvailableForStart
                && questStatus < QuestStatusEnum.AvailableAfter)
            {
                continue;
            }
            _ = quests.Append(quest);
        }
        logger.Info("Got quests");
        return quests;
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
