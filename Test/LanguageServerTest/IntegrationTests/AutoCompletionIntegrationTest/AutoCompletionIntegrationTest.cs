using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    [TestFixture]
    public class Tests
    {

        public TestSetupManager m = new TestSetupManager("AutoCompletion");
        private CompletionList completions;

        [SetUp]
        public void Setup()
        {
            m.Setup();
            completions = default;
        }

        [TearDown]
        public void TearDown()
        {
            m.TearDown();
        }

        [Test]
        public void RegularStateTest()
        {
            int line = 21;
            int col = 4;
            ArrayList exp = new ArrayList()
            {
                "a",
                "acc2",
                "acc3",
                "b",
                "c",
                "m",
                "Main",
                "C"
            };

            GetCompletions(Files.ac_ac, line, col);
            VerifyCompletions(exp, line, col);
        }


        private void GetCompletions(string file, int line, int column)
        {
            m.Client.TextDocument.DidOpen(file, "dfy");
            completions = m.Client.TextDocument.Completions(
                filePath: file,
                line: line-1,
                column: column-1,
                cancellationToken: m.CancellationSource.Token
            ).Result;
        }

        private void VerifyCompletions(ICollection expectedItems, int expectedStartLine, int expectedStartCol)
        {
            ArrayList receivedEdits = new ArrayList();

            foreach (CompletionItem ci in completions)
            {
                string text = ci.TextEdit.NewText;
                long start_line = ci.TextEdit.Range.Start.Line;
                long start_col  = ci.TextEdit.Range.Start.Character;
                long end_line   = ci.TextEdit.Range.End.Line;
                long end_col    = ci.TextEdit.Range.End.Character;
                receivedEdits.Add(text);

                Assert.AreEqual(start_line, end_line, "AutoComplete Item must be on one line only");
                Assert.AreEqual(text.Length, end_col-start_col, "TextLength does not match the text-edit's insertion range");
                Assert.AreEqual(expectedStartLine-1, start_line, "TextEdit Item does not start at cursor position (Line Mismatch)");
                Assert.AreEqual(expectedStartCol-1, start_col, "TextEdit Item does not start at cursor position (Col Mismatch)");
            }

            CollectionAssert.AreEquivalent(expectedItems, receivedEdits);
        }


    }
}