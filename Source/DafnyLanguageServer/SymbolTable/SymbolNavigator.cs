using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Boogie;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This class contains iterative functions that are used by the
    /// <c>SymbolTableManager</c> as well as by <c>Visitor</c>-Classes.
    /// If you are developing a feature (Handler class) do not use this class directly!
    /// Do use the <c>SymbolTableManager</c>.
    /// </summary>
    public class SymbolNavigator : ISymbolNavigator
    {
        /// <summary>
        /// Searches the tree down. Returns the found symbol or null.
        /// Only takes definitions into account.
        /// If you would like all symbols, not only definitions, use <c>GetSymbolAtPosition</c>.
        /// </summary>
        public ISymbolInformation TopDown(ISymbolInformation rootEntry, Uri file, int line, int character)
        {
            if (rootEntry == null)
            {
                return null;
            }

            ISymbolInformation bestMatch = null;

            if (SymbolUtil.PositionIsWithinSymbolTotalRange(rootEntry, file, line, character))
            {
                bestMatch = rootEntry;
            }

            foreach (var child in rootEntry.Children)
            {
                if (SymbolUtil.PositionIsWithinSymbolTotalRange(child, file, line, character))
                {
                    bestMatch = child;
                    if (child.HasChildren)
                    {
                        var match = TopDown(child, file, line, character);
                        bestMatch = match ?? bestMatch;
                    }
                }
                // in case no better match was found,
                // check default scope too
                if (
                    (bestMatch == null || bestMatch.Equals(rootEntry))
                    && (child.Name == Resources.SymbolTableStrings.default_class || child.Name == Resources.SymbolTableStrings.default_module)
                    && (child.Children?.Any() ?? false)
                )
                {
                    bestMatch = TopDown(child, file, line, character);
                }
            }
            return bestMatch;
        }

        public ISymbolInformation TopDown(ISymbolInformation rootEntry, IToken t)
        {
            return TopDown(rootEntry, new Uri(t.filename), t.line, t.col);
        }


        /// <summary>
        /// Searches the tree downwards to match a specific location.
        /// Only symbols that wrap the location are searched.
        /// <returns> Returns the match oder null.</returns>
        /// This will find all symbols, not only definitions.
        /// If you would like to search for definitions only, use <c>TopDown</c>
        /// </summary>
        public ISymbolInformation GetSymbolAtPosition(ISymbolInformation rootEntry, Uri file, int line, int character)
        {
            if (rootEntry == null || (!SymbolUtil.PositionIsWithinSymbolTotalRange(rootEntry, file, line, character) && (rootEntry.Name != Resources.SymbolTableStrings.default_module)))
            {
                return null;
            }
            var wrappingSymbol = TopDown(rootEntry, file, line, character);
            if (wrappingSymbol?.Descendants != null)
            {
                foreach (var symbol in wrappingSymbol.Descendants)
                {
                    if (SymbolUtil.PositionIsWithinSymbolIdentifier(symbol, file, line, character))
                    {
                        return symbol;
                    }
                }
            }
            if (line == wrappingSymbol?.Line && character <= wrappingSymbol.IdentifierEndColumn)
            {
                return wrappingSymbol;
            }
            return null;
        }

        public ISymbolInformation GetSymbolAtPosition(ISymbolInformation rootEntry, IToken token)
        {
            return GetSymbolAtPosition(rootEntry, new Uri(token.filename), token.line, token.col);
        }

        /// <summary>
        /// Searches all symbols (not just definitions) from top to bottom.
        /// An optional filter for the conditions can be specified.
        /// </summary>
        public List<ISymbolInformation> TopDownAll(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null)
        {
            filter = DefaultPredicateFilter(filter);

            List<ISymbolInformation> symbolList = new List<ISymbolInformation>();
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
        /// Starts a search from the inside out.
        /// Aborts the search when the first symbol fulfilling the filter is found.
        /// An optional filter for the conditions can be specified.
        /// </summary>
        public ISymbolInformation BottomUpFirst(ISymbolInformation entryPoint, Predicate<ISymbolInformation> filter = null)
        {
            filter = DefaultPredicateFilter(filter);
            var symbol = entryPoint;
            do
            {
                var result = GetSingleChild(symbol, filter);
                if (result != null)
                {
                    return result;
                }

                symbol = symbol.Parent;

            } while (symbol != null);

            if (entryPoint.Kind != Kind.RootNode)
            {
                return GetSingleChild(entryPoint.AssociatedDefaultClass, filter);
            }

            return null;
        }

        /// <summary>
        /// Returns one symbol that matches the filter criteria and is child of the argument <c>symbol</c>
        /// </summary>
        private ISymbolInformation GetSingleChild(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol), Resources.ExceptionMessages.symbol_entrypoint_must_be_set);
            }
            filter = DefaultPredicateFilter(filter);

            ISymbolInformation child = symbol.Children?.FirstOrDefault(filter.Invoke);
            if (child != null)
            {
                return child;
            }

            // The following branch checks if the symbol is inherited by a base class
            if (symbol.HasInheritedMembers)
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
            return null;
        }

        /// <summary>
        /// Starts a search from the inside out. Returns all symbols that match a filter.
        /// An optional filter for the conditions can be specified.
        /// </summary>
        public List<ISymbolInformation> BottomUpAll(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null)
        {
            filter = DefaultPredicateFilter(filter);

            List<ISymbolInformation> list = new List<ISymbolInformation>();
            if (symbol == null)
            {
                return list;
            }

            // in case it is the default scope; add functions and methods in the default scope too (and not only classes) 
            if (symbol.Name == Resources.SymbolTableStrings.default_module)
            {
                list.AddRange(GetAllChildren(symbol.AssociatedDefaultClass, filter));
            }

            do
            {
                list.AddRange(GetAllChildren(symbol, filter));
                symbol = symbol.Parent;
            } while (symbol != null);

            return list;
        }

        private IEnumerable<ISymbolInformation> GetAllChildren(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol), Resources.ExceptionMessages.symbol_entrypoint_must_be_set);
            }
            filter = DefaultPredicateFilter(filter);

            var list = symbol.Children?.Where(filter.Invoke).ToList() ?? new List<ISymbolInformation>();

            // The following branch checks if the symbol is inherited by a base class
            if (symbol.HasInheritedMembers)
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
        private Predicate<ISymbolInformation> DefaultPredicateFilter(Predicate<ISymbolInformation> filter)
        {
            return filter ?? (s => true);
        }
    }
}
