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
    /// Mostly there is only one module - the default module for a single Dafny file. 
    /// </summary>
    public class SymbolTableManager
    {
        private readonly Microsoft.Dafny.Program _dafnyProgram;
        public Dictionary<string, List<SymbolInformation>> SymbolTables { get; set; } = new Dictionary<string, List<SymbolInformation>>();  //still nicht sicher ob das sinnvoll ist.... eig könnt man auch alles in eine einzige tabelle aggregieren mit allen modulen (?)

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

                SymbolTables.Add(module.Name, deepVisitor.SymbolTable);

                string debugMe = CreateDebugReadOut();
            }
        }

        public string CreateDebugReadOut()
        {
            StringBuilder b = new StringBuilder();
            foreach (var kvp in SymbolTables)
            {
                b.AppendLine("Module: " + kvp.Key);
                foreach (var symbol in kvp.Value)
                {
                    b.AppendLine(symbol.ToString());
                }
            }
            return b.ToString();
        }

        public List<string> GetEntriesAsStringList()
        {
            return (from kvp in SymbolTables from symbol in kvp.Value select symbol.ToString()).ToList();
        }

        // EntryPoint... not by name:string, right? 
        public SymbolInformation GetSymbolByPosition(int line, int character)
        {
            var tmpDebugList = new List<SymbolInformation>();
            foreach (var modul in SymbolTables)
            {
                foreach (var symbolInformation in modul.Value)
                {
                    if (PositionIsInSymbolsRange(line, character, symbolInformation))
                        tmpDebugList.Add(symbolInformation);
                }
            }
            return tmpDebugList.Count > 0 ? tmpDebugList[0] : null;
        }

        public SymbolInformation GetClassSymbolByPath(string classPath)
        {
            // todo better do a split? is there rly every time "just one module and one class"? #212
            string[] originPath = classPath.Split('.'); // 0 = module, 1 = class
            if (originPath.Length != 2)
            {
                throw new ArgumentException("Invalid class path... expected Module.Class pattern."); //tmp
            }
            return SymbolTables[originPath[0]]?.Find(symbol => symbol.Name == originPath[1] && symbol.IsDeclaration);
        }

        private bool PositionIsInSymbolsRange(int line, int character, SymbolInformation symbol)
        {
            return (symbol.Line <= line
                    && symbol.Line >= line
                    && symbol.ColumnStart <= character
                    && symbol.ColumnEnd >= character);
        }

        // Wenn man ein Symbol liefert "parent" erhalten für AutoCompletion 2do #97
        // Autocompletion "ohne pre-symbol"
        public SymbolInformation GetScopeSymbolForSymbol(SymbolInformation symbol)
        {
            return null;
            // Mby rm duplicates and constructor 
        }

        /// <summary>
        /// Return itself if it is already a declaration.
        /// Used for Go2Definition. 
        /// </summary>
        public SymbolInformation GetOriginFromSymbol(SymbolInformation symbol)
        {
            return symbol.DeclarationOrigin;
        }

        // CodeLens
        public List<SymbolInformation> GetUsagesOfSymbol(SymbolInformation symbol)
        {
            return null;
        }
    }
}
