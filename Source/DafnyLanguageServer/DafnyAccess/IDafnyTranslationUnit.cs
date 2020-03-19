using Microsoft.Boogie;
using System.Collections.Generic;

namespace DafnyLanguageServer.DafnyAccess
{
    public interface IDafnyTranslationUnit
    {
        bool Verify();
        List<DiagnosticError> GetErrors();
        List<SymbolTable.SymbolInformation> Symbols();
        List<CounterExampleProvider.CounterExample> CounterExample();
        void DotGraph();
    }
}
