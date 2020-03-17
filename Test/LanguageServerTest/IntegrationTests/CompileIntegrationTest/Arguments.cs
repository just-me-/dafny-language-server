using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class Arguments : CompileBase
    {

        [Test]
        public void WithArgumentsNoCompile()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/compile:0" });
            VerifyResults(true, false, failMsg + " in line 0.");
        }

        [Test]
        public void WithArgumentsLegit()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/compile:1", "/nologo" });
            VerifyResults(false, false, successMsg);
        }

        [Test]
        public void WithArgumentsGarbage()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/bababutz sagt das kind!" });
            VerifyResults(true, false, failMsg + " in line 0.");
        }

    }
}