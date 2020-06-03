using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;

namespace CoreProviderTest
{
    public class SymbolManagerFakeForHover : ISymbolTableManager
    {
        private readonly bool returnDeclaration;
        private readonly bool returnFailure;

        public SymbolManagerFakeForHover(bool alreadyDefintion, bool failure)
        {
            returnDeclaration = alreadyDefintion;
            returnFailure = failure;
        }

        public ISymbol GetSymbolByPosition(Uri file, int line, int character)
        {
            if (returnFailure)
            {
                return null;
            }

            ISymbol root = new FakeSymbolTable().GenerateSymbolTable();
            ISymbol declaration = root["barapapa"];
            ISymbol notDeclaration = declaration.Usages[0];
            return returnDeclaration ? declaration : notDeclaration;
        }

        public ISymbol GetSymbolWrapperForCurrentScope(Uri file, int line, int character)
        {
            throw new InvalidOperationException("Hover provider should not use this method.");
        }

        public ISymbol GetClosestSymbolByName(ISymbol entryPoint, string symbolName)
        {
            throw new InvalidOperationException("Hover provider should not use this method.");
        }

        public List<ISymbol> GetAllDeclarationForSymbolInScope(ISymbol symbol, Predicate<ISymbol> filter = null)
        {
            throw new InvalidOperationException("Hover provider should not use this method.");
        }

        public ISymbol GetOriginFromSymbol(ISymbol symbol)
        {
            throw new InvalidOperationException("Hover provider should not use this method.");
        }

        public ISymbol GetClassOriginFromSymbol(ISymbol symbol)
        {
            throw new InvalidOperationException("Hover provider should not use this method.");
        }

        public List<ISymbol> GetAllSymbolDeclarations()
        {
            throw new InvalidOperationException("Hover provider should not use this method.");
        }
    }



    [TestFixture]
    [Category("Unit")]
    public class HoverProviderTest
    {
        [Test]
        public void TestRegularNotDefinition()
        {
            ISymbolTableManager manager = new SymbolManagerFakeForGoto(false, false);
            var provider = new HoverProvider(manager);

            var result = provider.GetHoverInformation(new Uri("file:///N:/u/l.l"), 0, 10); //note: params do not matter. the fake returns a fixed symbol and does not respect the position. those methods are tested within the symbol table tests.

            Assert.AreEqual(1, result.Range.Start.Line, "Start Line is not matching");
            Assert.AreEqual(21, result.Range.Start.Character, "Start Char is not matching");
            Assert.AreEqual(1, result.Range.End.Line, "End Line is not matching");
            Assert.AreEqual(29, result.Range.End.Character, "End Char is not matching");

            Console.WriteLine(result.Contents.MarkupContent.Value);
            string textExpectation = @"## Symbol Hover Information
*barapapa at Line 2 in l.l*
* **Kind:** RootNode
* **Type:** N/A
* **Scope:** `_programRootNode`
* **Declaration:** barapapa at Line 1 in l.l";

            //to get rid of linebreak mismatches in the linux CI: ev todo schauen obs geht.
            
            Assert.AreEqual(textExpectation, result.Contents.MarkupContent.Value, "Markup Content does not match");

        }
        

    }
}
