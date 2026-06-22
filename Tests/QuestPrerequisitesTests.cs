using System.Collections.Generic;
using Xunit;

namespace MoreCheckmarks.Tests
{
    public class QuestPrerequisitesTests
    {
        private static Dictionary<string, HashSet<string>> Map(
            params (string quest, string[] prereqs)[] entries)
        {
            var d = new Dictionary<string, HashSet<string>>();
            foreach (var (quest, prereqs) in entries)
                d[quest] = new HashSet<string>(prereqs);
            return d;
        }

        [Fact]
        public void UnknownQuest_ReturnsEmpty()
        {
            var result = QuestPrerequisites.ComputeAll("X", Map());
            Assert.Empty(result);
        }

        [Fact]
        public void DirectPrereqs_AreReturned()
        {
            var map = Map(("C", new[] { "A", "B" }));
            var result = QuestPrerequisites.ComputeAll("C", map);
            Assert.Equal(new HashSet<string> { "A", "B" }, result);
        }

        [Fact]
        public void TransitiveChain_IsFullyResolved()
        {
            // D -> C -> B -> A
            var map = Map(
                ("D", new[] { "C" }),
                ("C", new[] { "B" }),
                ("B", new[] { "A" }));
            var result = QuestPrerequisites.ComputeAll("D", map);
            Assert.Equal(new HashSet<string> { "C", "B", "A" }, result);
        }

        [Fact]
        public void DiamondGraph_DeduplicatesSharedAncestor()
        {
            // D -> B, C ; B -> A ; C -> A
            var map = Map(
                ("D", new[] { "B", "C" }),
                ("B", new[] { "A" }),
                ("C", new[] { "A" }));
            var result = QuestPrerequisites.ComputeAll("D", map);
            Assert.Equal(new HashSet<string> { "B", "C", "A" }, result);
        }

        [Fact]
        public void Cycle_Terminates()
        {
            // A -> B -> A (cycle)
            var map = Map(
                ("A", new[] { "B" }),
                ("B", new[] { "A" }));
            var result = QuestPrerequisites.ComputeAll("A", map);
            Assert.Equal(new HashSet<string> { "B", "A" }, result);
        }

        [Fact]
        public void Result_DoesNotIncludeQuestItself_WhenNotSelfReferential()
        {
            var map = Map(("C", new[] { "A" }));
            var result = QuestPrerequisites.ComputeAll("C", map);
            Assert.DoesNotContain("C", result);
        }
    }
}
