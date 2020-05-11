using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
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

        protected void VerifyCompletions(List<string> expectedItems, int expectedStartLine, int expectedStartCol)
        {
            List<string> receivedEdits = completions.Select(x => x.InsertText).ToList();
            CollectionAssert.AreEquivalent(expectedItems, receivedEdits);
        }
    }
}
