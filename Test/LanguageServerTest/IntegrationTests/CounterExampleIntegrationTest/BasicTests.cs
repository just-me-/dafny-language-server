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
            Assert.AreEqual(0, counterExampleResults.CounterExamples.Count);
        }


        [Test]
        public void ViolatedWith1CounterExamples()
        {
            GetCounterExamples(Files.ce_fail1);
            Assert.AreEqual(1, counterExampleResults.CounterExamples.Count);
        }

        [Test]
        public void ViolatedWith2CounterExamples()
        {
            GetCounterExamples(Files.ce_fail2);
            Assert.AreEqual(2, counterExampleResults.CounterExamples.Count);
        }

    }
}