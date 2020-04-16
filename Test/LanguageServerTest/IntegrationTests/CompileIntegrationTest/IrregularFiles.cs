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
            VerifyLoosely(true, false, "Could not locate file");
        }

        [Test]
        public void Empty_File()
        {
            RunCompilation(Files.cp_empty);
            VerifyResults(false, false, "Compiled assembly into empty.dll");
        }

    }
}