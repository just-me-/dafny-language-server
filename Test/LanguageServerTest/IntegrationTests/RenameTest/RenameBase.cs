using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;

namespace RenameTest
{
    public class RenameBase : IntegrationTestBase
    {

        public RenameBase() : base("Rename")
        {
        }

        protected WorkspaceEdit result;

        [SetUp]
        public void ResetResults()
        {
            result = null;
        }

        public void Run(string testfile, int lineInEditor, int colInEditor, string newText = "newText")
        {
            Client.TextDocument.DidOpen(testfile, "dfy");
            RenameParams p = new RenameParams()
            {
                NewName = newText,
                Position = new Position(lineInEditor-1, colInEditor-1),
                TextDocument = new TextDocumentIdentifier(new Uri(testfile))
            };
            var response = Client.SendRequest<WorkspaceEdit>("textDocument/rename", p, CancellationSource.Token);
            result = response.Result;

        }

        public void VerifyForSingleFile(IList<string> expected)
        {
            IEnumerable<TextEdit> changes = result.Changes.Values.First();
            var actual = changes.ToStringList();
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
