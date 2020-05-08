using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.HandlerServices
{
    public enum CompletionType
    {
        AfterDot,
        AfterNew,
        AllInScope
    }

    public class CompletionService
    {
        public CompletionType Desire { get; private set; }
        public string ExtractedSymbol { get; private set; }
        public IManager Manager { get; }

        public CompletionService(IManager manager)
        {
            Manager = manager;
        }

        public void AnalyzeSupposedDesire(string line, int col)
        {
            ExtractedSymbol = "TMP";
            Desire = CompletionType.AfterNew;
        }

        private CompletionType OLDGetSupposedDesire(int colPos, string line, out string symbolName) // hmm evt doch besser ins file selbst? 
        {
            // diese ganze funktion ist hässlich und buggy. 
            var characters = line.ToCharArray();
            int position = colPos - 2;
            if (position > characters.Length || position < 0)
            {
                throw new ArgumentException("Curser position ist ausserhalb der Zeilenreichweite "); // todo translation 
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
                    ) // hmm ned mit regex weil chars... testen ob das a-zA-Z0-9-_ gleichwertig ist... 
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

        public IEnumerable<ISymbol> GetSymbols(string line, int col)
        {
            AnalyzeSupposedDesire(line, col);
            var wrappingEntrypointSymbol = Manager.GetSymbolWrapperForCurrentScope(line, col);

            switch (Desire)
            {
                case CompletionType.AfterDot:
                    var selectedSymbol = Manager.GetClosestSymbolByName(wrappingEntrypointSymbol, extractedSymbolName);
                    return GetSymbolsProperties(Manager, selectedSymbol);
                case CompletionType.AfterNew:
                    return GetClassSymbolsInScope(Manager, wrappingEntrypointSymbol);
                case CompletionType.AllInScope:
                    return GetAllSymbolsInScope(Manager, wrappingEntrypointSymbol);
                default:
                    throw new ArgumentException("Users desire is not supported yet.");
            }
        }

        private IEnumerable<ISymbol> GetSymbolsProperties(IManager manager, ISymbol selectedSymbol)
        {
            // if selectedSymbol is null... error iwas... not found mässig... todo
            var classSymbol = manager.GetClassOriginFromSymbol(selectedSymbol);
            foreach (var suggestionElement in classSymbol.Children)
            {
                // strip constructor 2do 
                /*
                var ignoredSymbols = new[] { "_ctor", "_default" };
                list?.RemoveAll(x => ignoredSymbols.Any(x.Name.Contains));
                return list;
                */
                yield return suggestionElement;
            }
        }

        private IEnumerable<ISymbol> GetClassSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, new Predicate<ISymbol>(x => x.Kind == Kind.Class)))
            {
                yield return suggestionElement;
            }
        }

        private IEnumerable<ISymbol> GetAllSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol))
            {
                yield return suggestionElement;
            }
        }

    }
}
