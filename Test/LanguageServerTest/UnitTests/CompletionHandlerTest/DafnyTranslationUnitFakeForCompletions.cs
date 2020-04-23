using DafnyLanguageServer.DafnyAccess;
using Microsoft.Boogie;
using System;
using System.Collections.Generic;
using DafnyLanguageServer.Handler;
using DafnyServer;
using DafnyLanguageServer.FileManager;

namespace CompletionHandlerTest
{
    public class DafnyTranslationUnitFakeForCompletions : IDafnyTranslationUnit
    {
        public TranslationResult Verify()
        {
            return null;
        }

        public List<OldSymbolTable.OldSymbolInformation> Symbols()
        {
            List<OldSymbolTable.OldSymbolInformation> symbols = new List<OldSymbolTable.OldSymbolInformation>();
            symbols.Add(new OldSymbolTable.OldSymbolInformation { Name = "myFunction", Position = 0 });
            symbols.Add(new OldSymbolTable.OldSymbolInformation { Name = "myOtherFunction", Position = 1 });
            symbols.Add(new OldSymbolTable.OldSymbolInformation { Name = "duplicatedEntry", Position = 0 });
            symbols.Add(new OldSymbolTable.OldSymbolInformation { Name = "duplicatedEntry", Position = 0 });
            symbols.Add(new OldSymbolTable.OldSymbolInformation
            {
                Name = "ClassA",
                Line = 5,
                EndLine = 8,
                Position = 0,
                EndPosition = 5
            });
            symbols.Add(new OldSymbolTable.OldSymbolInformation
            {
                Name = "aFunctionInClassA",
                ParentClass = "ClassA",
                Line = 6,
                EndLine = 6,
                Position = 1,
                EndPosition = 10
            });
            symbols.Add(new OldSymbolTable.OldSymbolInformation
            {
                Name = "aFunctionNotInClassA",
                ParentClass = "ClassA",
                Line = 50,
                EndLine = 60,
                Position = 3,
                EndPosition = 4
            });
            symbols.Add(new OldSymbolTable.OldSymbolInformation { Name = "_ctor", Position = 0 });
            symbols.Add(new OldSymbolTable.OldSymbolInformation { Name = "_default", Position = 0 });
            return symbols;
        }

        public CounterExampleResults CounterExample()
        {
            return new CounterExampleResults();

        }

        public void DotGraph()
        {
            throw new NotImplementedException();
        }

        public List<DiagnosticElement> GetErrors()
        {
            throw new NotImplementedException();
        }

    }
}
