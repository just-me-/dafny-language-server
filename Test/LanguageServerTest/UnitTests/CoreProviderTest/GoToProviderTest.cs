using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;

namespace CoreProviderTest
{
    


    [TestFixture]
    [Category("Unit")]
    public class GoToProviderTest
    {
        [Test]
        public void TestRegularNotDefinition()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new DefinitionsProvider(manager);

            var result = provider.GetDefinitionLocation(new Uri("file:///N:/u/l.l"), 0, 10); //note: params do not matter. the fake returns a fixed symbol and does not respect the position. those methods are tested within the symbol table tests.

            Assert.AreEqual("file:///N:/u/l.l", result.FirstOrDefault().Location.Uri.ToString());
            Assert.AreEqual(0, result.FirstOrDefault().Location.Range.Start.Line, "Start Line is not matching");
            Assert.AreEqual(10, result.FirstOrDefault().Location.Range.Start.Character, "Start Char is not matching");
            Assert.AreEqual(0, result.FirstOrDefault().Location.Range.End.Line, "End Line is not matching");
            Assert.AreEqual(10, result.FirstOrDefault().Location.Range.End.Character, "End Char is not matching");
            Assert.AreEqual(DefinitionsOutcome.Success, provider.Outcome);

        }

        [Test]
        public void TestRegularAlreadyDefinition()
        {
            ISymbolTableManager manager = new FakeSymbolManager(true, false);
            var provider = new DefinitionsProvider(manager);

            var result = provider.GetDefinitionLocation(new Uri("file:///N:/u/l.l"), 0, 10); //note: params do not matter. the fake returns a fixed symbol and does not respect the position. those methods are tested within the symbol table tests.

            Assert.AreEqual("file:///N:/u/l.l", result.FirstOrDefault().Location.Uri.ToString());
            Assert.AreEqual(0, result.FirstOrDefault().Location.Range.Start.Line, "Start Line is not matching");
            Assert.AreEqual(10, result.FirstOrDefault().Location.Range.Start.Character, "Start Char is not matching");
            Assert.AreEqual(0, result.FirstOrDefault().Location.Range.End.Line, "End Line is not matching");
            Assert.AreEqual(10, result.FirstOrDefault().Location.Range.End.Character, "End Char is not matching");
            Assert.AreEqual(DefinitionsOutcome.WasAlreadyDefintion, provider.Outcome);

        }

        [Test]
        public void TestNoSymbol()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, true);
            var provider = new DefinitionsProvider(manager);

            var result = provider.GetDefinitionLocation(new Uri("file:///N:/u/l.l"), 0, 10); //note: params do not matter. the fake returns a fixed symbol and does not respect the position. those methods are tested within the symbol table tests.

            Assert.IsFalse(result.Any());

            Assert.AreEqual(DefinitionsOutcome.NotFound, provider.Outcome);

        }


    }
}
