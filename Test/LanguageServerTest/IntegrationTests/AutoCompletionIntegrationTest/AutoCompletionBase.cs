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
        private CompletionList completions;

        public AutoCompletionBase()
        {
            name = "AutoCompletion";
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

        protected void VerifyCompletions(ICollection expectedItems, int expectedStartLine, int expectedStartCol)
        {
            ArrayList receivedEdits = new ArrayList();

            foreach (CompletionItem ci in completions)
            {
                string text = ci.TextEdit.NewText;
                long start_line = ci.TextEdit.Range.Start.Line;
                long start_col = ci.TextEdit.Range.Start.Character;
                long end_line = ci.TextEdit.Range.End.Line;
                long end_col = ci.TextEdit.Range.End.Character;
                receivedEdits.Add(text);

                Assert.AreEqual(start_line, end_line, "AutoComplete Item must be on one line only");
                Assert.AreEqual(text.Length, end_col - start_col, "TextLength does not match the text-edit's insertion range");
                Assert.AreEqual(expectedStartLine - 1, start_line, "TextEdit Item does not start at cursor position (Line Mismatch)");
                Assert.AreEqual(expectedStartCol - 1, start_col, "TextEdit Item does not start at cursor position (Col Mismatch)");
            }

            CollectionAssert.AreEquivalent(expectedItems, receivedEdits);
        }
    }
}
