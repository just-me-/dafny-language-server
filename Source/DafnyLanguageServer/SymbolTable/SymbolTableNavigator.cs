using System;
using System.Collections.Generic;
using System.Linq;
using DafnyLanguageServer.Resources;
using Microsoft.Boogie;

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
        public ISymbol TopDown(ISymbol rootEntry, Uri file, int line, int character)
        {
            if (rootEntry == null)
            {
                return null;
            }
            ISymbol bestMatch = null;
            if (rootEntry.Wraps(file, line, character))
            {
                bestMatch = rootEntry;
            }
            foreach (var child in rootEntry.Children)
            {
                if (child.Wraps(file, line, character))
                {
                    bestMatch = child;
                    if (child.Children != null && child.Children.Any())             //todo sonar hat noch gewagtt, dass .Children eig shclecht ist, weil da ne liste kopiert wrid... man könnt echt "HasChidlre" machen und gelich aufm hashset das prüfen.
                    {
                        var match = TopDown(child, file, line, character);
                        bestMatch = match ?? bestMatch;
                    }
                }
                // in case no better match was found,
                // check default scope too
                if (
                    (bestMatch == null || bestMatch.Equals(rootEntry))
                    && (child.Name == SymbolTableStrings.default_class || child.Name == SymbolTableStrings.default_module)
                    && (child.Children?.Any() ?? false)
                )
                {
                    bestMatch = TopDown(child, file, line, character);
                }
            }
            return bestMatch;
        }

        public ISymbol TopDown(ISymbol rootEntry, IToken t)
        {
            return TopDown(rootEntry, new Uri(t.filename), t.line, t.col);
        }


        /// <summary>
        /// Searches the tree down. Returns the found symbol or null.
        /// Pay attention to all symbols, not just definitions.
        /// If you would like only definitions, use <c>TopDown</c>
        /// </summary>
        public ISymbol GetSymbolByPosition(ISymbol rootEntry, Uri file, int line, int character)
        {
            if (rootEntry == null || (!rootEntry.Wraps(file, line, character) && (rootEntry.Name != "_module")))
            {
                return null;
            }
            var wrappingSymbol = TopDown(rootEntry, file, line, character);
            if (wrappingSymbol?.Descendants != null)
            {
                foreach (var symbol in wrappingSymbol.Descendants)
                {
                    if (symbol.Wraps(file, line, character))
                    {
                        return symbol;
                    }
                }
            }
            if (line == wrappingSymbol?.Line && character <= wrappingSymbol.ColumnEnd)
            {
                return wrappingSymbol;
            }
            return null;
        }

        public ISymbol GetSymbolByPosition(ISymbol rootEntry, IToken token)
        {
            return GetSymbolByPosition(rootEntry, new Uri(token.filename), token.line, token.col);
        }

        /// <summary>
        /// Searches all symbols (not just definitions). An optional filter for the conditions can be specified.
        /// </summary>
        public List<ISymbol> TopDownAll(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            filter = DefaultPredicateFilter(filter);

            List<ISymbol> symbolList = new List<ISymbol>();
            if (symbol != null && filter.Invoke(symbol))
            {
                symbolList.Add(symbol);
            }

            if (symbol?.Descendants == null)
            {
                return symbolList;
            }

            foreach (var child in symbol.Descendants)
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
            filter = DefaultPredicateFilter(filter);

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

            if (entryPoint.Kind != Kind.RootNode)
            {
                return GetMatchingChild(entryPoint.AssociatedDefaultClass, filter);
            }

            return null;
        }

        private ISymbol GetMatchingChild(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            if (symbol == null)
            {
                return null;
            }
            filter = DefaultPredicateFilter(filter);

            ISymbol child = symbol.Children?.FirstOrDefault(filter.Invoke);
            
            // inherited?
            if (child == null && symbol.Kind == Kind.Class && (symbol.BaseClasses?.Any() ?? false))
            {
                foreach (var baseScope in symbol.BaseClasses)
                {
                    var baseClassSymbol = baseScope?.Children?.FirstOrDefault(filter.Invoke);
                    if (baseClassSymbol != null)
                    {
                        return baseClassSymbol;
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
            filter = DefaultPredicateFilter(filter);

            List<ISymbol> list = new List<ISymbol>();
            if (symbol == null)
            {
                return list;
            }
            list.AddRange(GetAllChildren(symbol, filter));
            // in case it is the default scope; add functions and methods in the default scope too (and not only classes) 
            if (symbol.Parent.Name == SymbolTableStrings.root_node
                && symbol.Name == SymbolTableStrings.default_module
                && symbol.ChildrenHash.ContainsKey(SymbolTableStrings.default_class))
            {
                list.AddRange(GetAllChildren(symbol.ChildrenHash[SymbolTableStrings.default_class], filter));
            }

            var parent = symbol;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
                list.AddRange(GetAllChildren(parent, filter));
            }
            return list;
        }

        private IEnumerable<ISymbol> GetAllChildren(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            filter = DefaultPredicateFilter(filter);

            var list = symbol?.Children?.Where(filter.Invoke).ToList();  //list todo könnte null sein... was tun?
            
            // The following branch checks if the symbol is inherited by a base class
            if (symbol.Kind == Kind.Class && (symbol.BaseClasses?.Any() ?? false))           //symbol todo könnte null sein... was tun?
            {
                foreach (var baseScope in symbol.BaseClasses)
                {
                    list.AddRange(baseScope?.Children?.Where(filter.Invoke) ?? throw new InvalidOperationException(Resources.ExceptionMessages.invalid_filter_operation));   
                }
            }
            return list;
        }

        /// <summary>
        /// Since mono is used for Ci, the ??= Operator is not valid (C#8 feature not supported)
        /// Predicates can not have default predicate as function parameters.
        /// Therefore this function is used to set a default predicate it the given predicate was null. 
        /// </summary>
        private Predicate<ISymbol> DefaultPredicateFilter(Predicate<ISymbol> filter)
        {
            return filter ?? (s => true);
        }
    }
}
