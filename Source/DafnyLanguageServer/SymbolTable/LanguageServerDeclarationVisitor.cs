﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Serilog.Sinks.File;
using Function = Microsoft.Dafny.Function;
using IdentifierExpr = Microsoft.Dafny.IdentifierExpr;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;
using LocalVariable = Microsoft.Dafny.LocalVariable;
using Type = Microsoft.Dafny.Type;
using Visitor = Microsoft.Dafny.Visitor;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This Visitor creates a symbol table visiting only declarations.
    /// This includes methods, fields classes and modules. 
    /// </summary>
    public class LanguageServerDeclarationVisitor : LanguageServerVisitorBase
    {
        public LanguageServerDeclarationVisitor()
        {
            GoesDeep = false;
        }

        public override void Visit(ModuleDefinition o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                kind: Kind.Module,
                type: null,

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
        }

        public override void Visit(ClassDecl o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                kind: Kind.Class,

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
            if (symbol.Name == DEFAULT_CLASS_NAME)
            {
                SetDefaultClass(symbol);
            }
        }

        public override void Leave(ClassDecl o)
        {
            JumpUpInScope();
            SetClass(null);
        }


        public override void Visit(Field o)
        {
            UserDefinedType userType = null;
            if (o.Type != null && o.Type is UserDefinedType)
            {
                userType = o.Type as UserDefinedType;
            }

            var symbol = CreateSymbol(
                name: o.Name,
                kind: Kind.Field,
                type: o.Type,
                typeDefinition: userType,

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
                kind: Kind.Method,

                positionAsToken: o.tok,
                bodyStartPosAsToken: o.BodyStartTok,
                bodyEndPosAsToken: o.BodyEndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: true
            );
            SetScope(symbol); //technically not necessary since we dont go deeper.
        }

        public override void Leave(Method o)
        {
            JumpUpInScope();
        }

        public override void Visit(Constructor o)
        {
            var symbol = CreateSymbol(
              name: o.Name,
              kind: Kind.Constructor,

              positionAsToken: o.tok,
              bodyStartPosAsToken: o.BodyStartTok,
              bodyEndPosAsToken: o.BodyEndTok,

              isDeclaration: true,
              declarationSymbol: null,
              addUsageAtDeclaration: false,

              canHaveChildren: true,
              canBeUsed: true
            );
            SetScope(symbol); //technically not necessary since we dont go deeper.
        }

        public override void Leave(Constructor o)
        {
            JumpUpInScope();
        }

        public override void Visit(Function o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                kind: Kind.Function,
                type: o.ResultType,

                positionAsToken: o.tok,
                bodyStartPosAsToken: o.BodyStartTok,
                bodyEndPosAsToken: o.BodyEndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: true,

                addToSymbolTable: true
            );
            SetScope(symbol); //technically not necessary since we dont go deeper.
        }

        public override void Leave(Function o)
        {
            JumpUpInScope();
        }

        public override void Visit(ExprDotName e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(ExprDotName e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(ThisExpr e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(ThisExpr e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(NameSegment e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(NonglobalVariable o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(NonglobalVariable o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(LocalVariable o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(LocalVariable o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(BlockStmt o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(BlockStmt o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(WhileStmt s)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(WhileStmt e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(IfStmt e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(IfStmt e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(AssignmentRhs o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(AssignmentRhs o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(TypeRhs e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(TypeRhs e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(Expression o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(Expression o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(AutoGhostIdentifierExpr e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(AutoGhostIdentifierExpr e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(LiteralExpr e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(LiteralExpr e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(ApplySuffix e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(ApplySuffix e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(NameSegment e)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Leave(IdentifierExpr o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }

        public override void Visit(IdentifierExpr o)
        {
            throw new NotImplementedException(Resources.ExceptionMessages.visit_only_declarations);
        }
    }
}
