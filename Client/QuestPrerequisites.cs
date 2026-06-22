using System.Collections.Generic;

namespace MoreCheckmarks
{
    /// <summary>
    /// Pure (game-independent) quest-prerequisite graph logic, extracted so it can be unit tested.
    /// </summary>
    public static class QuestPrerequisites
    {
        /// <summary>
        /// Returns the set of all transitive prerequisite quest ids for <paramref name="questId"/>.
        /// Does not include <paramref name="questId"/> itself. Terminates on cycles.
        /// </summary>
        public static HashSet<string> ComputeAll(string questId,
            Dictionary<string, HashSet<string>> directPrereqsByQuest)
        {
            var result = new HashSet<string>();
            var queue = new Queue<string>();

            if (directPrereqsByQuest.TryGetValue(questId, out var directPrereqs))
            {
                foreach (var prereq in directPrereqs)
                    queue.Enqueue(prereq);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (result.Contains(current)) continue;

                result.Add(current);

                if (directPrereqsByQuest.TryGetValue(current, out var prereqs))
                {
                    foreach (var prereq in prereqs)
                    {
                        if (!result.Contains(prereq))
                            queue.Enqueue(prereq);
                    }
                }
            }

            return result;
        }
    }
}
