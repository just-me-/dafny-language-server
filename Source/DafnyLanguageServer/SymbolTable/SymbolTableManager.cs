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
    /// </summary>
    public class SymbolTableManager
    {
        private Microsoft.Dafny.Program _dafnyProgram;
        public Dictionary<string, List<SymbolInformation>> SymbolTables { get; set; } = new Dictionary<string, List<SymbolInformation>>();

        public SymbolTableManager(Microsoft.Dafny.Program dafnyProgram)
        {
            this._dafnyProgram = dafnyProgram;
            GenerateSymbolTable();
        }

        private void GenerateSymbolTable()
        {
            foreach (var module in _dafnyProgram.Modules())
            {
                var visitor = new SymbolTableVisitor();
                module.Accept(visitor);

                SymbolTables.Add(module.Name, visitor.SymbolTable);

                string debugMe = CreateDebugReadOut(visitor.SymbolTable);
            }
        }

        private static string CreateDebugReadOut(List<SymbolInformation> visitorSymbolTable)
        {
            StringBuilder b = new StringBuilder();
            foreach (var symbol in visitorSymbolTable)
            {
                b.AppendLine(symbol.ToString());
            }
            return b.ToString();
        }

        // EntryPoint... not by name:string, right? 
        public SymbolInformation GetSymbolByPosition()
        {
            // for each... kann eig nicht mehrere geben aber evt fehler bei classes und co.
            // daher array zum start / v1
            // if symbol is in range
            return null; 
        }

        // Wenn man ein Symbol liefert "parent" erhalten für AutoCompletion 2do #97
        // Autocompletion "ohne pre-symbol"
        public SymbolInformation GetScopeSymbolForSymbol(SymbolInformation symbol)
        {
            return null; 
            // Mby rm duplicates and constructor 
        }

        // Go2Definition 
        public SymbolInformation GetOriginFromSymbol(SymbolInformation symbol)
        {
            return symbol.Parent;
        }

        // CodeLens
        public List<SymbolInformation> GetUsagesOfSymbol(SymbolInformation symbol)
        {
            return null; 
        }
    }
}
