using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;
using System;
using System.Linq;

namespace SymbolTableTest
{
    internal class SymbolTableNavigatorTest
    {
        private ISymbolNavigator nav = new SymbolNavigator();
        private static readonly string defaultFile = @"C:/file.dfy";
        private static readonly Uri defaultUri = new Uri(@"C:/file.dfy");

        // TopDown
        [Test]
        public void GetTopDownSymbolSimple()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 3, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 2, 0, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbolInformation symbol = nav.TopDown(rootEntry, defaultUri, 2, 0);
            Assert.AreEqual(mySymbol, symbol);
        }

        [Test]
        public void GetTopDownSymbolDeep()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 10, 0, defaultFile, "Parent");
            rootEntry.AddChild(new SymbolInformationFake(1, 0, 1, 0, 2, 0, defaultFile, "SubParent1"));
            rootEntry.AddChild(new SymbolInformationFake(3, 0, 3, 0, 4, 0, defaultFile, "SubParent2"));
            var subParent = new SymbolInformationFake(5, 0, 5, 0, 10, 0, defaultFile, "RightSubParent");
            rootEntry.AddChild(subParent);
            SymbolInformationFake mySymbol = new SymbolInformationFake(6, 0, 6, 0, 10, 0, defaultFile, "MyChild");
            subParent.AddChild(mySymbol);
            ISymbolInformation symbol = nav.TopDown(rootEntry, defaultUri, 9, 0);
            Assert.AreEqual(mySymbol, symbol);
        }

        [Test]
        public void GetTopDownSymbolEmptyLine()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 10, 0, defaultFile, "Parent");
            rootEntry.AddChild(new SymbolInformationFake(1, 0, 1, 0, 4, 0, defaultFile, "SubParent1"));
            rootEntry.AddChild(new SymbolInformationFake(6, 0, 6, 0, 9, 0, defaultFile, "SubParent2"));
            ISymbolInformation symbol = nav.TopDown(rootEntry, defaultUri, 5, 0);
            Assert.AreEqual(rootEntry, symbol);
        }

        [Test]
        public void GetTopDownSymbolEmptyRoot()
        {
            ISymbolInformation symbol = nav.TopDown(null, defaultUri, 5, 0);
            Assert.IsNull(symbol);
        }

        // GetSymbolAtPosition
        [Test]
        public void GetSymbolByPositionEmptyList()
        {
            ISymbolInformation symbol = nav.GetSymbolAtPosition(null, defaultUri, 1, 1);
            Assert.IsNull(symbol);
        }

        [Test]
        public void GetSymbolByPositionTopLevel()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 1, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbolInformation symbol = nav.GetSymbolAtPosition(rootEntry, defaultUri, 1, 1);
            Assert.AreEqual(rootEntry, symbol);
        }

        [Test]
        public void GetSymbolByPositionNested()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 1, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 1, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbolInformation symbol = nav.GetSymbolAtPosition(rootEntry, defaultUri, 2, 1);
            Assert.AreEqual(mySymbol, symbol);
        }

        [Test]
        public void GetSymbolByPositionNotExist()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            ISymbolInformation symbol = nav.GetSymbolAtPosition(rootEntry, defaultUri, 6, 9);
            Assert.IsNull(symbol);
        }

        // TopDownAll
        [Test]
        public void TopDownAllEmptyList()
        {
            Predicate<ISymbolInformation> filter = (s => true);
            var symbol = nav.TopDownAll(null, filter);
            Assert.False(symbol.Any());
        }

        [Test]
        public void TopDownAllMatchChild()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddSubsymbol(mySymbol);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("Child"));
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.True(symbol.Count == 1);
            Assert.AreEqual(mySymbol, symbol.First());
        }

        [Test]
        public void TopDownAllMatchParent()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddSubsymbol(mySymbol);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("Parent"));
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.True(symbol.Count == 1);
            Assert.AreEqual(rootEntry, symbol.First());
        }

        [Test]
        public void TopDownAllMatchNoDefinition()
        {
            SymbolInformationFake rootEntry = null;
            Predicate<ISymbolInformation> filter = (s => true);
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.IsFalse(symbol.Any());
        }

        [Test]
        public void TopDownAllNotFound()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("NotExists"));
            var symbol = nav.TopDownAll(rootEntry, filter);
            Assert.False(symbol.Any());
        }

        // BottomUpFirst
        [Test]
        public void BottomUpFirstChild()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("Child"));
            var symbol = nav.BottomUpFirst(mySymbol, filter);
            Assert.AreEqual(mySymbol, symbol);
        }

        [Test]
        public void BottomUpFirstParent()
        {
            SymbolInformationFake moduleFake = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Module");
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            moduleFake.AddChild(rootEntry);
            rootEntry.SetParent(moduleFake);
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("Parent"));
            var symbol = nav.BottomUpFirst(mySymbol, filter);
            Assert.AreEqual(rootEntry, symbol);
        }

        [Test]
        public void BottomUpFirstNoMatch()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("NotExists"));
            var symbol = nav.BottomUpFirst(mySymbol, filter);
            Assert.AreEqual(null, symbol);
        }

        // BottomUpAll
        [Test]
        public void BottomUpAllChild()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            rootEntry.AddChild(mySymbol);
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("Child"));
            var symbol = nav.BottomUpAll(mySymbol, filter);
            Assert.AreEqual(mySymbol, symbol.First());
        }

        [Test]
        public void BottomUpAllParent()
        {
            SymbolInformationFake moduleFake = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Module");
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            moduleFake.AddChild(rootEntry);
            rootEntry.SetParent(moduleFake);
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("Parent"));
            var symbol = nav.BottomUpAll(mySymbol, filter);
            Assert.AreEqual(rootEntry, symbol.First());
        }

        [Test]
        public void BottomUpAllNoMatch()
        {
            SymbolInformationFake rootEntry = new SymbolInformationFake(1, 0, 1, 0, 5, 0, defaultFile, "Parent");
            SymbolInformationFake mySymbol = new SymbolInformationFake(2, 0, 2, 0, 5, 0, defaultFile, "Child");
            mySymbol.SetParent(rootEntry);
            Predicate<ISymbolInformation> filter = (s => s.Name.Equals("NotExists"));
            var symbol = nav.BottomUpAll(mySymbol, filter);
            Assert.False(symbol.Any());
        }
    }
}