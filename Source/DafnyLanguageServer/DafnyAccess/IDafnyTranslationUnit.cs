using Microsoft.Boogie;
using System.Collections.Generic;
using DafnyLanguageServer.Handler;

namespace DafnyLanguageServer.DafnyAccess
{
    public interface IDafnyTranslationUnit
    {
        bool Verify();
        List<DiagnosticError> GetErrors();
        List<SymbolTable.SymbolInformation> Symbols();
        CounterExampleResults CounterExample();
        void DotGraph();
    }
}
