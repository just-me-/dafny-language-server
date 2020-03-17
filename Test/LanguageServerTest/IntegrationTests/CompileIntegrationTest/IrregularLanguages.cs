using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class IrregularLanguages : CompileBase
    {


        [Test]
        public void Java()
        {
            RunCompilation(Files.cp_otherlang_java);
            VerifyResults(true, false, failMsg + "Can only compile .dfy files");
        }

        [Test]
        public void Java_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_java_dfyending);
            VerifyResults(true, false, failMsg + "EOF expected in line 1.");
        }

        [Test]
        public void Py()
        {
            RunCompilation(Files.cp_otherlang_py);
            VerifyResults(true, false, failMsg + "Can only compile .dfy files");
        }

        [Test]
        public void Py_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_py_dfyending);
            VerifyResults(true, false, failMsg + "EOF expected in line 1.");
        }


    }
}