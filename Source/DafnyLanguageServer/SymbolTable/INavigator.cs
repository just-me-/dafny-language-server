using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.SymbolTable
{
    public interface INavigator
    {
        ISymbol GetSymbolByPosition(ISymbol rootEntry, int line, int character);
        ISymbol TopDown(ISymbol rootEntry, int line, int character);
        List<ISymbol> TopDownAll(ISymbol symbol, Predicate<ISymbol> filter);
        ISymbol BottomUpFirst(ISymbol entryPoint, Predicate<ISymbol> filter);
        List<ISymbol> BottomUpAll(ISymbol symbol, Predicate<ISymbol> filter);
    }
}
