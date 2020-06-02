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
    public class SymbolInformation : ISymbol
    {
        #region Position-Name-Token-Uri
        public TokenPosition Position { get; set; }
        public virtual int Line => Position.Token.line;
        public virtual int? BodyLineStart => Position.BodyStartToken?.line;
        public virtual int? BodyLineEnd => Position.BodyEndToken?.line;
        public bool HasBody => BodyLineStart != null && BodyLineEnd != null;
        public virtual int Column => Position.Token.col;
        public virtual int ColumnEnd => Column + Name.Length;
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
        public ISymbol DeclarationOrigin { get; set; }
        public bool IsDeclaration => ReferenceEquals(DeclarationOrigin, this);
        #endregion

        #region Parent-Children-Usages
        public ISymbol Parent { get; set; }
        public Dictionary<string, ISymbol> ChildrenHash { get; set; }
        public List<ISymbol> Children => ChildrenHash?.Values.ToList();      //children: only declarations
        public bool HasChildren => ChildrenHash != null && ChildrenHash.Any();
        public List<ISymbol> Descendants { get; set; }                        //Descendants: any symbol within my body, including simple usages.
        public List<ISymbol> Usages { get; set; }
        public List<ISymbol> BaseClasses { get; set; }
        public bool HasInheritedMembers => Kind == Kind.Class && (BaseClasses?.Any() ?? false);
        #endregion

        #region ContainingModule-And-DefaultClass
        public ISymbol Module { get; set; }
        public ISymbol AssociatedDefaultClass => Module?[Resources.SymbolTableStrings.default_class];
        #endregion


        #region ToString
        public string ToNiceString()
        {
            return $"{Name} at Line {Line} in {FileName}";
        }

        public string ToDebugString()
        {
            return $"[L{Line}:C{Column}] \"{Name}\" | P : [L{Parent?.Line}]{Parent?.Name} | D : {(IsDeclaration ? "self" : "[L" + DeclarationOrigin?.Line + "]" + DeclarationOrigin?.Name)} | C : {Children?.Count} | U : {Usages?.Count}";
        }

        public override string ToString()
        {
            return ToNiceString();
        }
        #endregion

        #region Indexer
        public ISymbol this[string index]
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
                       && symbol.Line == Line
                       && symbol.Column == Column
                       && symbol.ColumnEnd == ColumnEnd;
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


        #region Methods
        public bool Wraps(ISymbol child)
        {
            return Wraps(child.FileUri, child.Line, child.Column);
        }

        /// <summary>
        /// Checks if the given position (line, character) is included in the symbols range.
        /// </summary>
        public bool Wraps(Uri file, int line, int character)
        {
            return IsSameFile(file) && HasBody && (WrapsLine(line, character) || WrapsCharOnSameLine(line, character));
        }

        private bool IsSameFile(Uri file)
        {
            return this.Kind == Kind.RootNode || this.FileUri == file;
        }


        private bool WrapsLine(int line, int character)
        {
            return (
                       (Line < line && BodyLineEnd > line)
                    || (Line == line && Column <= character)
                    || (BodyLineEnd == line && Position?.BodyEndToken?.col >= character)
                   )
                   && Line != BodyLineEnd;
        }

        private bool WrapsCharOnSameLine(int line, int character)
        {
            return BodyLineStart == BodyLineEnd
                    && BodyLineStart == line
                    && Column <= character
                    && ColumnEnd >= character;
        }

    


        /// <summary>
        /// Returns all occurrences of a symbol.
        /// That is, the declaration and all usages.
        /// Targeted for Rename-Feature.
        /// </summary>
        public IEnumerable<ISymbol> GetAllOccurrences()
        {
            var decl = DeclarationOrigin;
            yield return decl;
            foreach (var usage in decl.Usages)
            {
                yield return usage;
            }
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
        Variable, //besser: Local Variable?
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
