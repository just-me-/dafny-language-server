using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Generic;
using System.Linq;
using TestCommons;

namespace AutoCompletionIntegrationTest
{
    public class AutoCompletionBase : IntegrationTestBase
    {
        protected CompletionList completions;

        public AutoCompletionBase() : base("AutoCompletion")
        {
        }

        [SetUp]
        public void CleanResults()
        {
            completions = null;
        }

        protected void GetCompletions(string file, int line, int column)
        {
            Client.TextDocument.DidOpen(file, "dfy");
            completions = Client.TextDocument.Completions(
                filePath: file,
                line: line - 1,
                column: column - 1,
                cancellationToken: CancellationSource.Token
            ).Result;
        }

        protected void VerifyCompletions(List<string> expectedItems)
        {
            List<string> receivedEdits = completions.Select(x => x.InsertText).ToList();
            CollectionAssert.AreEquivalent(expectedItems, receivedEdits);
        }

        protected void VerifyParameters(List<string> expectedItems)
        {
            List<string> receivedDetails = completions.Select(x => x.Detail).ToList();
            Assert.AreEqual(expectedItems.Count, receivedDetails.Count);
            for (int i = 0; i <= expectedItems.Count; i++)
            {
                if (expectedItems[0] != "")
                    Assert.IsTrue(receivedDetails[i].Contains(expectedItems[0]));
            }
        }
    }
}