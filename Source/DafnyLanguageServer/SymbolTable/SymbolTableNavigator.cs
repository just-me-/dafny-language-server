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
    /// </summary>
    public class SymbolTableNavigator
    {
        private void test(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            // break expression? 
            // yeald? 
            // todo mergen213 ClassMergen 
            //filter.Invoke(symbol.Children.ToArray());
            symbol.Children.Where(filter.Invoke);
        }

        public void GetList()
        {

        }

        public void GetFirstMatch()
        {
            SymbolInformation closestWrappingSymbol = null;


        }

        public SymbolInformation TopDown(SymbolInformation rootEntry, int line, int character)
        {
            SymbolInformation bestMatch = null;
            foreach (var child in rootEntry.Children)
            {
                if (child.Wraps(line, character))
                {
                    bestMatch = child;
                    if (child.Children.Any())
                    {
                        var match = TopDown(child, line, character);
                        bestMatch = match ?? bestMatch;
                    }
                }

                // sondercase für die default class imd default scope
                // wenn bisher nix gefunden, dann ist "wraps" immer true
                // für den default scope 
                if (bestMatch == null && child.Name == "_default")
                {
                    if (child.Children.Any())
                    {
                        var match = TopDown(child, line, character);
                        bestMatch = match ?? bestMatch;
                    }
                }
            }
            return bestMatch;
        }

        public SymbolInformation GetSymbolByPosition(SymbolInformation rootEntry, int line, int character)
        {
            var wrappingSymbol = TopDown(rootEntry, line, character);
            foreach (var symbol in wrappingSymbol.Descendants)
            {
                if (symbol.Wraps(line, character))
                {
                    return symbol;
                }
            }
            return null;
        }

        public List<SymbolInformation> TopDownAll(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            filter ??= (s => true);

            var symbolList = new List<SymbolInformation>();
            if (filter.Invoke(symbol))
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

        public SymbolInformation BottomUpFirst(SymbolInformation entryPoint, Predicate<SymbolInformation> filter)
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
        private SymbolInformation GetMatchingChild(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            var child = symbol?.Children?.Where(filter.Invoke).FirstOrDefault();
            if (child == null)
            {
                // habe ich geerbt? 
                if (symbol.Kind == Kind.Class && (symbol.BaseClases?.Any() ?? false))
                {
                    foreach (var baseScope in symbol.BaseClases)
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

        public List<SymbolInformation> BottomUpAll(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            filter ??= (s => true);

            List<SymbolInformation> list = new List<SymbolInformation>();
            list.AddRange(GetAllChildren(symbol, filter));

            var parent = symbol;
            while (parent != null)
            {
                parent = parent.Parent;
                list.AddRange(GetAllChildren(parent, filter));
            }
            list.AddRange(GetAllChildren(parent, filter));
            return list;
        }

        private List<SymbolInformation> GetAllChildren(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            var list = symbol?.Children?.Where(filter.Invoke).ToList();
            // habe ich geerbt? 
            if (symbol.Kind == Kind.Class && (symbol.BaseClases?.Any() ?? false))
            {
                foreach (var baseScope in symbol.BaseClases)
                {
                    list.AddRange(baseScope?.Children?.Where(filter.Invoke) ?? throw new InvalidOperationException("lolololol"));
                }
            }
            return list;
        }
    }
}
