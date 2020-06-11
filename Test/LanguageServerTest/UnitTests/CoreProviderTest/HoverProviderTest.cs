using DafnyLanguageServer.Core;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;
using System;

namespace CoreProviderTest
{
    [TestFixture]
    [Category("Unit")]
    public class HoverProviderTest
    {
        [Test]
        public void TestRegular()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new HoverProvider(manager);

            //note: params do not matter. the fake returns a fixed symbol and does not respect the position. those methods are tested within the symbol table tests.
            var result = provider.GetHoverInformation(new Uri("file:///N:/u/l.l"), 0, 10);

            Assert.AreEqual(1, result.Range.Start.Line, "Start Line is not matching");
            Assert.AreEqual(21, result.Range.Start.Character, "Start Char is not matching");
            Assert.AreEqual(1, result.Range.End.Line, "End Line is not matching");
            Assert.AreEqual(29, result.Range.End.Character, "End Char is not matching");

            Console.WriteLine(result.Contents.MarkupContent.Value);

            string textExpectation = " **`barapapa`** *(line 2, l.l)*\n" +
                                     "* **Type:** N/A\n" +
                                     "* **Kind:** RootNode\n" +
                                     "* **Scope:** `_programRootNode`\n" +
                                     "* **Declaration:** line 1, l.l";

            string actual = result.Contents.MarkupContent.Value.Replace("N:\\u\\l.l", "l.l");

            Assert.AreEqual(textExpectation, actual, "Markup Content does not match");
        }

        [Test]
        public void TestRegular2()
        {
            ISymbolTableManager manager = new FakeSymbolManager(true, false);
            var provider = new HoverProvider(manager);

            var result = provider.GetHoverInformation(new Uri("file:///N:/u/l.l"), 0, 10);

            Assert.AreEqual(0, result.Range.Start.Line, "Start Line is not matching");
            Assert.AreEqual(10, result.Range.Start.Character, "Start Char is not matching");
            Assert.AreEqual(0, result.Range.End.Line, "End Line is not matching");
            Assert.AreEqual(18, result.Range.End.Character, "End Char is not matching");

            Console.WriteLine(result.Contents.MarkupContent.Value);
            string textExpectation = " **`barapapa`** *(line 1, l.l)*\n" +
                                     "* **Type:** N/A\n" +
                                     "* **Kind:** RootNode\n" +
                                     "* **Scope:** `_programRootNode`\n" +
                                     "* **Declaration:** This symbol is the declaration.";
            string actual = result.Contents.MarkupContent.Value.Replace("N:\\u\\l.l", "l.l");

            Assert.AreEqual(textExpectation, actual, "Markup Content does not match");
        }

        [Test]
        public void TestNull()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, true);
            var provider = new HoverProvider(manager);

            var result = provider.GetHoverInformation(new Uri("file:///N:/u/l.l"), 0, 10);

            Assert.IsNull(result);
        }
    }
}