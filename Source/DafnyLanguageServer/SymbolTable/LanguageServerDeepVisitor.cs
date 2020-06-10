using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.Linq;
using Formal = Microsoft.Dafny.Formal;
using Function = Microsoft.Dafny.Function;
using IdentifierExpr = Microsoft.Dafny.IdentifierExpr;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;
using LocalVariable = Microsoft.Dafny.LocalVariable;

// ReSharper disable RedundantArgumentDefaultValue
// explicitly stated for clarity.

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This Visitor is used to generate the Symbol Table for Dafny code.
    /// It will generate symbols for any usages, but not for definitions.
    /// </summary>
    public class SymbolTableVisitorEverythingButDeclarations : LanguageServerVisitorBase
    {
        public SymbolTableVisitorEverythingButDeclarations(ISymbolInformation entryPoint) : base(entryPoint)
        {
            GoesDeep = true;
        }

        public override void Visit(ModuleDefinition o)
        {
            // Set scope but do not create new symbol.

            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Kind.Module);

            if (preDeclaredSymbol.Name != o.Name || preDeclaredSymbol.Kind != Kind.Module || !preDeclaredSymbol.IsDeclaration)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.invalid_module_handed_to_deep_visitor);
            }

            SetScope(preDeclaredSymbol);
            SetModule(preDeclaredSymbol);
        }

        public override void Leave(ModuleDefinition o)
        {
            SetScope(null);
        }

        public override void Visit(AliasModuleDecl o)
        {
            CreateSymbol(
                name: o.Name,
                positionAsToken: o.tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,
                kind: Kind.Module,
                type: null,
                isDeclaration: true,
                addUsageAtDeclaration: false,
                canHaveChildren: false,
                canBeUsed: true
            );
        }

        public override void Leave(AliasModuleDecl o)
        {
        }

        #region navigate-through-declarations

        public override void Visit(ClassDecl o)
        {
            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Kind.Class);

            if (o.TraitsTyp.Any())
            {
                preDeclaredSymbol.BaseClasses = new List<ISymbolInformation>();
                foreach (var baseClassType in o.TraitsTyp)
                {
                    var baseClassIdentifier = baseClassType as UserDefinedType;
                    ISymbolInformation baseSymbol = FindDeclaration(baseClassIdentifier?.Name, SurroundingScope);
                    preDeclaredSymbol.BaseClasses.Add(baseSymbol);

                    var t = CreateSymbol(
                      name: baseClassIdentifier?.Name,
                      positionAsToken: baseClassIdentifier?.tok,
                      bodyStartPosAsToken: null,
                      bodyEndPosAsToken: null,
                      kind: Kind.Class,
                      type: baseClassType,
                      isDeclaration: false,
                      declarationSymbol: baseSymbol,
                      addUsageAtDeclaration: true,
                      canHaveChildren: false,
                      canBeUsed: false
                    );
                    t.Parent = preDeclaredSymbol;
                }
            }

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
        }

        public override void Leave(Field o)
        {
        }

        public override void Visit(Method o)
        {
            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Kind.Method);

            SetScope(preDeclaredSymbol);
        }

        public override void Leave(Method o)
        {
            JumpUpInScope();
        }

        public override void Visit(Constructor o)
        {
            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Kind.Constructor);

            SetScope(preDeclaredSymbol);
        }

        public override void Leave(Constructor o)
        {
            JumpUpInScope();
        }

        public override void Visit(Function o)
        {
            var preDeclaredSymbol = FindDeclaration(o.Name, SurroundingScope, Kind.Function);

            SetScope(preDeclaredSymbol);
        }

        public override void Leave(Function o)
        {
            JumpUpInScope();
        }

        #endregion navigate-through-declarations

        public override void Visit(NonglobalVariable o)
        {
            CreateSymbol(
                name: o.Name,
                kind: Kind.Variable,
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

        public override void Leave(NonglobalVariable o)
        {
        }

        public override void Visit(Formal o)
        {
            var symbol = CreateSymbol(
                name: o.Name,
                kind: Kind.Variable,
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

            if (o.InParam)
            {
                SurroundingScope.Params = SurroundingScope.Params ?? new List<ISymbolInformation>();
                SurroundingScope.Params.Add(symbol);
            }
        }

        public override void Leave(Formal o)
        {
        }

        public override void Visit(LocalVariable o)
        {
            CreateSymbol(
                name: o.Name,
                kind: Kind.Variable,
                type: o.Type,

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

        public override void Leave(LocalVariable o)
        {
        }

        public override void Visit(BlockStmt o)
        {
            var name = "block-stmt-ghost-" + o.Tok.line;
            var symbol = CreateSymbol(
                name: name,
                kind: Kind.BlockScope,

                positionAsToken: o.Tok,
                bodyStartPosAsToken: o.Tok,
                bodyEndPosAsToken: o.EndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false
            );
            SetScope(symbol);
        }

        public override void Leave(BlockStmt o)
        {
            JumpUpInScope();
        }

        public override void Visit(WhileStmt s)
        {
            var name = "while-stmt-ghost-" + s.Tok.line;
            var symbol = CreateSymbol(
                name: name,
                kind: Kind.BlockScope,

                positionAsToken: s.Tok,
                bodyStartPosAsToken: s.Tok,
                bodyEndPosAsToken: s.EndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false
            );
            SetScope(symbol);
        }

        public override void Leave(WhileStmt e)
        {
            JumpUpInScope();
        }

        public override void Visit(IfStmt e)
        {
            var name = "if-stmt-ghost-" + e.Tok.line;

            var symbol = CreateSymbol(
                name: name,
                kind: Kind.BlockScope,

                positionAsToken: e.Tok,
                bodyStartPosAsToken: e.Tok,
                bodyEndPosAsToken: e.EndTok,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false
            );
            SetScope(symbol);
        }

        public override void Leave(IfStmt e)
        {
            JumpUpInScope();
        }

        public override void Visit(ComprehensionExpr o)
        {
            var name = "bounded-expression-ghost-" + o.tok.line;
            IToken endToken = new Token(o.tok.line, int.MaxValue - 1024);

            var symbol = CreateSymbol(
                name: name,
                kind: Kind.BlockScope,

                positionAsToken: o.tok,
                bodyStartPosAsToken: o.tok,
                bodyEndPosAsToken: endToken,

                isDeclaration: true,
                declarationSymbol: null,
                addUsageAtDeclaration: false,

                canHaveChildren: true,
                canBeUsed: false
            );
            SetScope(symbol);
        }

        public override void Leave(ComprehensionExpr o)
        {
            JumpUpInScope();
        }

        public override void Visit(TypeRhs e)
        {
            UserDefinedType t = null;
            if (e.Type is UserDefinedType type)
            {
                t = type;
            }

            if (t == null)
            {
                return;
            }

            var nav = new SymbolNavigator();
            var declaration = nav.GetSymbolAtPosition(RootNode, t.ResolvedClass.tok);

            CreateSymbol(
                name: t.Name,
                kind: Kind.Class,
                type: e.Type,

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

        public override void Leave(TypeRhs e)
        {
        }

        public override void Visit(AutoGhostIdentifierExpr e)
        {
        }

        public override void Leave(AutoGhostIdentifierExpr e)
        {
        }

        public override void Visit(LiteralExpr e)
        {
        }

        public override void Leave(LiteralExpr e)
        {
        }

        public override void Visit(ApplySuffix e)
        {
        }

        public override void Leave(ApplySuffix e)
        {
        }

        public override void Visit(NameSegment e)
        {
            var nav = new SymbolNavigator();
            var resolvedSymbol = nav.TopDown(RootNode, e.ResolvedExpression.tok);

            var declaration = FindDeclaration(e.Name, resolvedSymbol);

            CreateSymbol(
                name: e.Name,
                kind: null,
                type: e.Type,

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
            MemberSelectExpr mse = null;
            if (e.ResolvedExpression is MemberSelectExpr)
            {
                mse = e.ResolvedExpression as MemberSelectExpr;
            }

            var nav = new SymbolNavigator();
            var definingItem = nav.TopDown(RootNode, mse?.Member.tok);

            var declaration = FindDeclaration(e.SuffixName, definingItem);

            CreateSymbol(
                name: e.SuffixName,
                kind: null,
                type: e.Type,

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

        public override void Leave(ExprDotName e)
        {
        }

        public override void Visit(ThisExpr e)
        {
            string definingClassName = e.Type.ToString();
            var definingClass = FindDeclaration(definingClassName, SurroundingScope, Kind.Class);
            var declaration = definingClass;

            CreateSymbol(
                name: "this",
                kind: Kind.Class,

                positionAsToken: e.tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: false,

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

            CreateSymbol(
                name: o.tok.val + Resources.SymbolTableStrings.general_expression_visit_used,
                kind: null,
                type: o.Type,

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

        public override void Visit(IdentifierExpr e)
        {
        }

        public override void Leave(IdentifierExpr e)
        {
        }

        public override void Visit(DisplayExpression o)
        {
        }

        public override void Leave(DisplayExpression o)
        {
        }

        public override void Visit(AssignmentRhs o)
        {
            var declaration = FindDeclaration(o.Tok.val, SurroundingScope);

            CreateSymbol(
                name: o.Tok.val,
                kind: Kind.Variable,

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