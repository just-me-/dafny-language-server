using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class IrregularFilesAndLanguages : CompileBase
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

        [Test]
        public void Otherlang_Java()
        {
            RunCompilation(Files.cp_otherlang_java);
            VerifyResults(true, false, failMsg + "Can only compile .dfy files");
        }

        [Test]
        public void Otherlang_Java_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_java_dfyending);
            VerifyResults(true, false, failMsg + "EOF expected in line 1.");
        }

        [Test]
        public void Otherlang_Py()
        {
            RunCompilation(Files.cp_otherlang_py);
            VerifyResults(true, false, failMsg + "Can only compile .dfy files");
        }

        [Test]
        public void Otherlang_Py_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_py_dfyending);
            VerifyResults(true, false, failMsg + "EOF expected in line 1.");
        }


    }
}