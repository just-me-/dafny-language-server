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
                throw new ArgumentException("Cursor position is exceeding line width"); // todo resx auslagern
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

        private CompletionType OLDGetSupposedDesire(int colPos, string line, out string symbolName)
        {
            var characters = line.ToCharArray();
            int position = colPos - 2;
            if (position > characters.Length || position < 0)
            {
                throw new ArgumentException("Cursor position is exceeding line width"); // todo resx auslagern
            }
            if (characters[position] == '.')
            {
                position--;
                var symbolString = "";
                while (position >= 0)
                {
                    if (char.IsLetter(characters[position])
                        || char.IsNumber(characters[position])
                        || characters[position] == '_'
                        || characters[position] == '-'
                    )
                    {
                        symbolString += characters[position];
                        position--;
                    }
                    else // das else iwie streichen mit != 
                    {
                        break;
                    }
                }
                char[] symbolCharArray = symbolString.ToCharArray();
                Array.Reverse(symbolCharArray);
                symbolName = new string(symbolCharArray);
                return CompletionType.AfterDot;
            }
            symbolName = "";
            if (characters.Length >= 3 && position >= 3
                                       && characters[position] == ' '
                                       && characters[position - 1] == 'w'
                                       && characters[position - 2] == 'e'
                                       && characters[position - 3] == 'n')
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
                    throw new ArgumentException("Users desire is not supported yet."); // todo lang file
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
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, new Predicate<ISymbol>(x => (x.Kind == Kind.Class) && IsNoDefaultNamespace(x))))
            {
                yield return suggestionElement;
            }
        }

        private IEnumerable<ISymbol> GetAllSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, new Predicate<ISymbol>(IsNoDefaultNamespace)))
            {
                yield return suggestionElement;
            }
        }

        private bool IsNoDefaultNamespace(ISymbol element)
        {
            return (element.Name != SymbolTableStrings.default_class &&
                    element.Name != SymbolTableStrings.default_module &&
                    element.Kind != Kind.Constructor);
        }
    }
}
