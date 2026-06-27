using System.Collections.Generic;

namespace MoreCheckmarks
{
    public struct NeededStruct
    {
        public bool foundNeeded;
        public bool foundFulfilled;
        public int possessedCount;
        public int requiredCount;
    }

    public class QuestPair
    {
        // Key: quest name key, Value: (QuestName, QuestId, FirRequired)
        public Dictionary<string, (string questName, string questId, bool firRequired)> questData =
            new Dictionary<string, (string, string, bool)>();
        public int count;
    }
}
