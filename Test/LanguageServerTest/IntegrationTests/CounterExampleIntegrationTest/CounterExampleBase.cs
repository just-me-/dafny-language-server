using DafnyLanguageServer.CustomDTOs;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommons;

namespace CounterExampleIntegrationTest
{
    public class CounterExampleBase : IntegrationTestBase
    {
        protected CounterExampleResults counterExampleResults;
        protected List<string> resultsAsString => counterExampleResults.CounterExamples.ToStringList();

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
            Client.TextDocument.DidOpen(file, "dfy");
            counterExampleResults = Client.SendRequest<CounterExampleResults>(keyword, counterExampleParams, CancellationSource.Token).Result;
        }

        protected void VerifyResults(List<string> expectation)
        {
            if (counterExampleResults == null)
            {
                Assert.Fail("No Counter Example Message provided by server");
            }
            CollectionAssert.AreEquivalent(expectation, resultsAsString);
        }
    }
}