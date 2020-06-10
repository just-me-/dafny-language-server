using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

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

            MarkupContent formattedContent = CreateMarkupContent(symbol);

            Hover result = new Hover
            {
                Contents = new MarkedStringsOrMarkupContent(formattedContent),
                Range = new Range
                {
                    Start = new Position(symbol.Line - 1, symbol.Column - 1),
                    End = new Position(symbol.Line - 1, symbol.IdentifierEndColumn - 1)
                }
            };

            return result;
        }

        private static MarkupContent CreateMarkupContent(ISymbolInformation symbol)
        {
            string type = symbol.Type?.ToString();
            if (string.IsNullOrEmpty(type) || type == "?")
            {
                type = Resources.LoggingMessages.hover_unknown;
            }

            string declaration = symbol.IsDeclaration
                ? Resources.LoggingMessages.hover_isDeclaration
                : symbol.DeclarationOrigin.PositionToFormattedString();

            var formattedContent = new MarkupContent();
            formattedContent.Kind = MarkupKind.Markdown;
            formattedContent.Value =
                $" **`{symbol.Name}`** *({symbol.PositionToFormattedString()})*\n" +
                $"* **{Resources.LoggingMessages.hover_type}:** {type}\n" +
                $"* **{Resources.LoggingMessages.hover_kind}:** {symbol.Kind}\n" +
                $"* **{Resources.LoggingMessages.hover_scope}:** `{symbol.Parent.Name}`\n" +
                $"* **{Resources.LoggingMessages.hover_declaration}:** {declaration}";
            return formattedContent;
        }
    }
}