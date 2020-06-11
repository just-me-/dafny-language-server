using Microsoft.Dafny;
using System;
using Function = Microsoft.Dafny.Function;
using IdentifierExpr = Microsoft.Dafny.IdentifierExpr;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;
using LocalVariable = Microsoft.Dafny.LocalVariable;

// ReSharper disable RedundantArgumentDefaultValue
// Parameters explicitly stated for clarity.

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This Visitor is used to generate the Symbol Table for Dafny code.
    /// It will generate symbols for all declarations, but will not take actions for code bodies.
    /// </summary>
    public class LanguageServerDeclarationVisitor : LanguageServerVisitorBase
    {
        public LanguageServerDeclarationVisitor(ISymbolInformation entryPoint) : base(entryPoint)
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
                canBeUsed: true
            );
            symbol.Module = symbol;
            SetScope(symbol);
            SetModule(symbol);
        }

        public override void Leave(ModuleDefinition o)
        {
            JumpUpInScope();
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
        }

        public override void Leave(ClassDecl o)
        {
            JumpUpInScope();
            SetClass(null);
        }

        public override void Visit(Field o)
        {
            CreateSymbol(
                name: o.Name,
                kind: Kind.Field,
                type: o.Type,

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

        public override void Leave(Field o)
        {
        }

        public override void Visit(Method o)
        {
            var hasABody = o.BodyEndTok.line != 0 && o.BodyEndTok.col != 0;

            var symbol = CreateSymbol(
                name: o.Name,
                kind: Kind.Method,

                positionAsToken: o.tok,
                bodyStartPosAsToken: hasABody ? o.BodyStartTok : null,
                bodyEndPosAsToken: hasABody ? o.BodyEndTok : null,

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
            SetScope(symbol);
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
                canBeUsed: true
            );
            SetScope(symbol);
        }

        public override void Leave(Function o)
        {
            JumpUpInScope();
        }
    }
}