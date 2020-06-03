using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;

namespace CoreProviderTest
{
    public class FakeSymbolManager : ISymbolTableManager
    {
        private readonly bool _returnDeclaration;
        private readonly bool _returnNull;

        public FakeSymbolManager(bool returnsTheDeclaration, bool returnsNull)
        {
            _returnDeclaration = returnsTheDeclaration;
            _returnNull = returnsNull;
        }

        public ISymbol GetSymbolByPosition(Uri file, int line, int character)
        {
            if (_returnNull)
            {
                return null;
            }

            ISymbol root = new FakeSymbolTable().GenerateSymbolTable();
            ISymbol declaration = root["barapapa"];
            ISymbol notDeclaration = declaration.Usages[0];
            return _returnDeclaration ? declaration : notDeclaration;
        }

        public ISymbol GetSymbolWrapperForCurrentScope(Uri file, int line, int character)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbol GetClosestSymbolByName(ISymbol entryPoint, string symbolName)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbol GetOriginFromSymbol(ISymbol symbol)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbol GetClassOriginFromSymbol(ISymbol symbol)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public List<ISymbol> GetAllSymbolDeclarations()
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }
    }

}
