using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Layout;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;

namespace SymbolTableTest
{
    class SymbolTableNavigatorTest
    {
        INavigator nav = new SymbolTableNavigator();
        // rootEntry, line, character

        [Test]
        public void GetTopDownSymbolSimple()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 3, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 2, 0, 0, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbol symbol = nav.TopDown(rootEntry, 2, 0);
            Assert.AreEqual(mySymbol, symbol);
        }

        [Test]
        public void GetTopDownSymbolDeep()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 10, 0, 0, "Parent");
            rootEntry.AddChild(new SymbolInformationFake(1, 2, 0, 0, "SubParent1"));
            rootEntry.AddChild(new SymbolInformationFake(3, 4, 0, 0, "SubParent2"));
            var subParent = new SymbolInformationFake(5, 10, 0, 0, "RightSubParent");
            rootEntry.AddChild(subParent);
            SymbolInformationFake mySymbol = new SymbolInformationFake(6, 10, 0, 0, "MyChild");
            subParent.AddChild(mySymbol);
            ISymbol symbol = nav.TopDown(rootEntry, 9, 0);
            Assert.AreEqual(mySymbol, symbol);
        }
        [Test]
        public void GetTopDownSymbolEmptyLine()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 10, 0, 0, "Parent");
            rootEntry.AddChild(new SymbolInformationFake(1, 4, 0, 0, "SubParent1"));
            rootEntry.AddChild(new SymbolInformationFake(6, 9, 0, 0, "SubParent2"));
            ISymbol symbol = nav.TopDown(rootEntry, 5, 0);
            Assert.AreEqual(rootEntry, symbol);
        }

        [Test]
        public void GetTopDownSymbolEmptyRoot()
        {
            SymbolInformationFake rootEntry = null;
            ISymbol symbol = nav.TopDown(rootEntry, 5, 0);
            Assert.IsNull(symbol);
        }

    }
}
