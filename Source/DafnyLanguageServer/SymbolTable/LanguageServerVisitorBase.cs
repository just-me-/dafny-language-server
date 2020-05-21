﻿using System;
using System.Collections.Generic;
using Microsoft.Boogie;
using Microsoft.Dafny;
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
        protected LanguageServerVisitorBase(ISymbol entryPoint)
        {
            SetScope(entryPoint);
            var root = entryPoint;
            while (root.Kind != Kind.RootNode)
            {
                root = root.Parent;
            }
            RootNode = root;
        }

        public ISymbol RootNode { get; set; }

        //Zum Aufbau:
        public ISymbol SurroundingScope { get; set; }

        //Accessor for Convenience:
        public ISymbol Module { get; set; } //<- unique pro visitor - jeder visitor geht ja nur ein modul durch.
        public ISymbol CurrentClass { get; set; }

        public ISymbol DefaultClass => Module.ChildrenHash.ContainsKey(DEFAULT_CLASS_NAME) ? Module[DEFAULT_CLASS_NAME] : null;

        public ISymbol DefaultModule => RootNode.ChildrenHash.ContainsKey(DEFAULT_MODULE_NAME) ? RootNode[DEFAULT_MODULE_NAME] : null;

        protected ISymbol FindDeclaration(string target, ISymbol scope, Kind? type = null, bool goRecursive = true)
        {
            INavigator navigator = new SymbolTableNavigator();
            bool filter(ISymbol s) => s.Name == target && s.IsDeclaration && (type == null || s.Kind == type);
            return navigator.BottomUpFirst(scope, filter) ?? new SymbolInformation
            {
                Name = Resources.SymbolTable.declaration_not_found,
                ChildrenHash = new Dictionary<string, ISymbol>(),
                Usages = new List<ISymbol>()
            };
        }

        protected ISymbol FindDeclaration(ISymbol target, ISymbol scope)  //evtl bei leave iwie
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
        /// <returns>Returns a SymbolInformation about the specific token.</returns>
        protected ISymbol CreateSymbol(
            string name,
            IToken positionAsToken,

            Kind? kind = Kind.Undefined,
            Type type = null,
            UserDefinedType typeDefinition = null,

            IToken bodyStartPosAsToken = null,
            IToken bodyEndPosAsToken = null,

            bool isDeclaration = true,
            ISymbol declarationSymbol = null,
            bool addUsageAtDeclaration = false,
            bool canHaveChildren = true,
            bool canBeUsed = true
            )
        {
            ISymbol result = new SymbolInformation();
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

            result.Position = new TokenPosition
            {
                Token = positionAsToken,
                BodyStartToken = bodyStartPosAsToken ?? positionAsToken,
                BodyEndToken = bodyEndPosAsToken ?? positionAsToken
            };

            if (canBeUsed)
            {
                result.Usages = new List<ISymbol>();
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
                throw new InvalidOperationException(Resources.SymbolTable.symbol_creation_declaration_error);
            }

            if (addUsageAtDeclaration)
            {
                declarationSymbol?.Usages.Add(result);
            }

            if (canHaveChildren)
            {
                result.ChildrenHash = new Dictionary<string, ISymbol>();
                result.Descendants = new List<ISymbol>();
            }

            if (isDeclaration) //add child unless we are on toplevel scope.
            {
                SurroundingScope?.ChildrenHash.Add(result.Name, result);
            }

            SurroundingScope?.Descendants.Add(result);

            result.Module = Module;

            return result;
        }

        protected void SetScope(ISymbol symbol) => SurroundingScope = symbol;
        protected void JumpUpInScope() => SurroundingScope = SurroundingScope.Parent;
        protected void SetModule(ISymbol symbol) => Module = symbol;
        protected void SetClass(ISymbol symbol) => CurrentClass = symbol;

        public override void Visit(IAstElement o) { }

        public override void Leave(IAstElement o) { }
    }
}
