using System;
using System.Collections.Generic;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface ICompletionProvider
    {

        List<CompletionItem> FindCompletionItems(Uri file, int line, int col, string codeLine);
        CompletionType GetSupposedDesire(string line, int col);
        ISymbol GetWrappingEntrypointSymbol(Uri file, int line, int col);
        IEnumerable<ISymbol> GetSymbols(CompletionType desire, ISymbol wrappingEntrypointSymbol);
    }
}
