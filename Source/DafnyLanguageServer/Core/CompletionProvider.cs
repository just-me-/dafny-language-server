using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DafnyLanguageServer.Resources;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

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
        public IManager Manager { get; }

        public CompletionProvider(IManager manager)
        {
            Manager = manager;
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

        private CompletionItem CreateCompletionItem(ISymbol symbol, CompletionType desire)
        {
            CompletionItemKind kind = Enum.TryParse(symbol.Kind.ToString(), true, out kind)
                ? kind
                : CompletionItemKind.Reference;

            var insertCode = GetCompletionCodeFragment(symbol, desire);

            return
                new CompletionItem
                {
                    Label = symbol.Name,
                    Kind = kind,
                    InsertText = insertCode,
#if DEBUG
                    Detail = $"Kind: { symbol.Kind } \n Parent: { symbol.Parent.Name }"
#endif
                };
        }

        private string GetCompletionCodeFragment(ISymbol symbol, CompletionType desire)
        {
            switch (desire)
            {
                case CompletionType.AfterNew:
                    return symbol.Name + "()";
                case CompletionType.AfterDot:
                    return symbol.Kind == Kind.Method || symbol.Kind == Kind.Function ? symbol.Name + "()" : symbol.Name;
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
        public ISymbol GetWrappingEntrypointSymbol(Uri file, int line, int col)
        {
            return Manager.GetSymbolWrapperForCurrentScope(file, line, col);
        }

        /// <summary>
        /// Call <c>GetSupposedDesire</c> before this function to get the <c>CompletionType</c> parameter.
        /// Call <c>GetWrappingEntrypointSymbol</c> to get the entry point parameter. 
        /// </summary>
        public IEnumerable<ISymbol> GetSymbols(CompletionType desire, ISymbol wrappingEntrypointSymbol)
        {
            switch (desire)
            {
                case CompletionType.AfterDot:
                    var selectedSymbol = Manager.GetClosestSymbolByName(wrappingEntrypointSymbol, ExtractedSymbol);
                    return GetSymbolsProperties(Manager, selectedSymbol);
                case CompletionType.AfterNew:
                    return GetClassSymbolsInScope(Manager, wrappingEntrypointSymbol);
                case CompletionType.AllInScope:
                    return GetAllSymbolsInScope(Manager, wrappingEntrypointSymbol);
                default:
                    throw new ArgumentException(Resources.ExceptionMessages.completion_not_yet_supported);
            }
        }

        private IEnumerable<ISymbol> GetSymbolsProperties(IManager manager, ISymbol selectedSymbol)
        {
            // if selectedSymbol is null... error iwas... not found mässig... todo
            var classSymbol = manager.GetClassOriginFromSymbol(selectedSymbol);
            foreach (var suggestionElement in classSymbol.Children)
            {
                if (IsNoDefaultNamespace(suggestionElement))
                {
                    yield return suggestionElement;
                }
            }
        }

        private IEnumerable<ISymbol> GetClassSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, x => x.Kind == Kind.Class && IsNoDefaultNamespace(x)))
            {
                yield return suggestionElement;
            }
        }

        private IEnumerable<ISymbol> GetAllSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, IsNoDefaultNamespace))
            {
                yield return suggestionElement;
            }
        }

        private bool IsNoDefaultNamespace(ISymbol element)
        {
            return element.Name != SymbolTableStrings.default_class &&
                   element.Name != SymbolTableStrings.default_module &&
                   element.Kind != Kind.Constructor;
        }
    }
}
