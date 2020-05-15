﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
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
                    if (child.Children != null && child.Children.Any())
                    {
                        var match = TopDown(child, file, line, character);
                        bestMatch = match ?? bestMatch;
                    }
                }
                // in case no better match was found,
                // check default scope too
                if ((bestMatch == null || bestMatch.Equals(rootEntry)) && (child.Name == "_default" || child.Name == "_module")) //in case merge conflict: das module musste ich hier adden weil ich nun auch (neu dazu gekommen) module durchsuchen muss, und halt auch das defualt module wenne ss onst nix gefunden hat. auf ziele 67 ist noch sowas ähnliches, müsste das da cuh hin?
                {
                    if (child.Children.Any())
                    {
                        bestMatch = TopDown(child, file, line, character);
                    }
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
            if (line == wrappingSymbol?.Line && character <= wrappingSymbol?.ColumnEnd)
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

            ISymbol child = symbol?.Children?.Where(filter.Invoke).FirstOrDefault();
            if (child == null)
            {
                // inherited?
                if (symbol.Kind == Kind.Class && (symbol.BaseClasses?.Any() ?? false))
                {
                    foreach (var baseScope in symbol.BaseClasses)
                    {
                        var baseClassSymbol = baseScope?.Children?.Where(filter.Invoke).FirstOrDefault();
                        if (baseClassSymbol != null)
                        {
                            return baseClassSymbol;
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
            filter = DefaultPredicateFilter(filter);

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
            filter = DefaultPredicateFilter(filter);

            var list = symbol?.Children?.Where(filter.Invoke).ToList();
            // inherited?
            if (symbol.Kind == Kind.Class && (symbol.BaseClasses?.Any() ?? false))
            {
                foreach (var baseScope in symbol.BaseClasses)
                {
                    // todo translation file
                    list.AddRange(baseScope?.Children?.Where(filter.Invoke) ?? throw new InvalidOperationException("Invalid Filter Operation"));
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
