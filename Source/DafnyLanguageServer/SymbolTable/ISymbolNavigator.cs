using System;
using System.Collections.Generic;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolNavigator
    {
        ISymbolInformation GetSymbolByPosition(ISymbolInformation rootEntry, Uri file, int line, int character);
        ISymbolInformation TopDown(ISymbolInformation rootEntry, Uri file, int line, int character);
        List<ISymbolInformation> TopDownAll(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null);
        ISymbolInformation BottomUpFirst(ISymbolInformation entryPoint, Predicate<ISymbolInformation> filter = null);
        List<ISymbolInformation> BottomUpAll(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null);
    }
}
