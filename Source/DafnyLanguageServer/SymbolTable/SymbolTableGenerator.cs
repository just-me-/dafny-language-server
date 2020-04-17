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
        private Microsoft.Dafny.Program dafnyProgram;
        public Dictionary<string, List<SymbolInformation>> SymbolTables { get; set; } = new Dictionary<string, List<SymbolInformation>>();

        public SymbolTableGenerator(Microsoft.Dafny.Program dafnyProgram)
        {
            this.dafnyProgram = dafnyProgram;
            GenerateTable();
        }

        public void GenerateTable()
        {
            foreach (var module in dafnyProgram.Modules())
            {
                var visitor = new VisitorThatGeneratesSymbolTable();
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
