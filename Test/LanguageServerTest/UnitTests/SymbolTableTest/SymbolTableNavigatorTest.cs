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

        // TopDown
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
            ISymbol symbol = nav.TopDown(null, 5, 0);
            Assert.IsNull(symbol);
        }

        // GetSymbolByPosition
        [Test]
        public void GetSymbolByPositionEmptyList()
        {
            ISymbol symbol = nav.GetSymbolByPosition(null, 1, 1);
            Assert.IsNull(symbol);
        }

        [Test]
        public void GetSymbolByPositionTopLevel()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbol symbol = nav.GetSymbolByPosition(rootEntry, 1, 1);
            Assert.AreEqual(rootEntry, symbol);
        }

        [Test]
        public void GetSymbolByPositionNested()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbol symbol = nav.GetSymbolByPosition(rootEntry, 2, 1);
            Assert.AreEqual(mySymbol, symbol);
        }

        [Test]
        public void GetSymbolByPositionNotExist()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbol symbol = nav.GetSymbolByPosition(rootEntry, 6, 9);
            Assert.IsNull(symbol);
        }

        // TopDownAll
        [Test]
        public void TopDownAllEmptyList()
        {
            Predicate<ISymbol> filter = (s => true);
            var symbol = nav.TopDownAll(null, filter);
            Assert.False(symbol.Any());
        }

        [Test]
        public void TopDownAllMatchChild()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            rootEntry.AddSubsymbol(mySymbol);
            Predicate<ISymbol> filter = (s => s.Name.Equals("Child"));
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.True(symbol.Count == 1);
            Assert.AreEqual(mySymbol, symbol.First());
        }

        [Test]
        public void TopDownAllMatchParent()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            rootEntry.AddSubsymbol(mySymbol);
            Predicate<ISymbol> filter = (s => s.Name.Equals("Parent"));
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.True(symbol.Count == 1);
            Assert.AreEqual(rootEntry, symbol.First());
        }

        [Test]
        public void TopDownAllMatchNoDefinition()
        {
            SymbolInformationFake rootEntry = null;
            Predicate<ISymbol> filter = (s => true);
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.IsFalse(symbol.Any());
        }

        [Test]
        public void TopDownAllNotFound()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            rootEntry.AddChild(mySymbol);
            Predicate<ISymbol> filter = (s => s.Name.Equals("NotExists"));
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.False(symbol.Any());
        }

        // BottomUpFirst
        [Test]
        public void BottomUpFirstChild()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbol> filter = (s => s.Name.Equals("Child"));
            var symbol = nav.BottomUpFirst(mySymbol, filter);
            Assert.Equals(mySymbol, symbol);
        }

        [Test]
        public void BottomUpFirstParent()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbol> filter = (s => s.Name.Equals("Parent"));
            var symbol = nav.BottomUpFirst(mySymbol, filter);
            Assert.Equals(rootEntry, symbol);
        }

        [Test]
        public void BottomUpFirstNoMatch()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 5, 0, 0, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 5, 0, 0, "Child");
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbol> filter = (s => s.Name.Equals("NotExists"));
            var symbol = nav.BottomUpFirst(mySymbol, filter);
            Assert.Equals(null, symbol);
        }

        // BottomUpAll

    }
}
