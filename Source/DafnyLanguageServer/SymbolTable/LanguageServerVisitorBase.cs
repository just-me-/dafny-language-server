using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Serilog.Sinks.File;
using IdentifierExpr = Microsoft.Dafny.IdentifierExpr;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;
using LocalVariable = Microsoft.Dafny.LocalVariable;
using Type = Microsoft.Dafny.Type;
using Visitor = Microsoft.Dafny.Visitor;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This Visitor is used to generate the Symbol Table for Dafny code.
    /// It is an extension of the Microsoft Dafny Visitor Class.
    /// </summary>
    public abstract class LanguageServerVisitorBase : Visitor
    {
        public List<SymbolInformation> SymbolTable { get; set; } = new List<SymbolInformation>();
        public SymbolInformation SurroundingScope { get; set; }
        public SymbolInformation CurrentModule { get; set; }
        public SymbolInformation CurrentClass { get; set; }

        private SymbolInformation _globalScope;

        public SymbolInformation GlobalScope
        {
            get
            {
                if (_globalScope != null)
                {
                    return _globalScope;
                }

                foreach (var symbol in SymbolTable)
                {
                    if (symbol.Name == DEFAULT_CLASS_NAME && symbol.Kind == Kind.Class)
                    {
                        _globalScope = symbol;
                        return symbol; // todo this is basicly return _globalScope; ... if verinheitlichbar? #104
                    }
                }
                throw new InvalidOperationException(Resources.ExceptionMessages.global_class_not_registered);
            }
        }

        protected SymbolInformation FindDeclaration(string target, SymbolInformation scope, Kind? type = null, bool goRecursive = true)
        {
            var navigator = new SymbolTableNavigator();
            Predicate<SymbolInformation> filter = s =>
                s.Name == target &&
                s.IsDeclaration &&
                (type == null || s.Kind == type);
            return navigator.BottomUpFirst(scope, filter) ?? new SymbolInformation()
            {
                Name = "*ERROR - DECLARATION SYMBOL NOT FOUND*", // todo lang file #102
                ChildrenHash = new Dictionary<string, SymbolInformation>(),
                Usages = new List<SymbolInformation>()
            };
        }

        protected SymbolInformation FindDeclaration(SymbolInformation target, SymbolInformation scope)  //evtl bei leave iwie
        {
            return FindDeclaration(target.Name, scope);
        }

        /// <summary>
        ///  This is a Factory Method. Default values are set.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="kind">Variable, Method, Module, ...</param>
        /// <param name="type">int, bool, Person, Vehicle, ...</param>
        /// <param name="typeDefinition">UserDefinedType; entry point for ResolvedClass for class instances.</param>
        /// <param name="positionAsToken"></param>
        /// <param name="bodyStartPosAsToken"></param>
        /// <param name="bodyEndPosAsToken"></param>
        /// <param name="isDeclaration">Default: true</param>
        /// <param name="declarationSymbol">Default: null</param>
        /// <param name="addUsageAtDeclaration">Default: false</param>
        /// <param name="canHaveChildren">Default: true</param>
        /// <param name="canBeUsed">Default: true</param>
        /// <param name="addToSymbolTable">Default: true</param>
        /// <returns>Returns a SymbolInformation about the specific token.</returns>
        protected SymbolInformation CreateSymbol(
            string name,
            IToken positionAsToken,

            Kind? kind = Kind.Undefined,
            Type type = null,
            UserDefinedType typeDefinition = null,

            IToken bodyStartPosAsToken = null,
            IToken bodyEndPosAsToken = null,

            bool isDeclaration = true,
            SymbolInformation declarationSymbol = null,
            bool addUsageAtDeclaration = false,
            bool canHaveChildren = true,
            bool canBeUsed = true,
            bool addToSymbolTable = true
            )
        {
            SymbolInformation result = new SymbolInformation();
            result.Name = name;
            result.Parent = SurroundingScope; //is null for modules

            if (kind != null)
            {
                result.Kind = (Kind)kind;
            }
            else if (declarationSymbol != null)
            {
                result.Kind = declarationSymbol.Kind;
            }
            else
            {
                result.Kind = Kind.Undefined;
            }

            if (type != null)
            {
                result.Type = type;
            }
            else if (declarationSymbol != null)
            {
                result.Type = declarationSymbol.Type;
            }
            else
            {
                result.Type = null; // todo diese null checks sind nicht nötig wegen defaults? #120
            }

            result.UserTypeDefinition = typeDefinition;

            result.Position = new TokenPosition()
            {
                Token = positionAsToken,
                BodyStartToken = bodyStartPosAsToken ?? positionAsToken,
                BodyEndToken = bodyEndPosAsToken ?? positionAsToken
            };

            if (canBeUsed)
            {
                result.Usages = new List<SymbolInformation>();
            }

            if (isDeclaration)
            {
                result.DeclarationOrigin = result;
            }
            else if (declarationSymbol != null)
            {
                result.DeclarationOrigin = declarationSymbol;
            }
            else
            {
                //null? self?
            }

            if (addUsageAtDeclaration)
            {
                declarationSymbol.Usages.Add(result);
            }

            if (canHaveChildren)
            {
                result.ChildrenHash = new Dictionary<string, SymbolInformation>();
                result.Descendants = new List<SymbolInformation>();
            }

            if (isDeclaration && SurroundingScope != null) //add child unless we are on toplevel scope.
            {
                SurroundingScope.ChildrenHash.Add(result.Name, result);
            }
            if (SurroundingScope != null) //add child unless we are on toplevel scope.
            {
                SurroundingScope.Descendants.Add(result);
            }

            if (addToSymbolTable)
            {
                Add(result);
            }

            return result;
        }

        protected static void PerformArgChecks(bool isDeclaration, SymbolInformation declarationSymbol,
            bool addUsageAtDeclaration)
        {
            if (!isDeclaration && declarationSymbol == null)
            {
                throw new ArgumentNullException(nameof(declarationSymbol),
                    Resources.ExceptionMessages.missing_delcaration_origin);
            }

            if (isDeclaration && addUsageAtDeclaration)
            {
                throw new ArgumentException(Resources.ExceptionMessages.cannot_use_itself);
            }

            if (addUsageAtDeclaration && declarationSymbol == null)
            {
                throw new ArgumentException(Resources.ExceptionMessages.cannot_use_unknown_symbol);
            }
        }

        protected void Add(SymbolInformation symbol) => SymbolTable.Add(symbol);
        protected void SetScope(SymbolInformation symbol) => SurroundingScope = symbol;
        protected void JumpUpInScope() => SurroundingScope = SurroundingScope.Parent; // besseres naming todo
        protected void SetModule(SymbolInformation symbol) => CurrentModule = symbol;
        protected void SetClass(SymbolInformation symbol) => CurrentClass = symbol;

        public override void Visit(IAstElement o) { }

        public override void Leave(IAstElement o) { }
    }
}
