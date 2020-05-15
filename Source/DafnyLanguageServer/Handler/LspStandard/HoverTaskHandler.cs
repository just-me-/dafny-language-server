using System;
using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.ProgramServices;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>.
    /// </summary>
    public class HoverTaskHandler : LspBasicHandler<HoverCapability>, IHoverHandler
    {
        public HoverTaskHandler(ILanguageServer router, WorkspaceManager workspaceManager,
            ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
        }


        public Task<Hover> Handle(HoverParams request, CancellationToken cancellationToken)
        {
            var manager = _workspaceManager.SymbolTableManager;
            var line = (int)request.Position.Line + 1;
            var col = (int)request.Position.Character + 1;
            var symbol = manager.GetSymbolByPosition(request.TextDocument.Uri, line, col);

            if (symbol == null)
            {

                return Task.FromResult<Hover>(null);

            }

            MarkedString m1 = new MarkedString("Symbol: " + symbol.ToNiceString());
            MarkedString m2 = new MarkedString("Type: " + symbol.Type);
            MarkedString m3 = new MarkedString("Scope: " + symbol.Parent.Name);
            MarkedString m4 = new MarkedString("Declaration: " + (symbol.IsDeclaration ? "This symbol is a declaration." : symbol.DeclarationOrigin.ToNiceString()));

            Hover result = new Hover()
            {
                Contents = new MarkedStringsOrMarkupContent(m1, m2, m3, m4),
                Range = new Range()
                {
                    Start = new Position(symbol.Line - 1 ?? 0, symbol.Column - 1 ?? 0),
                    End = new Position(symbol.Line - 1 ?? 0, symbol.ColumnEnd - 1 ?? 0)
                }
            };

            return Task.FromResult(result);

        }


        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }
    }
}
