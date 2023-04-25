"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ConfigTypes_1 = require("C:/snapshot/project/obj/models/enums/ConfigTypes");
class Mod {
    preAkiLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const dynamicRouterModService = container.resolve("DynamicRouterModService");
        const staticRouterModService = container.resolve("StaticRouterModService");
        const profileHelper = container.resolve("ProfileHelper");
        const questHelper = container.resolve("QuestHelper");
        const configServer = container.resolve("ConfigServer");
        this.questConfig = configServer.getConfig(ConfigTypes_1.ConfigTypes.QUEST);
        //const questConditionHelper = container.resolve<QuestConditionHelper>("QuestConditionHelper");
        // Hook up a new static route
        staticRouterModService.registerStaticRouter("MoreCheckmarksRoutes", [
            {
                url: "/MoreCheckmarksRoutes/quests",
                action: (url, info, sessionID, output) => {
                    logger.info("MoreCheckmarks making quest data request");
                    const quests = [];
                    const allQuests = questHelper.getQuestsFromDb();
                    //const allQuests = databaseServer.getTables().templates.quests;
                    const profile = profileHelper.getPmcProfile(sessionID);
                    for (const quest of allQuests) {
                        // Skip if not a quest we can have
                        if (this.questIsForOtherSide(profile.Info.Side, quest._id)) {
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
                        if (questStatus >= 3 && questStatus <= 8) {
                            continue;
                        }
                        quests.push(quest);
                    }
                    logger.info("Got quests");
                    return JSON.stringify(quests);
                }
            }
        ], "custom-static-MoreCheckmarksRoutes");
    }
    questIsForOtherSide(playerSide, questId) {
        const isUsec = playerSide.toLowerCase() === "usec";
        if (isUsec && this.questConfig.bearOnlyQuests.includes(questId)) {
            // player is usec and quest is bear only, skip
            return true;
        }
        if (!isUsec && this.questConfig.usecOnlyQuests.includes(questId)) {
            // player is bear and quest is usec only, skip
            return true;
        }
        return false;
    }
}
module.exports = { mod: new Mod() };
