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
            var matches = scope.Children.Where(s =>
                s.Name == target &&
                s.IsDeclaration &&
                (type == null || s.Kind == type))
                .ToList();

            if (matches.Count() == 1)
            {
                return matches.Single();
            }

            if (matches.Count() > 1)
            {
                //simplified for now:
                //throw new InvalidOperationException("multiple candidates for symbol declaration");
                return new SymbolInformation()
                {
                    Name = "*ERROR - MULTIPLE DECLARATION CANDIDATES FOUND*" // todo lang file #102
                };
            }

            //if symbol not found in current scope, search parent scope, except if we are searching default scope.
            if (scope.Parent != null && goRecursive)
            {
                return FindDeclaration(target, scope.Parent, type);
            }

            //if symbol was not found and there are no more parents, it yet may be in global scope
            //this makes stack overlfows -> default class -> default module -> default class -> default module etc, so we got this boolean "goRecursive" to avoid that.
            if (scope != GlobalScope)
            {
                return FindDeclaration(target, GlobalScope, type, false); //may revisit (visit... get it...) this when we acgtually work with module includes. anyway a bit weirdo.
            }
            else
            {
                //This case is then Method Failure.

                //damit es nicht immer crashed erstmal soft-mässiges handling here:
                return new SymbolInformation()
                {
                    Name = "*ERROR - DECLARATION SYMBOL NOT FOUND*", // todo lang file #102
                    Children = new List<SymbolInformation>(),
                    Usages = new List<SymbolInformation>()
                };
                //throw new ArgumentOutOfRangeException("Symbol Declaration not found");
            }
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
        /// <param name="positionAsToken"></param>
        /// <param name="bodyStartPosAsToken"></param>
        /// <param name="bodyEndPosAsToken"></param>
        /// <param name="isDeclaration">Default: true</param>
        /// <param name="declarationSymbol">Default: null</param>
        /// <param name="addUsageAtDeclaration">Default: false</param>
        /// <param name="canHaveChildren">Default: true</param>
        /// <param name="setAsChildInParent">Default: true</param>
        /// <param name="canBeUsed">Default: true</param>
        /// <param name="addToSymbolTable">Default: true</param>
        /// <returns>Returns a SymbolInformation about the specific token.</returns>
        protected SymbolInformation CreateSymbol(
            string name,
            IToken positionAsToken,

            Kind? kind = Kind.Undefined,
            Type type = null,

            IToken bodyStartPosAsToken = null,
            IToken bodyEndPosAsToken = null,

            bool isDeclaration = true,
            SymbolInformation declarationSymbol = null,
            bool addUsageAtDeclaration = false,
            bool canHaveChildren = true,
            bool setAsChildInParent = true,
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
                result.Type = null;
            }

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
                result.Children = new List<SymbolInformation>();
            }

            if (isDeclaration && SurroundingScope != null) //add child unless we are on toplevel scope.
            {
                SurroundingScope.Children.Add(result);
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
        protected void JumpUpInScope() => SurroundingScope = SurroundingScope.Parent;
        protected void SetModule(SymbolInformation symbol) => CurrentModule = symbol;
        protected void SetClass(SymbolInformation symbol) => CurrentClass = symbol;

        public override void Visit(IAstElement o) { }

        public override void Leave(IAstElement o) { }
    }
}
