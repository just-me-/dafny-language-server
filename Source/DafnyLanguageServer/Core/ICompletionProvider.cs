using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.SymbolTable;

namespace DafnyLanguageServer.Core
{
    interface ICompletionProvider
    {
        CompletionType GetSupposedDesire(string line, int col);
        ISymbol GetWrappingEntrypointSymbol(Uri file, int line, int col);
        IEnumerable<ISymbol> GetSymbols(CompletionType desire, ISymbol wrappingEntrypointSymbol);
    }
}
