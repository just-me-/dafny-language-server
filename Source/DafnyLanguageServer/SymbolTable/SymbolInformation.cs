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
    /// This class represents any symbol in Dafny code.
    /// Every identifier occuring in the code is described with a SymbolInformation.
    /// </summary>
    /// <remarks>
    /// Position: Contains 3 Tokens. The identifier itself, and if available bodyStart "{" and bodyEnd "}"
    /// Name: Name of the symbol, e.g. myMethod. Unnamed scopes are for example called 'ghost-block-scope-13' or such.
    /// Kind: Method, Class, Variable, ...
    /// Type: int, MyClass, ...
    /// UserType: MyClass (only applicable for custom user classes).
    /// DeclarationOrigin: Symbol of the declaration. If symbol is declaration, points to 'this'.
    /// Parent: Symbol reference to the surrounding (parent) scope.
    /// ChildrenHash: All declarations occuring inside the body of this symbol. Key: string with child identifier name, Value: ISymbolInformation with childSymbol-reference.
    /// ChildrenHash is null if the symbol can't have descendants.
    /// The indexer ([]-operator) is overloaded to access the childrenhash. myClass["myMethod"] is same as myClass.ChildrenHash.Get("myMethod")
    /// Children: Same as list.
    /// Descendants: All symbols, not only declarations, also usages, occuring inside the body. Is null if symbol can't have descendants.
    /// Usages: List of symbol references, pointing to where this symbol gets used. Only available for declarations, otherwise null.
    /// BaseClasses: Only for classes that inherit from traits. Contains the trait symbols.
    /// Params: Only for methods (including ctor, function, etc): References to symbol in-parameters.
    /// Module: Contains a reference to the module this symbol is in. Could also move up to parents until a module is reached.
    /// AssociatedDefaultClass: Contains a reference to the default class of this symbol. Same as Module["_default"].
    /// </remarks>
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
