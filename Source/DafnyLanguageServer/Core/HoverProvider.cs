using System;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the related handler and contains the core logic.
    /// </summary>
    public class HoverProvider : IHoverProvider
    {
        private readonly ISymbolTableManager _manager;

        public HoverProvider(ISymbolTableManager manager)
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
            MarkedString m2 = new MarkedString("Kind: " + symbol.Kind);
            MarkedString m3 = new MarkedString("Type: " + type);
            MarkedString m4 = new MarkedString("Scope: " + symbol.Parent.Name);
            MarkedString m5 = new MarkedString("Declaration: " + (symbol.IsDeclaration ? Resources.LoggingMessages.hover_isDeclaration : symbol.DeclarationOrigin.ToNiceString()));

            Hover result = new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(m1, m2, m3, m4, m5),
                Range = new Range
                {
                    Start = new Position(symbol.Line - 1, symbol.Column - 1),
                    End = new Position(symbol.Line - 1, symbol.ColumnEnd - 1)
                }
            };

            return result;
        }
        
    }
}
