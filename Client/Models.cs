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
        // Key: quest name key, Value: (QuestName, QuestId)
        public Dictionary<string, (string questName, string questId)> questData = new Dictionary<string, (string, string)>();
        public int count;
    }
}
