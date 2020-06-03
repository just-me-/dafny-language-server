using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;

namespace CodeLensIntegrationTest
{
    public class CodeLensBase : IntegrationTestBase
    {

        protected CodeLensContainer codelensResults;

        public CodeLensBase() : base("CodeLens")
        {
        }

        [SetUp]
        public void CleanResults()
        {
            codelensResults = null;
        }

        protected void RunCodeLens(string testfile)
        {
            CodeLensParams codeLensParams = new CodeLensParams
            {
                TextDocument = new TextDocumentIdentifier(new Uri(testfile))
            };

            Client.TextDocument.DidOpen(testfile, "dfy");
            var response = Client.SendRequest<CodeLensContainer>("textDocument/codeLens", codeLensParams, CancellationToken.None);
            codelensResults = response.Result;
        }

        protected void VerifyResults(List<ExpectedCodeLensEntry> expected)
        {
            if (codelensResults == null)
            {
                Assert.Fail("CodeLens results are null - no results received!");
            }
            // check shown references 
            CollectionAssert.AreEquivalent(
                expected.Select(x => x.Name).ToList(),
                codelensResults.Select(x => x.Command.Title).ToList()
                );

            // check linked logic when clicked on references - is code lens popup filled up correctly? 
            if (expected.Count() != codelensResults.Count())
            {
                Assert.Fail("Not same numbers of results as expected.");
            }
            for (int i = 0; i < expected.Count(); i++)
            {
                var expectedPopupArguments = expected[i].References;
                var codeLensPopupArguments = codelensResults.ElementAt(i).Command.Arguments.ToString();
                foreach (var arg in expectedPopupArguments)
                {
                    Regex rgx = new Regex(@"Range\\\"":{\\\""Start\\\"":{\\\""Line\\\"":" + arg.Line + @",\\\""Character\\\"":" + arg.Col + @"},\\\""End");
                    if (!rgx.IsMatch(codeLensPopupArguments))
                    {
                        Assert.Fail($"Missing reference in CodeLens Popup! Reference to line {arg.Line}, Character {arg.Col} \n" +
                                    expected[i].Name + "\n" + codeLensPopupArguments);
                    }
                }
            }
        }
    }
}
