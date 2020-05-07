using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;
using TestCommons;

namespace SymbolTableTest
{
    class SymbolInformationTest
    {
        [Test]
        public void SymbolWrapsSymbol()
        {
            ISymbol symbolA = new SymbolInformationFake(0, 2, 0, 0);
            ISymbol symbolB = new SymbolInformationFake(1, 1, 0, 0);
            Assert.True(symbolA.Wraps(symbolB));
        }
        [Test]
        public void SymbolWrapsItself()
        {
            ISymbol symbol = new SymbolInformationFake(0, 2, 0, 0);
            Assert.True(symbol.Wraps(symbol));
        }
        [Test]
        public void SymbolWrapsPosition()
        {
            ISymbol symbol = new SymbolInformationFake(0, 2, 0, 0);
            Assert.True(symbol.Wraps(1, 0));
        }
        [Test]
        public void SymbolWrapsPositionOnSameLine()
        {
            ISymbol symbol = new SymbolInformationFake(0, 0, 0, 2);
            Assert.True(symbol.Wraps(0, 1));
        }
        [Test]
        public void SymbolDoesNotWrapPosition()
        {
            ISymbol symbol = new SymbolInformationFake(0, 0, 0, 2);
            Assert.False(symbol.Wraps(1, 1));
        }
        [Test]
        public void SymbolDoesNotWrapPositionOnSameLine()
        {
            ISymbol symbol = new SymbolInformationFake(0, 0, 0, 2);
            Assert.False(symbol.Wraps(0, 5));
        }
        // equals testen todo
    }
}
