using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Boogie;
using Microsoft.Dafny;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the <c>TextDocumentSyncTaskHandler</c> to provide verification for Dafny files with <c>SendDiagnostics</c>. 
    /// </summary>
    public class HoverProvider : IHoverProvider
    {
        private readonly IManager _manager;

        public HoverProvider(IManager manager)
        {
            _manager = manager;
        }

        public Hover GetHoverInformation(Uri uri, int line, int col)
        {
            var symbol = _manager.GetSymbolByPosition(uri, line, col);

            if (symbol == null)
            {
                return null;
            }

            string type = symbol.Type?.ToString();
            if (string.IsNullOrEmpty(type) || type == "?")
            {
                type = "N/A";
            }

            MarkedString m1 = new MarkedString("Symbol: " + symbol.ToNiceString());
            MarkedString m2 = new MarkedString("Kind: " + symbol.Kind + "; " + "Type: " + type);
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

            return result;
        }
        
    }
}
