using DafnyLanguageServer.DafnyAccess;
using Microsoft.Boogie;
using System;
using System.Collections.Generic;

namespace CompletionHandlerTest
{
    public class DafnyTranslationUnitFakeForCompletions : IDafnyTranslationUnit
    {
        public bool IsNice;

        public DafnyTranslationUnitFakeForCompletions(bool isnice = true)
        {
            IsNice = isnice;
        }

        public List<ErrorInformation> Errors { get; } = new List<ErrorInformation>();

        public bool Verify()
        {
            return IsNice;
        }

        public List<SymbolTable.SymbolInformation> Symbols()
        {
            List<SymbolTable.SymbolInformation> symbols = new List<SymbolTable.SymbolInformation>();
            symbols.Add(new SymbolTable.SymbolInformation { Name = "myFunction", Position = 0 });
            symbols.Add(new SymbolTable.SymbolInformation { Name = "myOtherFunction", Position = 1 });
            symbols.Add(new SymbolTable.SymbolInformation { Name = "duplicatedEntry", Position = 0 });
            symbols.Add(new SymbolTable.SymbolInformation { Name = "duplicatedEntry", Position = 0 });
            symbols.Add(new SymbolTable.SymbolInformation
            {
                Name = "ClassA",
                Line = 5,
                EndLine = 8,
                Position = 0,
                EndPosition = 5
            });
            symbols.Add(new SymbolTable.SymbolInformation
            {
                Name = "aFunctionInClassA",
                ParentClass = "ClassA",
                Line = 6,
                EndLine = 6,
                Position = 1,
                EndPosition = 10
            });
            symbols.Add(new SymbolTable.SymbolInformation
            {
                Name = "aFunctionNotInClassA",
                ParentClass = "ClassA",
                Line = 50,
                EndLine = 60,
                Position = 3,
                EndPosition = 4
            });
            symbols.Add(new SymbolTable.SymbolInformation { Name = "_ctor", Position = 0 });
            symbols.Add(new SymbolTable.SymbolInformation { Name = "_default", Position = 0 });
            return symbols;
        }

        public List<CounterExampleProvider.CounterExample> CounterExample()
        {
            List<CounterExampleProvider.CounterExample> ces = new List<CounterExampleProvider.CounterExample>();
            CounterExampleProvider.CounterExample ce = new CounterExampleProvider.CounterExample();
            ces.Add(ce);

            return ces;
        }

        public void DotGraph()
        {
            throw new NotImplementedException();
        }

        public List<ErrorInformation> GetErrors()
        {
            throw new NotImplementedException();
        }
    }
}
