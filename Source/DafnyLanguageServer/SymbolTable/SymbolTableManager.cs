﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// Provides all needed SymbolTableInformation for all Modules. Needs a Dafny Program to work. 
    /// Mostly there is only one module - the default module (and class) for a single Dafny file.
    /// This class also works as a facade for <c>SymbolNavigator</c>.
    /// </summary>
    public class SymbolTableManager : IManager
    {
        private readonly Microsoft.Dafny.Program _dafnyProgram;

        /// <summary>
        /// <c>SymbolTables</c> is a key-value-hash. Key is the module name (string) and value is a set of <c>SymbolInformation.</c>
        /// A SymbolTable (List of <c>SymbolInformation</c> for each module) is sorted.
        /// </summary>
        public Dictionary<string, ISymbol> SymbolTables { get; set; } = new Dictionary<string, ISymbol>();

        /// <summary>
        /// A virtual Root Symbol. It Covers all range, can not have a parent, and has all Top Level Modules as Descendants.
        /// </summary>
        public ISymbol DafnyProgramRootSymbol { get; } // todo SymbolTable ersetzen durch diesen einstiegspunkt? 

        public SymbolTableManager(Microsoft.Dafny.Program dafnyProgram)
        {
            _dafnyProgram = dafnyProgram;
            DafnyProgramRootSymbol = CreateRootNode();
            DafnyProgramRootSymbol.DeclarationOrigin = DafnyProgramRootSymbol;

            GenerateSymbolTable();
        }

        private ISymbol CreateRootNode()
        {
            return new SymbolInformation()
            {
                ChildrenHash = new Dictionary<string, ISymbol>(),
                Descendants = new List<ISymbol>(),
                Kind = Kind.RootNode,
                Name = "_programRootNode",
                Position = new TokenPosition()
                {
                    Token = new Token(0, 0),
                    BodyStartToken = new Token(0, 0),
                    BodyEndToken = new Token(int.MaxValue, int.MaxValue)
                }
            };
        }

        private void GenerateSymbolTable()
        {

            foreach (var module in _dafnyProgram.Modules())
            {
                var declarationVisitor = new LanguageServerDeclarationVisitor(DafnyProgramRootSymbol);
                module.Accept(declarationVisitor);
                var symbolForModuleThatWasVisited = declarationVisitor.Module;
            }

            foreach (var module in _dafnyProgram.Modules())
            {
                var deepVisitor = new SymbolTableVisitorEverythingButDeclarations(DafnyProgramRootSymbol);
                module.Accept(deepVisitor);


                var symbolOfTopLevelModule = deepVisitor.Module;

                if (symbolOfTopLevelModule.Kind != Kind.Module)
                {
                    throw new InvalidOperationException("First Symbol after visiting a module must be the module, but it isnt.");
                }

                SymbolTables.Add(module.Name, symbolOfTopLevelModule);
            }

            string debugMe = CreateDebugReadOut();
        }



        public string CreateDebugReadOut()
        {
            StringBuilder b = new StringBuilder();
            foreach (var kvp in SymbolTables)
            {
                var nav = new SymbolTableNavigator();
                b.AppendLine("Module: " + kvp.Key);
                var rootSymbol = kvp.Value;

                var allSymbs = nav.TopDownAll(rootSymbol);

                foreach (var symbol in allSymbs)
                {
                    b.AppendLine(symbol.ToString());
                }

                b.AppendLine();
            }
            return b.ToString();
        }



        // ab hier das meiste auslagern(?)
        // man kann das erst auslagern, wenn module als base symbol implementiert wurde. 
        // bis dann brauchen wir auf diesem level einen base iterator durch alle module todo

        public ISymbol GetSymbolByPosition(long line, long character)
        {
            return GetSymbolByPosition((int)line, (int)character); //cast should be safe in real world examples.
        }
        // weg 
        public ISymbol GetSymbolByPosition(int line, int character)
        {
            INavigator navigator = new SymbolTableNavigator();
            ISymbol symbol = null;
            foreach (var modul in SymbolTables) //todo: neu wäre das foreach (var modul in rootNode.Descendants)  (bei merge concflcict: nicht dieses nehmen)
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
            return SymbolTables[originPath[0]][originPath[1]];  //todo: neu wäre das rootNode[ 0path ding] [1pathding]  (bei merge concflcict: nicht dieses nehmen)
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
            foreach (var modul in SymbolTables)   //todo: neu wäre das foreach (var modul in rootNode.Descendants)  (bei merge concflcict: nicht dieses nehmen) nur kommentar
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
            return GetAllDeclarationForSymbolInScope(symbol, x => true);
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
                symbol.Kind != Kind.Constructor &&
                symbol?.Line != null && symbol.Line > 0
            );
            foreach (var module in SymbolTables)  //todo: neu wäre das foreach (var modul in rootNode.Descendants)  (bei merge concflcict: nicht dieses nehmen)
            {
                symbols.AddRange(navigator.TopDownAll(module.Value, filter));
            }
            return symbols;
        }
    }
}
