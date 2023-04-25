import { DependencyContainer } from "tsyringe";
import type { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod";
import type { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import type {DynamicRouterModService} from "@spt-aki/services/mod/dynamicRouter/DynamicRouterModService";
import type {StaticRouterModService} from "@spt-aki/services/mod/staticRouter/StaticRouterModService";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
//import { QuestConditionHelper } from "@spt-aki/helpers/QuestConditionHelper";
import { QuestHelper } from "@spt-aki/helpers/QuestHelper";
import { IQuestConfig } from "@spt-aki/models/spt/config/IQuestConfig";
import { ConfigTypes } from "@spt-aki/models/enums/ConfigTypes";
import { ConfigServer } from "@spt-aki/servers/ConfigServer";

class Mod implements IPreAkiLoadMod
{
    protected questConfig: IQuestConfig;
	
    public preAkiLoad(container: DependencyContainer): void {
        const logger = container.resolve<ILogger>("WinstonLogger");
        const dynamicRouterModService = container.resolve<DynamicRouterModService>("DynamicRouterModService");
        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");
        const profileHelper = container.resolve<ProfileHelper>("ProfileHelper");
        const questHelper = container.resolve<QuestHelper>("QuestHelper");
		const configServer = container.resolve<ConfigServer>("ConfigServer");
        this.questConfig = configServer.getConfig(ConfigTypes.QUEST);
        //const questConditionHelper = container.resolve<QuestConditionHelper>("QuestConditionHelper");

        // Hook up a new static route
        staticRouterModService.registerStaticRouter(
            "MoreCheckmarksRoutes",
            [
                {
                    url: "/MoreCheckmarksRoutes/quests",
                    action: (url, info, sessionID, output) => 
                    {
                        logger.info("MoreCheckmarks making quest data request");
						const quests: IQuest[] = [];
						const allQuests = questHelper.getQuestsFromDb();
						//const allQuests = databaseServer.getTables().templates.quests;
						const profile: IPmcData = profileHelper.getPmcProfile(sessionID);
						
						for (const quest of allQuests)
						{
							// Skip if not a quest we can have
							if (this.questIsForOtherSide(profile.Info.Side, quest._id))
							{
								continue;
							}
							
							// Skip if already complete or can't complete
							const questStatus = questHelper.getQuestStatus(profile, quest._id);
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
							if (questStatus >= 3 && questStatus <= 8)
							{
								continue;
							}
							
							quests.push(quest);
						}
						
                        logger.info("Got quests");
						return JSON.stringify(quests);
                    }
                }
            ],
            "custom-static-MoreCheckmarksRoutes"
        );
        
    }
	
    protected questIsForOtherSide(playerSide: string, questId: string): boolean
    {
        const isUsec = playerSide.toLowerCase() === "usec";
        if (isUsec && this.questConfig.bearOnlyQuests.includes(questId))
        {
            // player is usec and quest is bear only, skip
            return true;
        }

        if (!isUsec && this.questConfig.usecOnlyQuests.includes(questId))
        {
            // player is bear and quest is usec only, skip
            return true;
        }

        return false;
    }
}
module.exports = {mod: new Mod()}