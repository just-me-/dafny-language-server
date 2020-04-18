using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Serilog.Sinks.File;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;
using LocalVariable = Microsoft.Dafny.LocalVariable;
using Visitor = Microsoft.Dafny.Visitor;

namespace DafnyLanguageServer.SymbolTable
{
    public class VisitorThatGeneratesSymbolTable : Visitor
    {
        public List<SymbolInformation> SymbolTable { get; set; } = new List<SymbolInformation>();
        public SymbolInformation SurroundingScope { get; set; }
        public SymbolInformation CurrentModule { get; set; }
        public SymbolInformation CurrentClass { get; set; }



        public override void Visit(IAstElement o) { }

        public override void Leave(IAstElement o) { }

        public override void Visit(ModuleDefinition o)
        {

            var symbol = CreateSymbol(
                name: o.Name,
                type: Type.Module,
                positionAsToken: o.tok,
                bodyStartPosAsToken: o.BodyStartTok,
                bodyEndPosAsToken: o.BodyEndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false
            );

            SetScope(symbol);
            SetModule(symbol);

        }




        public override void Leave(ModuleDefinition o)
        {
            SetScope(null);
            SetModule(null);
        }

        public override void Visit(ClassDecl o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                type: Type.Class,

                positionAsToken: o.tok,
                bodyStartPosAsToken: o.BodyStartTok,
                bodyEndPosAsToken: o.BodyEndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: true
            );

            SetClass(symbol);
            SetScope(symbol);

            //todo evtl heir alle member kurz definieren, weil eine decl auch nach der nutzung kommen kann.
            //dafür dann unten bei field, method defintiion etc muss man das dann net mehr machen.
            //lassen wir aber erstmal. erstmal das einfache.

            //vlt besser sogar: wenn decl net gefunden wird, iwie auf nen stack legen und am ende nochmal kucken oder so. ka.
        }

        public override void Leave(ClassDecl o)
        {
            JumpUpInScope();
            SetClass(null);
        }

        public override void Visit(Field o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                type: Type.Field,

                positionAsToken: o.tok,
                bodyStartPosAsToken: o.BodyStartTok,
                bodyEndPosAsToken: o.BodyEndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: false,
                canBeUsed: true
            );
        }

        public override void Leave(Field o)
        {
        }

        public override void Visit(Method o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                type: Type.Method,

                positionAsToken: o.tok,
                bodyStartPosAsToken: o.BodyStartTok,
                bodyEndPosAsToken: o.BodyEndTok,


                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: true
            );

            SetScope(symbol);
        }

        public override void Leave(Method o)
        {
            JumpUpInScope();
        }


        /// <summary>
        /// Nonglobal Variables are Method Parameters (in and out parameters).
        /// We treat them as variable definitions.
        /// Additional Info: Base Class of NonglobalVariable is Formal.
        /// </summary>
        public override void Visit(NonglobalVariable o)
        {
            var symbol = CreateSymbol(
                    name: o.Name,
                    type: Type.Variable,

                    positionAsToken: o.tok,
                    bodyStartPosAsToken: null,
                    bodyEndPosAsToken: null,

                    isDeclaration: true,
                    declarationSymbol: null,
                    addUsageAtDeclaration: false,

                    canHaveChildren: false,
                    canBeUsed: true
                    );
        }

        public override void Leave(NonglobalVariable o)
        { 
        }

        //local variable are just locally defined vars: var bla:=2
        public override void Visit(LocalVariable o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                type: Type.Variable,

                positionAsToken: o.Tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: false,
                canBeUsed: true
            );

        }

        public override void Leave(LocalVariable o) {
        }

    public override void Visit(BlockStmt o)
        {
            //manual creation to bypass creation logic
            //it is just here to mark the scope.
            //blockstmt is neither a declaration, nor does it have a declaring symbol,
            //and it shall not be a child of the parent symbol.
            //It's ghosty.

            var symbol = new SymbolInformation()
            {
                Name = "ghost-block-scope",
                Type = Type.BlockStmt,
                Parent = SurroundingScope,

                DeclarationOrigin = null,
                Usages = null,

                Children = new List<SymbolInformation>(),

                Position = new TokenPosition()
                {
                    Token = o.Tok,
                    BodyStartToken = o.Tok,
                    BodyEndToken = o.EndTok
                },
            };

            SetScope(symbol);
        }

        public override void Leave(BlockStmt o)
        {
            JumpUpInScope();
        }

        //A Type RHS is the right hand side of omsething like var a:= new MyClass(). See also its class description.
        //Also has some Array stuff that could be relevant for us.
        public override void Visit(TypeRhs e)
        {
            UserDefinedType t = null;
            if (e.Type is UserDefinedType type)   //can it be anything else?
                t = type;

            var declaration = FindDeclaration(t.Name, SurroundingScope);

            var symbol = CreateSymbol(
                name: t.Name,
                type: Type.Class,

                positionAsToken: t.tok,
                bodyStartPosAsToken: e.Tok,  //"new"
                bodyEndPosAsToken: t.tok,    //ClassName

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: true,

                canHaveChildren: false,
                canBeUsed: false

            );
        } 

        public override void Leave(TypeRhs e) { }

        //A AutoGhostIndentifierExpr is when we have a VarDeclStmt: var a:= b.
        //This VarDecl Stmt contains a Update stmt, and the left side contains ghostVars, aka the 'a'.
        //We do nth since within the VarDeclStmt, the 'a' gets registered.
        public override void Visit(AutoGhostIdentifierExpr e) { } //do nth since handled in localVar
        public override void Leave(AutoGhostIdentifierExpr e) { }

        public override void Visit(LiteralExpr e) { } //do nth
        public override void Leave(LiteralExpr e) { }

        //ApllySuffixes are just brackets after a Method call.
        //The Visitor will redirect the accept statements to the expressions lef to the (), thus we do nth.
        public override void Visit(ApplySuffix e) { } 
        public override void Leave(ApplySuffix e) { }

        //Name Segment are identifiers, especially also in methods.
        //For example two name segments in   var1 := returnsTwo(); --> var1, returnsTwo
        public override void Visit(NameSegment e)
        {
            var declaration = FindDeclaration(e.Name, SurroundingScope);
            
            var symbol = CreateSymbol(
                name: e.Name,
                type: null,

                positionAsToken: e.tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: true,

                canHaveChildren: false,
                canBeUsed: false

            );

        }

        public override void Leave(NameSegment e)
        {
        }

        public override void Visit(ExprDotName e)
        {
            string definingClassName = e.Lhs.Type.ToString();
            var definingClass = FindDeclaration(definingClassName, SurroundingScope, Type.Class);
            var declaration = FindDeclaration(e.SuffixName, definingClass);

            var symbol = CreateSymbol(
                name: e.SuffixName,
                type: null,

                positionAsToken: e.tok, //nimmt den ganze, net nur den suffix.
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: true,

                canHaveChildren: false,
                canBeUsed: false

            );
        }

        public override void Leave(ExprDotName e)
        {
        }

        public override void Visit(ThisExpr e)
        {
            //we could also just do nothing, then you couldn't click on "this." for goto def or so.
            //maybe even better cause many this names may cause trouble or such, i don't know.

            string definingClassName = e.Type.ToString();
            var definingClass = FindDeclaration(definingClassName, SurroundingScope, Type.Class);
            var declaration = definingClass;

            var symbol = CreateSymbol(
                name: "this",
                type: Type.Class,

                positionAsToken: e.tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: false,    //fänd ich komisch. jedes mal "this" = zusätzliche referenz? oder?

                canHaveChildren: false,
                canBeUsed: false

            );
        }

        public override void Leave(ThisExpr e)
        {
        }

        public override void Visit(Expression o)
        {

            var declaration = FindDeclaration(o.tok.val, SurroundingScope);

            var symbol = CreateSymbol(
                name: o.tok.val + "** General Expression Visit Used!! **",
                type: null,

                positionAsToken: o.tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: true,

                canHaveChildren: false,
                canBeUsed: false

            );

        }

        public override void Leave(Expression o)
        {
        }

        public override void Visit(AssignmentRhs o)
        {
            var declaration = FindDeclaration(o.Tok.val, SurroundingScope);

            var symbol = CreateSymbol(
                name: o.Tok.val,
                type: Type.Variable,

                positionAsToken: o.Tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: true,

                canHaveChildren: false,
                canBeUsed: false
            );

        }


        public override void Leave(AssignmentRhs o)
        {
        }


        private SymbolInformation FindDeclaration(string target, SymbolInformation scope, Type? type = null)
        {
            var matches = scope.Children.Where(s =>
                s.Name == target &&
                s.IsDeclaration &&
                (type == null || s.Type == type))
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
                    Name = "*ERROR - MULTIPLE DECLARATION CANDIDATES FOUND*"
                };
            }

            //if symbol not found in current scope, search parent scope
            if (scope.Parent != null)
            {
                return FindDeclaration(target, scope.Parent, type);
            }
            else
            {
                //damit es nicht immer crashed erstmal soft-mässiges handling here:
                //throw new ArgumentOutOfRangeException("Symbol Declaration not found");
                return new SymbolInformation()
                {
                    Name = "*ERROR - DECLARATION SYMBOL NOT FOUND*"
                };
            }
        }


        private SymbolInformation CreateSymbol(
            string name,
            Type? type,

            IToken positionAsToken,
            IToken bodyStartPosAsToken,
            IToken bodyEndPosAsToken,

            bool isDeclaration,
            SymbolInformation declarationSymbol,
            bool addUsageAtDeclaration,

            bool canHaveChildren,
            bool canBeUsed,

            bool addToSymbolTable = true
            )
        {
            SymbolInformation result = new SymbolInformation();
            result.Name = name;
            result.Parent = SurroundingScope; //is null for modules

            if (type != null)
            {
                result.Type = (Type) type;
            }
            else if (declarationSymbol != null)
            {
                result.Type = declarationSymbol.Type;
            } else
            {
                result.Type = Type.Undefined;
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

            PerformArgChecks(isDeclaration, declarationSymbol, addUsageAtDeclaration);
            if (isDeclaration)
            {
                result.DeclarationOrigin = result;
            }
            else
            {
                result.DeclarationOrigin = declarationSymbol;
            }
            if (addUsageAtDeclaration) //entspräche !isdecl, ausser bei "this"
            {
                declarationSymbol.Usages.Add(result);
            }

            if (canHaveChildren)
            {
                result.Children = new List<SymbolInformation>();
            }

            if (isDeclaration && type != Type.Module) //all decls, except for top level modules, are a child
            {
                SurroundingScope.Children.Add(result);
            }

            if (addToSymbolTable)
            {
                Add(result);
            }

            return result;
        }

        private static void PerformArgChecks(bool isDeclaration, SymbolInformation declarationSymbol,
            bool addUsageAtDeclaration)
        {
            if (!isDeclaration && declarationSymbol == null)
            {
                throw new ArgumentNullException(nameof(declarationSymbol),
                    "when symbol is not a declaration, its declarationOrigin must be given");
            }

            if (isDeclaration && addUsageAtDeclaration)
            {
                throw new ArgumentException("when symbol is a declaration, it cannot be a usage of itself.");
            }

            if (addUsageAtDeclaration && declarationSymbol == null)
            {
                throw new ArgumentException("cannot add usage at unknown symbol");

            }
        }


        private void Add(SymbolInformation symbol) => SymbolTable.Add(symbol);
        private void SetScope(SymbolInformation symbol) => SurroundingScope = symbol;
        private void JumpUpInScope() => SurroundingScope = SurroundingScope.Parent;
        private void SetModule(SymbolInformation symbol) => CurrentModule = symbol;
        private void SetClass(SymbolInformation symbol) => CurrentClass = symbol;


    }
}
