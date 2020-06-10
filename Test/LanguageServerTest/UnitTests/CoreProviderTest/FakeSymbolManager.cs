using DafnyLanguageServer.SymbolTable;
using System;
using System.Collections.Generic;

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

        public ISymbolInformation GetSymbolByPosition(Uri file, int line, int character)
        {
            if (_returnNull)
            {
                return null;
            }

            ISymbolInformation root = new FakeSymbolTable().GenerateSymbolTable();
            ISymbolInformation declaration = root["barapapa"];
            ISymbolInformation notDeclaration = declaration.Usages[0];
            return _returnDeclaration ? declaration : notDeclaration;
        }

        public ISymbolInformation GetSymbolWrapperForCurrentScope(Uri file, int line, int character)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbolInformation GetClosestSymbolByName(ISymbolInformation entryPoint, string symbolName)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public List<ISymbolInformation> GetAllDeclarationForSymbolInScope(ISymbolInformation symbol, Predicate<ISymbolInformation> filter = null)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbolInformation GetOriginFromSymbol(ISymbolInformation symbol)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbolInformation GetClassOriginFromSymbol(ISymbolInformation symbol)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public List<ISymbolInformation> GetAllSymbolDeclarations()
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }

        public ISymbolInformation GetEnclosingClass(ISymbolInformation entryPoint)
        {
            throw new InvalidOperationException("This fake is not offering this method. Use it only for rename, hover or goto.");
        }
    }
}