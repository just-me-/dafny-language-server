using System.Collections.Generic;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;
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
                "L4 C19: inp1=((- 160)); more=((- 320)); "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void ViolatedWith2CounterExamples()
        {
            GetCounterExamples(Files.ce_fail2);
            List<string> expecation = new List<string>()
            {
                "L4 C22: inp1=((- 24)); inp2=0; less=(**less#0); more=((- 24))'1; ", 
                "L5 C22: inp1=((- 24)); inp2=0; less=((- 24))'2; more=((- 24))'1; "
            };
            VerifyResults(expecation);
        }

    }
}