using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DafnyProgram = Microsoft.Dafny.Program;

namespace DafnyLanguageServer.DafnyAccess
{


    public class MetaDafnySymboltableBaem
    {
        public List<SymbolInformation> SymbolTable { get; set; } = new List<SymbolInformation>();
        private DafnyProgram DafnyProgram { get; }

        public MetaDafnySymboltableBaem(DafnyProgram dafnyProgram)
        {
            DafnyProgram = dafnyProgram;
        }

        public void GenerateSymboltable()
        {
            foreach (var module in DafnyProgram.Modules())
            {
                AddClasses(module);
                //AddMethods(module);
                //AddFields(module);
            }
        }

        private void AddClasses(ModuleDefinition module)
        {
            foreach (var clbl in ModuleDefinition.AllCallables(module.TopLevelDecls)
                .Where(e => e != null && !(e.Tok is IncludeToken)))
            {

            }
        }


        public class SymbolInformation
        {
            public Position Position { get; set; }
            public string Name { get; set; }

            //evt wieder weg
            public ModuleDefinition Module { get; set; }

            public Type Type { get; set; }
            public SymbolInformation Parent { get; set; } // TopLevelDecl müsste man noch casten 
            public SymbolInformation DeclarationOrigin { get; set; }
            public List<SymbolInformation> Children { get; set; }
            public List<SymbolInformation> Usages { get; set; }


            public SymbolInformation()
            {

            }
        }

        public class Position
        {
            // col start end
            // line 
        }

        public enum Type
        {
            Class,
            Method,
            Function,
            Field,
            Call,
            Definition,
            Predicate
        }
    }
}