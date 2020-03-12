using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Serilog;
using Serilog.Extensions.Logging;
using TestCommons;
using Files = TestCommons.Paths;

namespace CounterExampleIntegrationTest
{
    [TestFixture]
    public class Tests
    {
        public TestSetupManager m = new TestSetupManager("GoTo");
        private CounterExampleResults counterExampleResults;
        private const string keyword = "counterExample";

        [SetUp]
        public void Setup()
        {
            m.Setup();
            counterExampleResults = default;
        }

        [TearDown]
        public void TearDown()
        {
            m.TearDown();
        }


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


        private void GetCounterExamples(string file)
        {
            var counterExampleParams = new CounterExampleParams
            {
                DafnyFile = file
            };
            m.Client.TextDocument.DidOpen(file, "dfy");  //notiz für später / dafny translation unit etc: das counter example will, dass das file geöffnet ist! drum mussten wir da auch iwo mal explizit ne eigenen DTU machen - kann das sein??
            counterExampleResults = m.Client.SendRequest<CounterExampleResults>(keyword, counterExampleParams, m.CancellationSource.Token).Result;
        }


        //todo Future Envy ticket 115
        private void CheckResult()
        {
            if (counterExampleResults == null)
            {
                Assert.Fail("No Counter Example Message provided by server");
            }


            foreach (CounterExampleResult ce in counterExampleResults.CounterExamples)
            {
                var line = ce.Line;
                var col = ce.Col;
                foreach (KeyValuePair<string, string> kvp in ce.Variables)
                {
                    var key = kvp.Key;
                    var val = kvp.Value;
                    //In case needed for later with detailled check
                }
            }
        }

        
    }
}