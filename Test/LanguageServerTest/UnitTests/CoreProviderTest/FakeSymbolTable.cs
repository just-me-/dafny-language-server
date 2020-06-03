using System.Collections.Generic;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Boogie;

namespace CoreProviderTest
{
    public class FakeSymbolTable : ISymbolTableGenerator
    {
        /// <summary>
        /// Builds a short fake symbol table which can be used for testing.
        /// </summary>
        /// <returns>root symbol of the fake table</returns>
        /// <remarks> 
        ///                     root
        ///           another          decl
        ///                         use1   use2
        /// </remarks>
        public ISymbol GenerateSymbolTable()
        {
            ISymbol root = SymbolTableGenerator.GetEmptySymbolTable();
            SymbolInformation decl = new SymbolInformation
            {
                Position = new TokenPosition() { Token = new Token(1, 11) },
                Name = "barapapa",
                Parent = root

            };
            decl.DeclarationOrigin = decl;

            SymbolInformation use1 = new SymbolInformation
            {
                Position = new TokenPosition() { Token = new Token(2, 22) },
                Name = "barapapa",
                Parent = root,
                DeclarationOrigin = decl
            };

            SymbolInformation use2 = new SymbolInformation
            {
                Position = new TokenPosition() { Token = new Token(3, 33) },
                Name = "barapapa",
                Parent = root,
                DeclarationOrigin = decl

            };

            SymbolInformation anotherSymbol = new SymbolInformation
            {
                Position = new TokenPosition() { Token = new Token(4, 443) },
                Name = "bubu",
                Parent = root,
            };
            anotherSymbol.DeclarationOrigin = anotherSymbol;

            root.ChildrenHash = new Dictionary<string, ISymbol>();
            root.ChildrenHash.Add("barapapa", decl);
            root.ChildrenHash.Add("bubu", anotherSymbol);
            decl.Usages = new List<ISymbol> { use1, use2 };

            return root;
        }
    }
}
