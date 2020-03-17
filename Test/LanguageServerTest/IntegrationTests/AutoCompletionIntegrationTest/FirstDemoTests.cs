using System.Collections;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    [TestFixture]
    public class FirstDemoTests : AutoCompletionBase
    {

        [Test]
        public void RegularStateTest()
        {
            int line = 21;
            int col = 4;
            ArrayList exp = new ArrayList()
            {
                "a",
                "acc2",
                "acc3",
                "b",
                "c",
                "m",
                "Main",
                "C"
            };

            GetCompletions(Files.ac_ac, line, col);
            VerifyCompletions(exp, line, col);
        }


    }
}