using NUnit.Framework;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace CounterExampleIntegrationTest
{
    [TestFixture]
    public class BasicTests : CounterExampleBase
    {
        [Test]
        public void PostconditionFullfilled()
        {
            GetCounterExamples(Files.ce_ok);
            CollectionAssert.IsEmpty(counterExampleResults.CounterExamples);
        }

        [Test]
        public void ViolatedWith1CounterExamples()
        {
            GetCounterExamples(Files.ce_fail1);
            List<string> expecation = new List<string>()
            {
                "L3 C20: inp1 = -160; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void ViolatedWith2CounterExamples()
        {
            GetCounterExamples(Files.ce_fail2);
            List<string> expecation = new List<string>()
            {
                "L3 C23: inp1 = -24; inp2 = 0; "
            };
            VerifyResults(expecation);
        }
    }
}