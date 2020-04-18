using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public class SymbolTableGenerator
    {
        private Microsoft.Dafny.Program _dafnyProgram;
        public Dictionary<string, List<SymbolInformation>> SymbolTables { get; set; } = new Dictionary<string, List<SymbolInformation>>();

        public SymbolTableGenerator(Microsoft.Dafny.Program dafnyProgram)
        {
            this._dafnyProgram = dafnyProgram;
            GenerateTable();
        }

        public void GenerateTable()
        {
            foreach (var module in _dafnyProgram.Modules())
            {
                var visitor = new SymbolTableVisitor();
                module.Accept(visitor);

                SymbolTables.Add(module.Name, visitor.SymbolTable);

                string debugMe = CreateReadOut(visitor.SymbolTable);
            }
        }

        private static string CreateReadOut(List<SymbolInformation> visitorSymbolTable)
        {
            StringBuilder b = new StringBuilder();
            foreach (var symbol in visitorSymbolTable)
            {
                b.AppendLine(symbol.ToString());
            }

            return b.ToString();
        }
    }
}
