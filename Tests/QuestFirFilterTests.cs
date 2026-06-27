using System.Collections.Generic;
using Xunit;

namespace MoreCheckmarks.Tests
{
    public class QuestFirFilterTests
    {
        private static QuestPair Pair(params (string key, string name, string id, bool fir)[] entries)
        {
            var p = new QuestPair();
            foreach (var (key, name, id, fir) in entries)
                p.questData[key] = (name, id, fir);
            p.count = entries.Length;
            return p;
        }

        [Fact]
        public void Null_HasNoVisible()
        {
            Assert.False(QuestFirFilter.HasVisible(null, true));
            Assert.False(QuestFirFilter.HasVisible(null, false));
            Assert.Empty(QuestFirFilter.Visible(null, true));
        }

        [Fact]
        public void ModeOff_ReturnsAllEntries()
        {
            var p = Pair(("q1", "Q1", "id1", true), ("q2", "Q2", "id2", false));
            var visible = QuestFirFilter.Visible(p, false);
            Assert.Equal(2, visible.Count);
            Assert.True(QuestFirFilter.HasVisible(p, false));
        }

        [Fact]
        public void ModeOn_ReturnsOnlyFirRequiredEntries()
        {
            var p = Pair(("q1", "Q1", "id1", true), ("q2", "Q2", "id2", false));
            var visible = QuestFirFilter.Visible(p, true);
            Assert.Single(visible);
            Assert.Equal("q1", visible[0].Key);
            Assert.True(QuestFirFilter.HasVisible(p, true));
        }

        [Fact]
        public void ModeOn_AllNonFir_HasNoVisible()
        {
            var p = Pair(("q1", "Q1", "id1", false), ("q2", "Q2", "id2", false));
            Assert.Empty(QuestFirFilter.Visible(p, true));
            Assert.False(QuestFirFilter.HasVisible(p, true));
        }
    }
}
