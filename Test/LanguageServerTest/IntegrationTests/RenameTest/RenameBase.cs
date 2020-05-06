using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Run(string testfile)
        {
            int lineInEditor = 9;
            int colInEditor = 16;


            Client.TextDocument.DidOpen(testfile, "dfy");
            RenameParams p = new RenameParams()
            {
                NewName = "nn",
                Position = new Position(lineInEditor-1, colInEditor-1),
                TextDocument = new TextDocumentIdentifier(new Uri(testfile))
            };
            var response = Client.SendRequest<WorkspaceEdit>("textDocument/rename", p, CancellationSource.Token);
            result = response.Result;

        }

        public void Verify()
        {

        }
    }
}
