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
    public class SymbolInformation
    {
        public TokenPosition Position { get; set; } //todo we only need main token probably.
        public int? Line => Position?.Token.line;
        public int? Column => ColumnStart;
        public int? ColumnStart => Position?.Token.col;
        public int? ColumnEnd => ColumnStart + Name.Length;
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

        public SymbolInformation Parent { get; set; }
        public SymbolInformation DeclarationOrigin { get; set; }
        //public List<SymbolInformation> Children { get; set; } // key value hash machen 
        // hash als adapter machen, user soll nix geändert haben 
        public Dictionary<string, SymbolInformation> ChildrenHash { get; set; }
        public List<SymbolInformation> Children => ChildrenHash?.Values.ToList();

        public List<SymbolInformation> Usages { get; set; }
        public List<SymbolInformation> BaseClases { get; set; }
        public bool IsDeclaration => DeclarationOrigin == this;

        public override string ToString()
        {
            return $"[L{Line}:C{Column}] \"{Name}\" | P : [L{Parent?.Line}]{Parent?.Name} | D : {(IsDeclaration ? "self" : "[L" + DeclarationOrigin?.Line + "]" + DeclarationOrigin?.Name)} | C : {Children?.Count} | U : {Usages?.Count}";
        }
    }

    public enum Kind
    {
        Module,

        Class,
        Method,
        Constructor,
        Function,
        Field,
        Variable, //besser: Local Variable?
        Call,     //was das? wird nie bentuzt
        Definition, //same?? hat ja "IsDeclaration"
        Predicate,  //wtf...das eiunfach ne function mit bool return.
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
