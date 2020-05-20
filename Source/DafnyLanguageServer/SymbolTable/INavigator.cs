using System;
using System.Collections.Generic;

namespace DafnyLanguageServer.SymbolTable
{
    public interface INavigator
    {
        ISymbol GetSymbolByPosition(ISymbol rootEntry, Uri file, int line, int character);
        ISymbol TopDown(ISymbol rootEntry, Uri file, int line, int character);
        List<ISymbol> TopDownAll(ISymbol symbol, Predicate<ISymbol> filter = null);
        ISymbol BottomUpFirst(ISymbol entryPoint, Predicate<ISymbol> filter = null);
        List<ISymbol> BottomUpAll(ISymbol symbol, Predicate<ISymbol> filter = null);
    }
}
