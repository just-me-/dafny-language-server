using System.Collections.Generic;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;
using Files = TestCommons.Paths;

namespace CounterExampleIntegrationTest
{
    [TestFixture]
    public class Various : CounterExampleBase
    {

        [Test]
        public void LoopInvariant()
        {
            GetCounterExamples(Files.ce_li);
            List<string> expecation = new List<string>()
            {
                "L12 C24: b = 0; n = 1178; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void RandomInternetExample()
        {
            GetCounterExamples(Files.ce_lp);
            List<string> expecation = new List<string>()
            {
                "L11 C16: x = 23876; y = 1; "
            };
            VerifyResults(expecation);
        }


    }
}