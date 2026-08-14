using System.Collections.Generic;

namespace MoreCheckmarks
{
    /// <summary>
    /// Pure, game-independent helpers for summing item stacks and formatting the
    /// stash / on-you tooltip count lines. No EFT/Unity dependencies so this can be
    /// unit-tested in the Tests project.
    /// </summary>
    public static class InventoryCounts
    {
        /// <summary>
        /// Sums stack counts over a sequence of (stackCount, isFoundInRaid) pairs.
        /// Returns (fir, total) where fir counts only found-in-raid stacks.
        /// </summary>
        public static (int fir, int total) SumStacks(IEnumerable<(int stack, bool fir)> items)
        {
            int fir = 0;
            int total = 0;
            foreach (var (stack, isFir) in items)
            {
                total += stack;
                if (isFir)
                {
                    fir += stack;
                }
            }
            return (fir, total);
        }

        /// <summary>
        /// Builds the "STASH: x found in raid / y total" line, plus an "ON YOU" line
        /// rendered only when equipmentTotal > 0.
        /// </summary>
        public static string BuildCountLines(ItemCounts counts, string stashLabel, string onYouLabel)
        {
            string result = Localization.Format("tooltip.counts.line",
                ("label", stashLabel), ("fir", counts.stashFir), ("total", counts.stashTotal));

            if (counts.equipmentTotal > 0)
            {
                result += "\n" + Localization.Format("tooltip.counts.line",
                    ("label", onYouLabel), ("fir", counts.equipmentFir), ("total", counts.equipmentTotal));
            }

            return result;
        }
    }
}
