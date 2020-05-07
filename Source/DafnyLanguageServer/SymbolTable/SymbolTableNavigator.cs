using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie.VCExprAST;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This class contains iterative functions that are used by the
    /// <c>SymbolTableManager</c> as well as by <c>Visitor</c>-Classes.
    /// If you are developing a feature (Handler class) do not use this class directly!
    /// Do use the <c>SymbolTableManager</c>.
    /// </summary>
    public class SymbolTableNavigator : INavigator
    {
        /// <summary>
        /// Searches the tree down. Returns the found symbol or null.
        /// Only takes definitions into account.
        /// If you would like all symbols, not only definitions, use <c>GetSymbolByPosition</c>.
        /// </summary>
        public ISymbol TopDown(ISymbol rootEntry, int line, int character)
        {
            if (rootEntry == null)
            {
                return null;
            }
            ISymbol bestMatch = null;
            if (rootEntry.Wraps(line, character))
            {
                bestMatch = rootEntry;
            }
            foreach (var child in rootEntry.Children)
            {
                if (child.Wraps(line, character))
                {
                    bestMatch = child;
                    if (child.Children != null && child.Children.Any())
                    {
                        var match = TopDown(child, line, character);
                        bestMatch = match ?? bestMatch;
                    }
                }
                // in case no better match was found,
                // check default scope too
                if ((bestMatch == null || bestMatch == rootEntry) && child.Name == "_default")
                {
                    if (child.Children.Any())
                    {
                        bestMatch = TopDown(child, line, character);
                    }
                }
            }
            return bestMatch;
        }

        /// <summary>
        /// Searches the tree down. Returns the found symbol or null.
        /// Pay attention to all symbols, not just definitions.
        /// If you would like only definitions, use <c>TopDown</c>
        /// </summary>
        public ISymbol GetSymbolByPosition(ISymbol rootEntry, int line, int character)
        {
            if (rootEntry == null || (!rootEntry.Wraps(line, character) && !rootEntry.Name.Equals("_module")))
            {
                return null;
            }
            var wrappingSymbol = TopDown(rootEntry, line, character);
            if (wrappingSymbol?.Descendants != null)
            {
                foreach (var symbol in wrappingSymbol.Descendants)
                {
                    if (symbol.Wraps(line, character))
                    {
                        return symbol;
                    }
                }
            }
            return wrappingSymbol;
        }


        /// <summary>
        /// Searches all symbols (not just definitions). An optional filter for the conditions can be specified.
        /// </summary>
        public List<ISymbol> TopDownAll(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            filter ??= (s => true);

            List<ISymbol> symbolList = new List<ISymbol>();
            if (symbol != null && filter.Invoke(symbol))
            {
                symbolList.Add(symbol);
            }

            if (symbol?.Descendants == null)
            {
                return symbolList;
            }

            foreach (var child in symbol?.Descendants)
            {
                symbolList.AddRange(TopDownAll(child, filter));
            }
            return symbolList;
        }


        /// <summary>
        /// Starts a search from the inside out. Returns the first symbol found.
        /// An optional filter for the conditions can be specified.
        /// </summary>
        public ISymbol BottomUpFirst(ISymbol entryPoint, Predicate<ISymbol> filter = null)
        {
            filter ??= (s => true);

            var matchingSymbol = GetMatchingChild(entryPoint, filter);
            if (matchingSymbol != null)
            {
                return matchingSymbol;
            }

            var parent = entryPoint;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
                matchingSymbol = GetMatchingChild(parent, filter);
                if (matchingSymbol != null)
                {
                    return matchingSymbol;
                }
            }
            return GetMatchingChild(parent["_default"], filter);
        }
        private ISymbol GetMatchingChild(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            if (symbol == null)
            {
                return null;
            }
            filter ??= (s => true);
            ISymbol child = symbol?.Children?.Where(filter.Invoke).FirstOrDefault();
            if (child == null)
            {
                // habe ich geerbt?
                if (symbol.Kind == Kind.Class && (symbol.BaseClasses?.Any() ?? false))
                {
                    foreach (var baseScope in symbol.BaseClasses)
                    {
                        var baseclassSymbol = baseScope?.Children?.Where(filter.Invoke).FirstOrDefault();
                        if (baseclassSymbol != null)
                        {
                            return baseclassSymbol;
                        }
                    }
                }
            }
            return child;
        }

        /// <summary>
        /// Starts a search from the inside out. Returns all symbols found.
        /// An optional filter for the conditions can be specified.
        /// </summary>
        public List<ISymbol> BottomUpAll(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            filter ??= (s => true);

            List<ISymbol> list = new List<ISymbol>();
            if (symbol == null)
            {
                return list;
            }
            list.AddRange(GetAllChildren(symbol, filter));

            var parent = symbol;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
                list.AddRange(GetAllChildren(parent, filter));
            }
            return list;
        }

        private List<ISymbol> GetAllChildren(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            filter ??= (s => true);

            var list = symbol?.Children?.Where(filter.Invoke).ToList();
            // habe ich geerbt?
            if (symbol.Kind == Kind.Class && (symbol.BaseClasses?.Any() ?? false))
            {
                foreach (var baseScope in symbol.BaseClasses)
                {
                    list.AddRange(baseScope?.Children?.Where(filter.Invoke) ?? throw new InvalidOperationException("Invalid Filter Operation"));
                }
            }
            return list;
        }
    }
}
