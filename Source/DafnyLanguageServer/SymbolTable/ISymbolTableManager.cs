using System;
using System.Collections.Generic;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolTableManager
    {
        ISymbolInformation GetSymbolByPosition(Uri file, int line, int character);
        ISymbolInformation GetSymbolWrapperForCurrentScope(Uri file, int line, int character);
        ISymbolInformation GetClosestSymbolByName(ISymbolInformation entryPoint, string symbolName);
        List<ISymbolInformation> GetAllDeclarationForSymbolInScope(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null);
        ISymbolInformation GetClassOriginFromSymbol(ISymbolInformation symbol);
        List<ISymbolInformation> GetAllSymbolDeclarations();
    }
}
