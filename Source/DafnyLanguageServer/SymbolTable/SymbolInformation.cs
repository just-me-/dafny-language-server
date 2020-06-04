using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// The actual "string" name for a Symbol is the base <c>SymbolInformation</c>.
    /// Eg in <c>function myMethod {...}</c> the Symbol (and its position data) is myMethod.
    /// To get the position of { use BodyStartToken and for } use BodyEndToken. Those positions are only available for
    /// types like methods, functions, class, ... not for fields or variables. Just like you would expect.
    /// </summary>
    public class SymbolInformation : ISymbolInformation
    {
        #region Position-Name-Token-Uri
        public TokenPosition Position { get; set; }
        public int Line => Position.Token.line;
        public int Column => Position.Token.col;
        public int IdentifierEndColumn => Position.Token.col + Name.Length;
        public bool HasBody => Position.BodyStartToken != null && Position.BodyEndToken != null;
        #endregion

        #region Filename
        public string Name { get; set; }
        public Uri FileUri => new Uri(Position?.Token?.filename ?? "N:\\u\\l.l");
        public string FileName => Path.GetFileName(FileUri.LocalPath);
        #endregion

        #region Type-Kind
        public Kind Kind { get; set; }
        public Type Type { get; set; }
        public UserDefinedType UserTypeDefinition
        {
            get
            {
                if (Type is UserDefinedType type)
                {
                    return type;
                }
                return null;
            }
        }
        #endregion

        #region Declaration
        public ISymbolInformation DeclarationOrigin { get; set; }
        public bool IsDeclaration => ReferenceEquals(DeclarationOrigin, this);
        #endregion

        #region Parent-Children-Usages
        public ISymbolInformation Parent { get; set; }
        public Dictionary<string, ISymbolInformation> ChildrenHash { get; set; }
        public List<ISymbolInformation> Children => ChildrenHash?.Values.ToList();      //children: only declarations
        public bool HasChildren => ChildrenHash != null && ChildrenHash.Any();
        public List<ISymbolInformation> Descendants { get; set; }                        //Descendants: any symbol within my body, including simple usages.
        public List<ISymbolInformation> Usages { get; set; }
        public List<ISymbolInformation> BaseClasses { get; set; }
        public List<ISymbolInformation> Params { get; set; }
        public bool HasInheritedMembers => Kind == Kind.Class && (BaseClasses?.Any() ?? false);
        #endregion

        #region ContainingModule-And-DefaultClass
        public ISymbolInformation Module { get; set; }
        public ISymbolInformation AssociatedDefaultClass => Module?[Resources.SymbolTableStrings.default_class];
        #endregion

        #region ToString
        public string PositionToFormattedString()
        {
            return $"line {Line}, {FileName}";
        }

        public string ToDebugString()
        {
            return $"[L{Line}:C{Column}] \"{Name}\" | P : [L{Parent?.Line}]{Parent?.Name} | D : {(IsDeclaration ? "self" : "[L" + DeclarationOrigin?.Line + "]" + DeclarationOrigin?.Name)} | C : {Children?.Count} | U : {Usages?.Count}";
        }

        public override string ToString()
        {
            return $"{Name} at Line {Line} in {FileName}";
        }
        #endregion

        #region Indexer
        public ISymbolInformation this[string index]
        {
            get
            {
                ChildrenHash.TryGetValue(index, out var value);
                return value;
            }
            set => ChildrenHash.Add(index, value);
        }
        #endregion

        #region EqualsHash
        public override bool Equals(object obj)
        {
            if (obj is SymbolInformation symbol)
            {
                return symbol.Name == Name
                       && symbol.FileUri == FileUri
                       && symbol.Line == Line
                       && symbol.Column == Column;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 7 + FileUri.GetHashCode();
            hash = hash * 7 + Line.GetHashCode();
            return hash * 7 + Column.GetHashCode();
        }
        #endregion
    }

    public enum Kind
    {
        RootNode,
        Module,
        Class,
        Method,
        Constructor,
        Function,
        Field,
        Variable,
        Undefined,
        BlockScope
    }

    public class TokenPosition
    {
        public IToken BodyStartToken { get; set; }
        public IToken BodyEndToken { get; set; }
        public IToken Token { get; set; }
    }
}
