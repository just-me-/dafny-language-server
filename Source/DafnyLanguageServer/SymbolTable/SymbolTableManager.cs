using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// Provides all needed SymbolTableInformation for all Modules. Needs a Dafny Program to work. 
    /// <c>SymbolTables</c> is a key-value-hash. Key is the module name (string) and value is a set of <c>SymbolInformation.</c>
    /// A SymbolTable (List of <c>SymbolInformation</c> for each module) is sorted.
    /// Mostly there is only one module - the default module (and class) for a single Dafny file. 
    /// </summary>
    public class SymbolTableManager : IManager
    {
        private readonly Microsoft.Dafny.Program _dafnyProgram;
        public Dictionary<string, ISymbol> SymbolTables { get; set; } = new Dictionary<string, ISymbol>();

        public SymbolTableManager(Microsoft.Dafny.Program dafnyProgram)
        {
            _dafnyProgram = dafnyProgram;
            GenerateSymbolTable();
        }

        private void GenerateSymbolTable()
        {
            foreach (var module in _dafnyProgram.Modules())
            {
                var declarationVisitor = new LanguageServerDeclarationVisitor();
                module.Accept(declarationVisitor);
                var declarationTable = declarationVisitor.SymbolTable;

                var deepVisitor = new SymbolTableVisitorEverythingButDeclarations();
                deepVisitor.SymbolTable = declarationTable;

                module.Accept(deepVisitor);

                if (deepVisitor.SymbolTable[0].Kind != Kind.Module)
                {
                    throw new Exception("Mimimi komisch mimimumu");
                }
                SymbolTables.Add(module.Name, deepVisitor.SymbolTable[0]); // todo immer erstes modul #201

                string debugMe = CreateDebugReadOut();
            }
        }

        public string CreateDebugReadOut()
        {
            StringBuilder b = new StringBuilder();
            foreach (var kvp in SymbolTables)
            {
                b.AppendLine("Module: " + kvp.Key);
                foreach (var symbol in kvp.Value.Children)
                {
                    b.AppendLine(symbol.ToString());
                }
            }
            return b.ToString();
        }

        public List<string> GetEntriesAsStringList()
        {
            return (from kvp in SymbolTables from symbol in kvp.Value.Children select symbol.ToString()).ToList();
        }


        // ab hier das meiste auslagern(?)


        // weg 
        public ISymbol GetSymbolByPosition(int line, int character)
        {
            INavigator navigator = new SymbolTableNavigator();
            ISymbol symbol = null;
            foreach (var modul in SymbolTables)
            {
                symbol ??= navigator.GetSymbolByPosition(modul.Value, line, character);
            }
            return symbol;
        }

        private ISymbol GetClassSymbolByPath(string classPath)
        {
            // todo better do a split? is there rly every time "just one module and one class"? #212
            string[] originPath = classPath.Split('.'); // 0 = module, 1 = class
            if (originPath.Length != 2)
            {
                throw new ArgumentException("Invalid class path... expected Module.Class pattern."); //tmp
            }
            return SymbolTables[originPath[0]][originPath[1]];
        }

        private bool PositionIsInSymbolsRange(int line, int character, ISymbol symbol)
        {
            return (symbol.Line <= line
                    && symbol.Line >= line
                    && symbol.ColumnStart <= character
                    && symbol.ColumnEnd >= character);
        }

        /// <summary>
        /// In case the user is typing and there can not be an entry point via a Symbol
        /// (eg for auto completion), the entry point has to be via the scope of the wrapping parent symbol.
        /// Use this method to get the parent symbol as en entry point. 
        /// </summary>
        //  weg 
        public ISymbol GetSymbolWrapperForCurrentScope(int line, int character)
        {
            ISymbol closestWrappingSymbol = null;
            foreach (var modul in SymbolTables)
            {
                INavigator navigator = new SymbolTableNavigator();
                closestWrappingSymbol = navigator.TopDown(modul.Value, line, character);
            }
            return closestWrappingSymbol;
        }

        /// <summary>
        /// Provide an entry point (symbol) and a string (name of a symbol) you are looking for.
        /// This method returns the nearest declaration with that name that can be found. 
        /// </summary>
        ///
        /// "mxTest"
        public ISymbol GetClosestSymbolByName(ISymbol entryPoint, string symbolName)
        {
            INavigator navigator = new SymbolTableNavigator();
            Predicate<ISymbol> filter = x => x.IsDeclaration && x.Name == symbolName;
            return navigator.BottomUpFirst(entryPoint, filter);
        }

        /// <summary>
        /// This returns all symbol declaration that are in scope for the given symbol.
        /// This recursive and can be used for functions like auto completion. 
        /// </summary>
        public List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol)
        {
            return GetAllDeclarationForSymbolInScope(symbol, new Predicate<ISymbol>(x => true));
        }
        public List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol, Predicate<ISymbol> preFilter)
        {
            INavigator navigator = new SymbolTableNavigator();
            Predicate<ISymbol> filter = x => x.IsDeclaration && x.Kind != Kind.Constructor && preFilter.Invoke(x);
            return navigator.BottomUpAll(symbol, filter);
        }

        /// <summary>
        /// Return itself if it is already a declaration.
        /// Used for Go2Definition. 
        /// </summary>
        public ISymbol GetOriginFromSymbol(ISymbol symbol)
        {
            return symbol.DeclarationOrigin;
        }

        /// <summary>
        /// For instances of classes - returns the origins "class type" as Smybol.
        /// Eg var instance = new ClassA();
        /// Calling this function with instance will return the symbol of ClassA (origin). 
        /// </summary>
        public ISymbol GetClassOriginFromSymbol(ISymbol symbol)
        {
            // todo mergen213 ClassMergen 
            var classPath = GetOriginFromSymbol(symbol).UserTypeDefinition.ResolvedClass.FullName;
            return GetClassSymbolByPath(classPath);
        }

        // CodeLens
        public List<ISymbol> GetUsagesOfSymbol(ISymbol symbol)
        {
            return null;
        }

        /// <summary>
        /// Gets all Symbols for features like CodeLens. 
        /// </summary>
        public List<ISymbol> GetAllSymbolDeclarations()
        {
            List<ISymbol> symbols = new List<ISymbol>();
            INavigator navigator = new SymbolTableNavigator();
            Predicate<ISymbol> filter = symbol => (
                symbol.IsDeclaration && (
                symbol.Kind == Kind.Class ||
                symbol.Kind == Kind.Function ||
                symbol.Kind == Kind.Method) &&
                // no constructors and make sure no out-of-range root _defaults
                symbol.Name != "_ctor" &&
                symbol?.Line != null && symbol.Line > 0
            );
            foreach (var module in SymbolTables)
            {
                symbols.AddRange(navigator.TopDownAll(module.Value, filter));
            }
            return symbols;
        }
    }
}
