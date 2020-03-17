using System;
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
            Console.WriteLine(compilerResults.Message);
            VerifyLoosely(true, false);
            Console.WriteLine(compilerResults.Message);
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
            Console.WriteLine(compilerResults.Message);
            VerifyLoosely(true, false);
        }

        [Test]
        public void WithArgumentsGarbage2()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "bababutz sagt das kind!" });
            VerifyLoosely(true, false);

        }

        [Test]
        public void WithArgumentsGarbage3()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "``~ÄÄÄ" });
            Console.WriteLine(compilerResults.Message);

            VerifyLoosely(true, false);


        }

    }
}