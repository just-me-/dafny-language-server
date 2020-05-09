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
            VerifyResults(false, true, "Compiled assembly into compiles_as_exe.exe");
        }

        [Test]
        public void SuccessWithDllAsResult()
        {
            RunCompilation(Files.cp_fineDLL);
            VerifyResults(false, false, "Compiled assembly into compiles_as_dll.dll");
        }

        [Test]
        public void Included_File()
        {
            RunCompilation(Files.cp_include_main);
            VerifyResults(false, false, "Compiled assembly into basic.dll");
        }

    }
}