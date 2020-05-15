using CompileIntegrationTest;
using NUnit.Framework;
using Files = TestCommons.Paths;


namespace CodeLensIntegrationTest
{
    [TestFixture]
    public class IrregularFiles : CodeLensBase
    {

        //[Test]
        public void FirstTest()
        {
            RunCodeLens(Files.gt_goto);
            VerifyResults(false, false, "Basic Test");
        }

    }
}