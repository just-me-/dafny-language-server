using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public class SymbolTableGenerator
    {




        public SymbolTableGenerator(Microsoft.Dafny.Program dafnyProgram)
        {
            foreach (var module in dafnyProgram.Modules())
            {
                var allClasses = ModuleDefinition.AllClasses(module.TopLevelDecls);
                foreach (ClassDecl cd in allClasses)
                {
                    var visitor = new SymbolTableGeneratorVisitor();
                    cd.Accept(visitor);
                }
            }
        }

    }
}
