using DafnyLanguageServer.Resources;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DafnyLanguageServer.Core
{
    public enum CompletionType
    {
        AfterDot,
        AfterNew,
        AllInScope
    }

    public class CompletionProvider : ICompletionProvider
    {
        public string ExtractedSymbol { get; private set; }
        public ISymbolTableManager SymbolTableManager { get; }

        public CompletionProvider(ISymbolTableManager symbolTableManager)
        {
            SymbolTableManager = symbolTableManager;
        }

        public List<CompletionItem> FindCompletionItems(Uri file, int line, int col, string codeLine)
        {
            var desire = GetSupposedDesire(codeLine, col);
            var entryPoint = GetWrappingEntrypointSymbol(file, line, col);

            var completionItems = new List<CompletionItem>();
            foreach (var symbol in GetSymbols(desire, entryPoint))
            {
                completionItems.Add(CreateCompletionItem(symbol, desire));
            }
            return completionItems;
        }

        private CompletionItem CreateCompletionItem(ISymbolInformation symbol, CompletionType desire)
        {
            CompletionItemKind kind = Enum.TryParse(symbol.Kind.ToString(), true, out kind)
                ? kind
                : CompletionItemKind.Reference;

            var insertCode = GetCompletionCodeFragment(symbol, desire);

            var details = "";
            if (symbol.Params != null)
            {
                details += LoggingMessages.completion_params + ": "
                        + string.Join(", ", symbol?.Params.Select(parameter => parameter.Name + ": " + parameter.Type))
                        + "\n ";
            }
#if DEBUG
            details += $"Kind: { symbol.Kind } \n Parent: { symbol.Parent.Name }";
#endif

            return
                new CompletionItem
                {
                    Label = symbol.Name,
                    Kind = kind,
                    InsertText = insertCode,
                    Detail = details
                };
        }

        private string GetCompletionCodeFragment(ISymbolInformation symbol, CompletionType desire)
        {
            switch (desire)
            {
                case CompletionType.AfterNew:
                    return symbol.Name + "()";
                case CompletionType.AfterDot:
                    return (symbol.Kind == Kind.Method || symbol.Kind == Kind.Function)
                        ? (symbol.Name + "()")
                        : symbol.Name;
                default:
                    return symbol.Name;
            }
        }

        /// <summary>
        /// Analyzes the user's intent. Sets the extracted symbol as a side effect as a string (necessary for individual functions like "AfterDot").
        /// Symbol can be requested as property <c>ExtractedSymbol</c>. (eg for testing this component)
        /// </summary>
        public CompletionType GetSupposedDesire(string line, int col)
        {
            col--;
            ExtractedSymbol = "";

            if (line.Length < col || col < 0)
            {
                throw new ArgumentException(Resources.ExceptionMessages.file_cursor_exceed_line_width);
            }

            line = line.Substring(0, col);

            // get word before point
            var match = Regex.Match(line, @"([a-zA-Z_]\w*)\.$");
            if (match.Success)
            {
                ExtractedSymbol = match.Groups[1].Value;
                return CompletionType.AfterDot;
            }
            // matches "new "
            match = Regex.Match(line, @"new\s+$");
            if (match.Success)
            {
                return CompletionType.AfterNew;
            }
            return CompletionType.AllInScope;
        }

        /// <summary>
        /// Returns the wrapping symbol for the given scope.
        /// Can be used as entry point to call <c>GetSymbols</c>.
        /// </summary>
        private ISymbolInformation GetWrappingEntrypointSymbol(Uri file, int line, int col)
        {
            return SymbolTableManager.GetSymbolWrapperForCurrentScope(file, line, col);
        }

        /// <summary>
        /// Call <c>GetSupposedDesire</c> before this function to get the <c>CompletionType</c> parameter.
        /// Call <c>GetWrappingEntrypointSymbol</c> to get the entry point parameter.
        /// </summary>
        private IEnumerable<ISymbolInformation> GetSymbols(CompletionType desire, ISymbolInformation wrappingEntrypointSymbol)
        {
            switch (desire)
            {
                case CompletionType.AfterDot:
                    if (ExtractedSymbol == "this")
                    {
                        ISymbolInformation declarationOfSymbolBeforeDot = SymbolTableManager.GetEnclosingClass(wrappingEntrypointSymbol);
                        return GetSymbolsProperties(declarationOfSymbolBeforeDot);
                    }
                    else
                    {
                        var declarationOfSymbolBeforeDot = SymbolTableManager.GetClosestSymbolByName(wrappingEntrypointSymbol, ExtractedSymbol);
                        var classSymbol = SymbolTableManager.GetClassOriginFromSymbol(declarationOfSymbolBeforeDot);
                        return GetSymbolsProperties(classSymbol);
                    }

                case CompletionType.AfterNew:
                    return GetClassSymbolsInScope(SymbolTableManager, wrappingEntrypointSymbol);

                case CompletionType.AllInScope:
                    return GetAllSymbolsInScope(SymbolTableManager, wrappingEntrypointSymbol);

                default:
                    throw new ArgumentException(Resources.ExceptionMessages.completion_not_yet_supported);
            }
        }

        private IEnumerable<ISymbolInformation> GetSymbolsProperties(ISymbolInformation entryPoint)
        {
            if (entryPoint == null)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.no_symbol_before_fot_found);
            }

            foreach (var suggestionElement in entryPoint.Children)
            {
                if (IsNoDefaultNamespace(suggestionElement))
                {
                    yield return suggestionElement;
                }
            }
        }

        private IEnumerable<ISymbolInformation> GetClassSymbolsInScope(ISymbolTableManager manager, ISymbolInformation wrappingEntrypointSymbol)
        {
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, x => x.Kind == Kind.Class && IsNoDefaultNamespace(x)))
            {
                yield return suggestionElement;
            }
        }

        private IEnumerable<ISymbolInformation> GetAllSymbolsInScope(ISymbolTableManager manager, ISymbolInformation wrappingEntrypointSymbol)
        {
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, IsNoDefaultNamespace))
            {
                yield return suggestionElement;
            }
        }

        private bool IsNoDefaultNamespace(ISymbolInformation element)
        {
            return element.Name != Resources.SymbolTableStrings.default_class &&
                   element.Name != Resources.SymbolTableStrings.default_module &&
                   element.Kind != Kind.Constructor;
        }
    }
}