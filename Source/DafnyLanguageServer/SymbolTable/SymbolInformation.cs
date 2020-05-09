using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// There are default Tokens set though. (?) todo #102
    /// </summary>
    public class SymbolInformation : ISymbol
    {
        public TokenPosition Position { get; set; } //todo we only need main token probably.
        public Uri File => new Uri(Position?.Token?.filename ?? "N:\\u\\l.l");
        public virtual int? Line => Position?.Token.line; // Line of "Symbol" ... can this be LineStart like ColumnStart? todo 
        public virtual int? LineStart => Position?.BodyStartToken?.line; // Line that Symbol Wraps {
        public virtual int? LineEnd => Position?.BodyEndToken?.line; // Endline of Wrap }
        public virtual int? Column => ColumnStart;
        public virtual int? ColumnStart => Position?.Token.col;
        public virtual int? ColumnEnd => ColumnStart + Name.Length;
        public string Name { get; set; }

        public Kind Kind { get; set; }
        public Type Type { get; set; }

        public UserDefinedType UserTypeDefinition { get; set; }
        //ich würde hier: so machen: dann müsste man den gar nie setzen, oder das snippet auch net auslagern in ne methode.
        /*
         * get
          {
            if (Type != null && Type is UserDefinedType) {
              _userType = Type as UserDefinedType;
            }
      }
         */

        public ISymbol Parent { get; set; }
        public ISymbol DeclarationOrigin { get; set; }
        //public List<SymbolInformation> Children { get; set; } // key value hash machen 
        // hash als adapter machen, user soll nix geändert haben 
        public Dictionary<string, ISymbol> ChildrenHash { get; set; }
        public List<ISymbol> Children => ChildrenHash?.Values.ToList();      //children: nur deklarationen

        public List<ISymbol> Usages { get; set; }
        public List<ISymbol> BaseClasses { get; set; }
        public List<ISymbol> Descendants { get; set; }                     //Descendants: alles was drunter liegt
        public bool IsDeclaration => DeclarationOrigin == this;

        public ISymbol Module { get; set; }
        public ISymbol AssociatedDefaultClass => Module?["_default"];

        public override string ToString()
        {
            return $"[L{Line}:C{Column}] \"{Name}\" | P : [L{Parent?.Line}]{Parent?.Name} | D : {(IsDeclaration ? "self" : "[L" + DeclarationOrigin?.Line + "]" + DeclarationOrigin?.Name)} | C : {Children?.Count} | U : {Usages?.Count}";
        }

        public ISymbol this[string index]
        {
            get
            {
                ChildrenHash.TryGetValue(index, out var value);
                return value;
            }
            set => ChildrenHash.Add(index, value);
        }

        public override bool Equals(Object obj)
        {
            if (obj is SymbolInformation)
            {
                var symbol = (SymbolInformation)obj;
                return (symbol.Name == Name
                        && symbol.Line == Line
                        && symbol.ColumnStart == ColumnStart
                        && symbol.ColumnEnd == ColumnEnd);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Name.GetHashCode();
            hash = (hash * 7) + Line.GetHashCode();
            return (hash * 7) + ColumnStart.GetHashCode();
        }

        public bool Wraps(ISymbol child)
        {
            if (child is SymbolInformation childSymbol)
            {
                return childSymbol != null && this.Wraps(child.File, (int)childSymbol.Line, (int)childSymbol.Column);
            }
            return false;
        }

        /// <summary>
        /// Checks if the given position (line, character) is included in the symbols range. 
        /// </summary>
        public bool Wraps(Uri file, int line, int character)
        {
            return IsSameFile(file) && HasLine() && (WrapsLine(line) || WrapsCharOnSameLine(line, character) || WrapsAsClassField(line, character));
        }

        private bool IsSameFile(Uri file)
        {
            return this.Kind == Kind.RootNode || this.File == file;
        }

        private bool HasLine()
        {
            return (Line != null && LineEnd != null);
        }

        private bool WrapsLine(int line)
        {
            return (Line <= line
                    && LineEnd >= line
                    && Line != LineEnd);
        }

        private bool WrapsCharOnSameLine(int line, int character)
        {
            return (LineStart == LineEnd
                    && LineStart == line
                    && ColumnStart <= character
                    && ColumnEnd >= character);
        }

        private bool WrapsAsClassField(int line, int character)
        {
            // Class fields do not have LineStart/LineEnd. This fields are a special case.
            return (Kind == Kind.Field && LineEnd == 0
                    && Line == line
                    && ColumnStart <= character
                    && ColumnEnd >= character);
        }
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
        BlockScope,
    }

    public class TokenPosition
    {
        public IToken BodyStartToken { get; set; }
        public IToken BodyEndToken { get; set; }
        public IToken Token { get; set; }
    }
}
