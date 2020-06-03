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
            
            MarkupContent fancyContent = CreateMarkupContent(symbol);

            Hover result = new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(fancyContent),
                Range = new Range
                {
                    Start = new Position(symbol.Line - 1, symbol.Column - 1),
                    End = new Position(symbol.Line - 1, symbol.ColumnEnd - 1)
                }
            };

            return result;
        }

        private static MarkupContent CreateMarkupContent(ISymbol symbol)
        {
            string type = symbol.Type?.ToString();
            if (string.IsNullOrEmpty(type) || type == "?")
            {
                type = "N/A";
            }

            string declaration = symbol.IsDeclaration
                ? Resources.LoggingMessages.hover_isDeclaration
                : symbol.DeclarationOrigin.ToNiceString();

            var fancyContent = new MarkupContent();
            fancyContent.Kind = MarkupKind.Markdown;
            fancyContent.Value =

@$"## Symbol Hover Information
*{symbol.ToNiceString()}*
* **Kind:** {symbol.Kind}
* **Type:** {type}
* **Scope:** `{symbol.Parent.Name}`
* **Declaration:** {declaration}";

            return fancyContent;
        }
    }
}
