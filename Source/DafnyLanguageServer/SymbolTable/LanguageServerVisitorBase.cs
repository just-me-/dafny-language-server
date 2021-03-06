﻿using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using Formal = Microsoft.Dafny.Formal;
using Function = Microsoft.Dafny.Function;
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
        protected LanguageServerVisitorBase(ISymbolInformation entryPoint)
        {
            SetScope(entryPoint);
            var root = entryPoint;
            while (root.Kind != Kind.RootNode)
            {
                root = root.Parent;
            }
            RootNode = root;
        }

        public ISymbolInformation RootNode { get; set; }
        public ISymbolInformation SurroundingScope { get; set; }
        public ISymbolInformation Module { get; set; }
        public ISymbolInformation CurrentClass { get; set; }

        public ISymbolInformation DefaultClass => Module.ChildrenHash.ContainsKey(DEFAULT_CLASS_NAME) ? Module[DEFAULT_CLASS_NAME] : null;
        
        public ISymbolInformation DefaultModule => RootNode.ChildrenHash.ContainsKey(DEFAULT_MODULE_NAME) ? RootNode[DEFAULT_MODULE_NAME] : null;

        protected ISymbolInformation FindDeclaration(string target, ISymbolInformation scope, Kind? kind = null)
        {
            ISymbolNavigator navigator = new SymbolNavigator();
            bool filter(ISymbolInformation s) => s.Name == target && s.IsDeclaration && (kind == null || s.Kind == kind);
            return navigator.BottomUpFirst(scope, filter) ?? new SymbolInformation
            {
                Name = Resources.SymbolTableStrings.declaration_not_found,
                Position = new TokenPosition()
                {
                    Token = new Token(0, 0)
                },
                ChildrenHash = new Dictionary<string, ISymbolInformation>(),
                Usages = new List<ISymbolInformation>()
            };
        }

        /// <summary>
        /// This method creates symbols.
        /// Provide the arguments carefully.
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
        /// <param name="canBeUsed">Default: true</param>
        /// <returns>Returns a SymbolInformation about the specific token.</returns>
        protected ISymbolInformation CreateSymbol(
            string name,
            IToken positionAsToken,

            Kind? kind = Kind.Undefined,
            Type type = null,

            IToken bodyStartPosAsToken = null,
            IToken bodyEndPosAsToken = null,

            bool isDeclaration = true,
            ISymbolInformation declarationSymbol = null,
            bool addUsageAtDeclaration = false,
            bool canHaveChildren = true,
            bool canBeUsed = true
            )
        {
            ISymbolInformation result = new SymbolInformation();
            result.Name = name;
            result.Parent = SurroundingScope; //is null for root

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

            if (positionAsToken == null)
            {
                throw new ArgumentException(Resources.ExceptionMessages.token_requires);
            }

            result.Position = new TokenPosition
            {
                Token = positionAsToken,
                BodyStartToken = bodyStartPosAsToken,
                BodyEndToken = bodyEndPosAsToken
            };

            if (canBeUsed)
            {
                result.Usages = new List<ISymbolInformation>();
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
                throw new InvalidOperationException(Resources.SymbolTableStrings.symbol_creation_declaration_error);
            }

            if (addUsageAtDeclaration)
            {
                declarationSymbol?.Usages.Add(result);
            }

            if (canHaveChildren)
            {
                result.ChildrenHash = new Dictionary<string, ISymbolInformation>();
                result.Descendants = new List<ISymbolInformation>();
            }

            if (isDeclaration)
            {
                SurroundingScope?.ChildrenHash.Add(result.Name, result);
            }

            SurroundingScope?.Descendants.Add(result);

            result.Module = Module;

            return result;
        }

        protected void SetScope(ISymbolInformation symbol) => SurroundingScope = symbol;

        protected void JumpUpInScope() => SurroundingScope = SurroundingScope.Parent;

        protected void SetModule(ISymbolInformation symbol) => Module = symbol;

        protected void SetClass(ISymbolInformation symbol) => CurrentClass = symbol;

        #region empty-base-methods
        public override void Visit(IAstElement o)
        {
        }

        public override void Leave(IAstElement o)
        {
        }

        public override void Visit(ClassDecl o)
        {
        }

        public override void Leave(ClassDecl o)
        {
        }

        public override void Visit(Field o)
        {
        }

        public override void Leave(Field o)
        {
        }

        public override void Visit(Method o)
        {
        }

        public override void Leave(Method o)
        {
        }

        public override void Visit(Constructor o)
        {
        }

        public override void Leave(Constructor o)
        {
        }

        public override void Visit(Function o)
        {
        }

        public override void Leave(Function o)
        {
        }

        public override void Visit(NonglobalVariable o)
        {
        }

        public override void Leave(NonglobalVariable o)
        {
        }

        public override void Visit(Formal o)
        {
        }

        public override void Leave(Formal o)
        {
        }

        public override void Visit(LocalVariable o)
        {
        }

        public override void Leave(LocalVariable o)
        {
        }

        public override void Visit(AssignmentRhs o)
        {
        }

        public override void Leave(AssignmentRhs o)
        {
        }

        public override void Visit(TypeRhs e)
        {
        }

        public override void Leave(TypeRhs e)
        {
        }

        public override void Visit(BlockStmt o)
        {
        }

        public override void Leave(BlockStmt o)
        {
        }

        public override void Visit(WhileStmt s)
        {
        }

        public override void Leave(WhileStmt e)
        {
        }

        public override void Visit(IfStmt e)
        {
        }

        public override void Leave(IfStmt e)
        {
        }

        public override void Visit(Expression o)
        {
        }

        public override void Leave(Expression o)
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

        public override void Visit(IdentifierExpr e)
        {
        }

        public override void Leave(IdentifierExpr e)
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
        }

        public override void Leave(NameSegment e)
        {
        }

        public override void Leave(ModuleDefinition o)
        {
        }

        public override void Visit(ModuleDefinition o)
        {
        }

        public override void Leave(AliasModuleDecl o)
        {
        }

        public override void Visit(AliasModuleDecl o)
        {
        }

        public override void Visit(ExprDotName e)
        {
        }

        public override void Leave(ExprDotName e)
        {
        }

        public override void Visit(ThisExpr e)
        {
        }

        public override void Leave(ThisExpr e)
        {
        }

        public override void Visit(DisplayExpression o)
        {
        }

        public override void Leave(DisplayExpression o)
        {
        }

        public override void Visit(ComprehensionExpr o)
        {
        }

        public override void Leave(ComprehensionExpr o)
        {
        }
        #endregion empty-base-methods
    }
}