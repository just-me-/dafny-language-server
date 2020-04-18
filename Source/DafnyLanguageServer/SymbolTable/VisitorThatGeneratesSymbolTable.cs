﻿using System;
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
                setAsChildInParent: false,
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
                setAsChildInParent: true,
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
                setAsChildInParent: true,
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
                setAsChildInParent: true,
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
                    setAsChildInParent: true,
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
                setAsChildInParent: true,
                canBeUsed: true
            );

        }

        public override void Leave(LocalVariable o) {
        }

    public override void Visit(BlockStmt o)
        {
            //todo: hier noch vermerken dass neuer scope irgendwie... für zukunftsmarcel und zukunftstom ticket 46548
            //..> BlockStmt als eigenes special-symbol handhaben, welches dann parent sein kann.
        }

        public override void Leave(BlockStmt o)
        {
            //todo siehe oben
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
                setAsChildInParent: false,
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
                setAsChildInParent: false,
                canBeUsed: false

            );

        }

        public override void Leave(NameSegment e)
        {
        }

        public override void Visit(ExprDotName e)
        {
            string definingClassName = e.Lhs.Type.ToString();
            var definingClass = FindDeclaration(definingClassName, SurroundingScope); //robuster für später: wenn man FindClass macht wo man nur explizit nach klassen sucht.. falls ne var gleich der variable oder so.
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
                setAsChildInParent: false,
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
            var definingClass = FindDeclaration(definingClassName, SurroundingScope);  //hier auch vlt besser "FindClass"
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
                setAsChildInParent: false,
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
                setAsChildInParent: false,
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
                setAsChildInParent: false,
                canBeUsed: false
            );

        }


        public override void Leave(AssignmentRhs o)
        {
        }


        private SymbolInformation FindDeclaration(string target, SymbolInformation scope)
        {
            foreach (SymbolInformation s in scope.Children)
            {
                if (s.Name == target && s.IsDeclaration) return s;
            }

            //if symbol not found in current scope, search parent scope
            if (scope.Parent != null)
            {
                return FindDeclaration(target, scope.Parent);
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
        private SymbolInformation FindDeclaration(SymbolInformation target, SymbolInformation scope)  //evtl bei leave iwie
        {
            return FindDeclaration(target.Name, scope);
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
            bool setAsChildInParent,

            bool canBeUsed,

            bool addToSymbolTable = true
            )
        {
            SymbolInformation result = new SymbolInformation();
            result.Name = name;

            //Type
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

            //Position
            result.Position = new TokenPosition()
            {
                Token = positionAsToken,
                BodyStartToken = bodyStartPosAsToken ?? positionAsToken,
                BodyEndToken = bodyEndPosAsToken ?? positionAsToken
            };

            //Decl und Usages
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

            if (addUsageAtDeclaration) //todo entspricht eig !isDecl, oder?
            {
                declarationSymbol.Usages.Add(result);
            }

            //Parent and Children
            if (result.Type != Type.Module)
            {
                result.Parent = SurroundingScope;
            }

            if (canHaveChildren) //todo verienfachbar? hmm, glaubs doch rechts peizfisch (field net, method ja, blockstmt ja, localvar ne)
            {
                result.Children = new List<SymbolInformation>();
            }

            if (setAsChildInParent) //todo entspricht das isDeclaration?
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
