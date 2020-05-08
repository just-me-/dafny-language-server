using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.SymbolTable
{
    public interface IManager
    {
        ISymbol DafnyProgramRootSymbol { get; }
        ISymbol GetSymbolByPosition(int line, int character);
        ISymbol GetSymbolWrapperForCurrentScope(int line, int character);
        ISymbol GetClosestSymbolByName(ISymbol entryPoint, string symbolName);
        List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol);

        List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol,
            Predicate<ISymbol> filter);

        ISymbol GetOriginFromSymbol(ISymbol symbol);
        ISymbol GetClassOriginFromSymbol(ISymbol symbol);
        List<ISymbol> GetAllSymbolDeclarations();
    }
}
