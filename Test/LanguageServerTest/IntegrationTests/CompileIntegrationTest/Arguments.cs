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
            VerifyLoosely(true, false);
        }

        [Test]
        public void WithArgumentsLegit()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/compile:1", "/nologo" });
            VerifyResults(false, false, "Compiled assembly into compiles_as_dll.dll");
        }

        [Test]
        public void WithArgumentsGarbage()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/bababutz sagt das kind!" });
            VerifyLoosely(true, false, "Error while preprocessing your custom command line arguments.");
        }

        [Test]
        public void WithArgumentsGarbage2()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "bababutz sagt das kind!" });
            VerifyLoosely(true, false, "Error while preprocessing your custom command line arguments.");
        }

        [Test]
        public void WithArgumentsGarbage3()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "``~ÄÄÄ" });
            VerifyLoosely(true, false, "Error while preprocessing your custom command line arguments.");



        }

    }
}