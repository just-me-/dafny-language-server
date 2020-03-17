using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class SuccessStories : CompileBase
    {


        [Test]
        public void SuccessWithExeAsResult()
        {
            RunCompilation(Files.cp_fineEXE);
            VerifyResults(false, true, successMsg);
        }

        [Test]
        public void SuccessWithDllAsResult()
        {
            RunCompilation(Files.cp_fineDLL);
            VerifyResults(false, false, successMsg);
        }

        [Test]
        public void Included_File()
        {
            RunCompilation(Files.ic_basic);
            VerifyResults(false, false, successMsg);
        }

    }
}