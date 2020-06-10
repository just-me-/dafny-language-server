using NUnit.Framework;
using Files = TestCommons.Paths;

namespace GotoIntegrationTest
{
    [TestFixture]
    public class IrregularPositions : GoToBase
    {
        [Test]
        public void RandomSpot()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 18, 1);
            Assert.IsEmpty(goneTo);
        }

        [Test]
        public void UnrealSpot_Line()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 20000, 5);
            Assert.IsEmpty(goneTo);
        }

        [Test]
        public void UnrealSpot_Col()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 1, 50000);
            Assert.IsEmpty(goneTo);
        }

        [Test]
        public void NegativeSpot()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, -5, -5);
            Assert.IsEmpty(goneTo);
        }
    }
}