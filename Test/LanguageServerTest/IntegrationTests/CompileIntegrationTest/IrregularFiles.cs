using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class IrregularFiles : CompileBase
    {

        [Test]
        public void Inexistant_File()
        {
            RunCompilation(Files.int_inexistant);
            VerifyResults(true, false, failMsg + "Dafny Source File does not exist");
        }

        [Test]
        public void Empty_File()
        {
            RunCompilation(Files.cp_empty);
            VerifyResults(false, false, successMsg);
        }

    }
}