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
            VerifyResults(true, false, failPrefix + "Can only compile .dfy files.");
        }

        [Test]
        public void Java_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_java_dfyending);
            VerifyResults(true, false, failPrefix + "[L1:C1] Syntax Error: \n EOF expected");
        }

        [Test]
        public void Py()
        {
            RunCompilation(Files.cp_otherlang_py);
            VerifyResults(true, false, failPrefix + "Can only compile .dfy files.");
        }

        [Test]
        public void Py_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_py_dfyending);
            VerifyResults(true, false, failPrefix + "[L1:C1] Syntax Error: \n EOF expected");
        }


    }
}