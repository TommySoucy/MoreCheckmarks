using Xunit;

namespace MoreCheckmarks.Tests
{
    public class InventoryCountsTests
    {
        [Fact]
        public void SumStacks_Empty_ReturnsZero()
        {
            var (fir, total) = InventoryCounts.SumStacks(new (int, bool)[0]);
            Assert.Equal(0, fir);
            Assert.Equal(0, total);
        }

        [Fact]
        public void SumStacks_SumsStacksAndPartitionsFir()
        {
            var (fir, total) = InventoryCounts.SumStacks(new[]
            {
                (2, true),
                (3, false),
                (1, true),
            });
            Assert.Equal(3, fir);    // 2 + 1
            Assert.Equal(6, total);  // 2 + 3 + 1
        }

        [Fact]
        public void BuildCountLines_NoEquipment_OmitsOnYouLine()
        {
            var counts = new ItemCounts { stashFir = 1, stashTotal = 2, equipmentFir = 0, equipmentTotal = 0 };
            var result = InventoryCounts.BuildCountLines(counts, "STASH", "ON YOU");
            Assert.Equal("STASH: <color=#dd831a>1</color> found in raid / 2 total", result);
            Assert.DoesNotContain("ON YOU", result);
            Assert.DoesNotContain("\n", result);
        }

        [Fact]
        public void BuildCountLines_WithEquipment_AppendsOnYouLine()
        {
            var counts = new ItemCounts { stashFir = 2, stashTotal = 2, equipmentFir = 1, equipmentTotal = 1 };
            var result = InventoryCounts.BuildCountLines(counts, "STASH", "ON YOU");
            Assert.Equal(
                "STASH: <color=#dd831a>2</color> found in raid / 2 total\n" +
                "ON YOU: <color=#dd831a>1</color> found in raid / 1 total",
                result);
        }

        [Fact]
        public void BuildCountLines_EquipmentTotalNonZeroButFirZero_StillShowsOnYou()
        {
            var counts = new ItemCounts { stashFir = 0, stashTotal = 0, equipmentFir = 0, equipmentTotal = 3 };
            var result = InventoryCounts.BuildCountLines(counts, "STASH", "ON YOU");
            Assert.Contains("ON YOU: <color=#dd831a>0</color> found in raid / 3 total", result);
        }
    }
}
