﻿using Microsoft.Boogie;
using Microsoft.Dafny;
using System.Collections.Generic;
using System.Linq;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// Provides all needed SymbolTableInformation for all Modules. Needs a Dafny Program to work.
    /// Mostly there is only one module - the default module (and class) for a single Dafny file.
    /// This class also works as a facade for <c>SymbolNavigator</c>.
    /// </summary>
    public class SymbolTableGenerator : ISymbolTableGenerator
    {
        /// <summary>
        /// A virtual Root Symbol. It covers all range, can not have a parent, and has all top level modules as descendants.
        /// </summary>
        private ISymbolInformation DafnyProgramRootSymbol { get; }

        private Microsoft.Dafny.Program DafnyProgram { get; }

        public SymbolTableGenerator(Microsoft.Dafny.Program dafnyProgram)
        {
            DafnyProgram = dafnyProgram;
            DafnyProgramRootSymbol = CreateRootNode();
            DafnyProgramRootSymbol.DeclarationOrigin = DafnyProgramRootSymbol;
        }

        /// <summary>
        /// Generates the symbolt able for the provdied Dafny Program.
        /// </summary>
        /// <returns>A single symbol acting as the root entrypoint (aka global namespace) for that dafny program.</returns>
        public ISymbolInformation GenerateSymbolTable()
        {
            var modules = DafnyProgram.Modules().ToList();
            modules.Sort((m1, m2) => Depth(m1) - Depth(m2));

            foreach (var module in modules)
            {
                ISymbolInformation rootForVisitor = GetEntryPoint(module);
                var declarationVisitor = new LanguageServerDeclarationVisitor(rootForVisitor);
                module.Accept(declarationVisitor);
            }

            foreach (var module in modules)
            {
                ISymbolInformation rootForVisitor = GetEntryPoint(module);
                var deepVisitor = new SymbolTableVisitorEverythingButDeclarations(rootForVisitor);
                module.Accept(deepVisitor);
            }

            return DafnyProgramRootSymbol;
        }

        private static ISymbolInformation CreateRootNode()
        {
            return new SymbolInformation
            {
                ChildrenHash = new Dictionary<string, ISymbolInformation>(),
                Descendants = new List<ISymbolInformation>(),
                Kind = Kind.RootNode,
                Name = Resources.SymbolTableStrings.root_node,
                Position = new TokenPosition
                {
                    Token = new Token(0, 0),
                    BodyStartToken = new Token(0, 0),
                    BodyEndToken = new Token(int.MaxValue - 1024, int.MaxValue - 1024)
                }
            };
        }

        /// <summary>
        /// Returns an empty symbol tree, that is, just a root node without any descendants.
        /// </summary>
        public static ISymbolInformation GetEmptySymbolTable()
        {
            return CreateRootNode();
        }

        private int Depth(ModuleDefinition m) => m.FullName.Split('.').Length - 1;   //gäbe height iwas.

        private ISymbolInformation GetEntryPoint(ModuleDefinition m)
        {
            var hierarchy = m.FullName.Split('.').ToList();
            var rootForVisitor = DafnyProgramRootSymbol;

            while (hierarchy.Count > 1)
            {
                rootForVisitor = rootForVisitor[hierarchy.First()];
                hierarchy.RemoveAt(0);
            }

            return rootForVisitor;
        }
    }
}