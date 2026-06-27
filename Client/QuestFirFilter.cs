using System.Collections.Generic;

namespace MoreCheckmarks
{
    /// <summary>
    /// Pure (game-independent) logic deciding which quest entries are visible given the
    /// "only show FiR-required quests" mode. Extracted so it can be unit tested.
    /// </summary>
    public static class QuestFirFilter
    {
        /// <summary>
        /// Returns the quest entries that should be displayed/counted.
        /// When <paramref name="onlyFirRequired"/> is false, all entries are returned.
        /// When true, only entries whose quest requires Found-in-Raid are returned.
        /// </summary>
        public static List<KeyValuePair<string, (string questName, string questId, bool firRequired)>> Visible(
            QuestPair quests, bool onlyFirRequired)
        {
            var result = new List<KeyValuePair<string, (string questName, string questId, bool firRequired)>>();
            if (quests == null)
            {
                return result;
            }

            foreach (var kvp in quests.questData)
            {
                if (!onlyFirRequired || kvp.Value.firRequired)
                {
                    result.Add(kvp);
                }
            }

            return result;
        }

        /// <summary>
        /// Whether at least one quest is visible (drives whether a checkmark shows at all).
        /// </summary>
        public static bool HasVisible(QuestPair quests, bool onlyFirRequired)
        {
            if (quests == null)
            {
                return false;
            }

            foreach (var kvp in quests.questData)
            {
                if (!onlyFirRequired || kvp.Value.firRequired)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
