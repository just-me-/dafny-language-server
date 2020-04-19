using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public class SymbolInformation
    {
        public TokenPosition Position { get; set; }
        public int? LineStart => Position?.BodyStartToken.line;
        public int? LineEnd => Position?.BodyEndToken.line;
        public int? ColumnStart => Position?.BodyStartToken.col;
        public int? ColumnEnd => Position?.BodyEndToken.col;
        public string Name { get; set; }
        public Type Type { get; set; }
        public SymbolInformation Parent { get; set; }
        public SymbolInformation DeclarationOrigin { get; set; }
        public List<SymbolInformation> Children { get; set; }
        public List<SymbolInformation> Usages { get; set; }
        public bool IsDeclaration => DeclarationOrigin == this;

        public override string ToString()
        {
            return $"[L{LineStart}:C{ColumnStart}] \"{Name}\" | P : [L{Parent?.LineStart}]{Parent?.Name} | D : {(IsDeclaration ? "self" : "[L" + DeclarationOrigin?.LineStart + "]" + DeclarationOrigin?.Name)} | C : {Children?.Count} | U : {Usages?.Count}";
        }
    }

    public enum Type
    {
        Module,
        Class,
        Method,
        Function,
        Field,
        Variable,
        Call,
        Definition,
        Predicate,
        Undefined,
        BlockStmt
    }

    public class TokenPosition
    {
        public IToken BodyStartToken { get; set; }
        public IToken BodyEndToken { get; set; }
        public IToken Token { get; set; }
    }
}
