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
    /// <summary>
    /// This Visitor is used to generate the Symbol Table for Dafny code.
    /// It is an extension of the Microsoft Dafny Visitor Class.
    /// </summary>
    public class SymbolTableVisitorEverythingButDeclarations : LanguageServerVisitorBase
    {


        public override void Visit(ModuleDefinition o)
        {
            //jsut set scope and stuff, but don't create new symbol.
            //symbol shouild be the first symbol in table.

            var preDeclaredSymbol = SymbolTable.First(); //todo revisit this after modulesa re impelemented... steht der wirklich immer zuoberst? darf ich das einfach so machen dann?
            if (preDeclaredSymbol.Name != o.Name || preDeclaredSymbol.Type != Type.Module ||      
                !preDeclaredSymbol.IsDeclaration)
            {
                throw new InvalidOperationException("frist symbol in table is not module.");
            }
            SetScope(preDeclaredSymbol);
            SetModule(preDeclaredSymbol);
        }

        #region navigate-through-declarations

        public override void Leave(ModuleDefinition o)
        {
            SetScope(null);
            SetModule(null);
        }

        public override void Visit(ClassDecl o)
        {
            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Type.Class);

            SetScope(preDeclaredSymbol);
            SetClass(preDeclaredSymbol);

        }

        public override void Leave(ClassDecl o)
        {
            JumpUpInScope();
            SetClass(null);
        }

        public override void Visit(Field o)
        {
            //do nothing, no need to set scope or such for fields
        }

        public override void Leave(Field o)
        {
        }

        public override void Visit(Method o)
        {
            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Type.Method);

            SetScope(preDeclaredSymbol);

        }

        public override void Leave(Method o)
        {
            JumpUpInScope();
        }


#endregion

        ///////"new" stuff from here on the other visitor didnt do from here on.


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
                canBeUsed: true
            );
        }

        public override void Leave(LocalVariable o) {
        }

    public override void Visit(BlockStmt o)
    {
        var name = "block-stmt-ghost";

        var symbol = CreateSymbol(
            name: name,
            type: Type.BlockScope,

            positionAsToken: o.Tok,
            bodyStartPosAsToken: o.Tok,
            bodyEndPosAsToken: o.EndTok,

            isDeclaration: false,
            declarationSymbol: null,
            addUsageAtDeclaration: false,

            canHaveChildren: true,
            canBeUsed: false,
            addToSymbolTable: false
        );

            SetScope(symbol);
        }

        public override void Leave(BlockStmt o)
        {
            JumpUpInScope();
        }

        public override void Visit(WhileStmt o)
        {
            var name = "while-stmt-ghost";

            var symbol = CreateSymbol(
                name: name,
                type: Type.BlockScope,

                positionAsToken: o.Tok,
                bodyStartPosAsToken: o.Tok,
                bodyEndPosAsToken: o.EndTok,

                isDeclaration: false,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false,
                addToSymbolTable: false
            );
            SetScope(symbol);
        }

        public override void Leave(WhileStmt e)
        {
            JumpUpInScope();
        }

        public override void Visit(IfStmt o)
        {
            var name = "if-stmt-ghost";

            var symbol = CreateSymbol(
                name: name,
                type: Type.BlockScope,

                positionAsToken: o.Tok,
                bodyStartPosAsToken: o.Tok,
                bodyEndPosAsToken: o.EndTok,

                isDeclaration: false,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false,
                addToSymbolTable: false
            );
            SetScope(symbol);
        }

        public override void Leave(IfStmt e)
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
    }
}
