using System;
using System.Collections.Generic;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolTableManager
    {
        ISymbol GetSymbolByPosition(Uri file, int line, int character);
        ISymbol GetSymbolWrapperForCurrentScope(Uri file, int line, int character);
        ISymbol GetClosestSymbolByName(ISymbol entryPoint, string symbolName);
        List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol);

        List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol, Predicate<ISymbol> filter);

        ISymbol GetOriginFromSymbol(ISymbol symbol);
        ISymbol GetClassOriginFromSymbol(ISymbol symbol);
        List<ISymbol> GetAllSymbolDeclarations();
    }
}
