﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This class contains iterative functions that are used by the
    /// <c>SymbolTableManager</c> as well as by <c>Visitor</c>-Classes. 
    /// </summary>
    public class SymbolTableNavigator
    {
        public Dictionary<string, List<SymbolInformation>> SymbolTables { get; set; } = new Dictionary<string, List<SymbolInformation>>();  //still nicht sicher ob das sinnvoll ist.... eig könnt man auch alles in eine einzige tabelle aggregieren mit allen modulen (?)

        public SymbolTableNavigator(Dictionary<string, List<SymbolInformation>> SymbolTables)
        {
            if (SymbolTables == null)
            {
                throw new ArgumentException("Can not be null.. need smyboltables"); // todo ressources 
            }
            this.SymbolTables = SymbolTables;
        }

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

        }

        private void TopDown()
        {

        }

        public SymbolInformation BottomUpFirst(SymbolInformation entryPoint, Predicate<SymbolInformation> filter)
        {
            var matchingSymbol = GetMatchingChild(entryPoint, filter);
            if (matchingSymbol != null)
            {
                return matchingSymbol;
            }

            var parent = entryPoint.Parent;
            while (parent != null)
            {
                matchingSymbol = GetMatchingChild(parent, filter);
                if (matchingSymbol != null)
                {
                    return matchingSymbol;
                }
                parent = parent.Parent;
            }
            // default module hat default class als child. für global scope 

            return null;
        }
        private SymbolInformation GetMatchingChild(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            if (symbol == null || symbol.Children == null)
            {
                return null;
            }
            foreach (var childSymbol in symbol.Children)
            {
                if (filter.Invoke(childSymbol))
                {
                    return childSymbol;
                }
            }
            return null;
        }

        public List<SymbolInformation> BottomUpAll(SymbolInformation symbol, Predicate<SymbolInformation> filter)
        {
            List<SymbolInformation> list = new List<SymbolInformation>();
            list.AddRange(GetAllChildren(symbol, null, filter));

            var parent = symbol.Parent;
            while (parent != null)
            {
                list.AddRange(GetAllChildren(parent, symbol, filter));
                parent = parent.Parent;
            }

            // default module hat default class als child. für global scope 
            //this.SymbolTables["_modul "].["_default"] // jat immer nur ein layer. nur methoden. 
            //==> getDefaultClass oder so als helpter
            return list;
        }
        private List<SymbolInformation> GetAllChildren(SymbolInformation symbol, SymbolInformation excludeSymbol, Predicate<SymbolInformation> filter)
        {
            if (symbol == null || symbol.Children == null)
            {
                return null;
            }
            List<SymbolInformation> list = new List<SymbolInformation>();
            foreach (var childSymbol in symbol.Children)
            {
                if (filter.Invoke(childSymbol)
                    && (excludeSymbol == null || excludeSymbol.Name != childSymbol.Name))
                {
                    list.Add(childSymbol);
                }
            }
            return list;
        }
    }
}