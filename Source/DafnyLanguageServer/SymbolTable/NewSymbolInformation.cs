using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public class NewSymbolInformation
    {
        public TokenPosition Position { get; set; }

        public int Line => Position.Token.line;
        public int Col => Position.Token.col;
        public string Name { get; set; }

        //evt wieder weg
        public ModuleDefinition Module { get; set; }

        public Type Type { get; set; }
        public NewSymbolInformation Parent { get; set; }
        public NewSymbolInformation DeclarationOrigin { get; set; }
        public List<NewSymbolInformation> Children { get; set; } = new List<NewSymbolInformation>();
        public List<NewSymbolInformation> Usages { get; set; } = new List<NewSymbolInformation>();
        public bool IsDeclaration => DeclarationOrigin == this;

    }


    public enum Type
    {
        Class,
        Method,
        Function,
        Field,
        Variable,
        Call,
        Definition,
        Predicate
    }

    public class TokenPosition
    {
        public IToken BodyStartToken { get; set; }
        public IToken BodyEndToken { get; set; }
        public IToken Token { get; set; }
    }
}
