using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;

namespace CounterExampleIntegrationTest
{
    public class CounterExampleBase : IntegrationTestBase
    {
        protected CounterExampleResults counterExampleResults;
        protected const string keyword = "counterExample";

        public CounterExampleBase() : base("CounterExample")
        {
        }

        [SetUp]
        public void CleanResults()
        {
            counterExampleResults = null;
        }

        protected void GetCounterExamples(string file)
        {
            var counterExampleParams = new CounterExampleParams
            {
                DafnyFile = file
            };
            Client.TextDocument.DidOpen(file, "dfy");  //notiz für später / dafny translation unit etc: das counter example will, dass das file geöffnet ist! drum mussten wir da auch iwo mal explizit ne eigenen DTU machen - kann das sein??
            counterExampleResults = Client.SendRequest<CounterExampleResults>(keyword, counterExampleParams, CancellationSource.Token).Result;
        }

        //todo auch mit string vergleich machen ticket 217

        //todo Future Envy ticket 115
        protected void CheckResult()
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
