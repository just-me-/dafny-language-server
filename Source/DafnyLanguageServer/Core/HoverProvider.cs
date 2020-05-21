using System;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the <c>TextDocumentSyncTaskHandler</c> to provide verification for Dafny files with <c>SendDiagnostics</c>. 
    /// </summary>
    public class HoverProvider : IHoverProvider
    {
        private readonly ISymbolTree _symbolTree;

        public HoverProvider(ISymbolTree symbolTree)
        {
            _symbolTree = symbolTree;
        }

        public Hover GetHoverInformation(Uri uri, int line, int col)
        {
            var symbol = _symbolTree.GetSymbolByPosition(uri, line, col);

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
            MarkedString m2 = new MarkedString("Kind: " + symbol.Kind);
            MarkedString m3 = new MarkedString("Type: " + type);
            MarkedString m4 = new MarkedString("Scope: " + symbol.Parent.Name);
            MarkedString m5 = new MarkedString("Declaration: " + (symbol.IsDeclaration ? Resources.LoggingMessages.hover_isDeclaration : symbol.DeclarationOrigin.ToNiceString()));

            Hover result = new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(m1, m2, m3, m4, m5),
                Range = new Range
                {
                    Start = new Position(symbol.Line - 1 ?? 0, symbol.Column - 1 ?? 0),
                    End = new Position(symbol.Line - 1 ?? 0, symbol.ColumnEnd - 1 ?? 0)
                }
            };

            return result;
        }
        
    }
}
