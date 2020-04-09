﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Boogie;
using Microsoft.Dafny;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DafnyProgram = Microsoft.Dafny.Program;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;

namespace DafnyLanguageServer.DafnyAccess
{


    public class MetaDafnySymboltableBaem
    {
        public List<SymbolInformation> SymbolTable { get; set; } = new List<SymbolInformation>();
        private DafnyProgram DafnyProgram { get; }

        private ModuleDefinition CurrentModule { get; set; }

        public MetaDafnySymboltableBaem(DafnyProgram dafnyProgram)
        {
            DafnyProgram = dafnyProgram;

            foreach (var module in dafnyProgram.Modules())
            {
                CurrentModule = module;
                var allClasses = ModuleDefinition.AllClasses(module.TopLevelDecls);
                foreach (ClassDecl cd in allClasses)
                {
                    HandleClass(cd);
                }
            }
        }

        public void HandleClass(ClassDecl classDecl)
        {
            var classSymbol = new SymbolInformation()
            {
                Module = CurrentModule,
                Name = classDecl.Name,
                Position = new TokenPosition()
                {
                    Token = classDecl.tok,
                    BodyStartToken = classDecl.BodyStartTok,
                    BodyEndToken = classDecl.BodyEndTok
                },
                Type = Type.Class
            };
            classSymbol.DeclarationOrigin = classSymbol;
            classSymbol.Children = new List<SymbolInformation>();     //für class symbol fehlt: usages

            foreach (var member in classDecl.Members)
            {
                HandleClassMemberDeklaration(member, classSymbol);

            }
        }

        public void HandleClassMemberDeklaration(MemberDecl member, SymbolInformation parent)
        {
            switch (member)
            {
                case Field memberAsField:
                {
                    HandleField(memberAsField, parent);
                    break;
                }
                case Method memberAsMethod:
                    //memberAsMethod.accept(new SymbolTableGeneratorWithVisitor()); //so, oder?=? warum keine accept method?

                    
                    HandleMethod(memberAsMethod, parent);
                    break;
            }
        }

        public void HandleField(Field memberAsField, SymbolInformation parent)
        {
            var fieldSymbol = new SymbolInformation()
            {
                Module = CurrentModule,
                Name = memberAsField.Name,
                Type = Type.Field,
                Parent = parent,
                Position = new TokenPosition()
                {
                    Token = memberAsField.tok,
                    BodyStartToken = memberAsField.BodyStartTok,
                    BodyEndToken = memberAsField.BodyEndTok
                }
            };
            fieldSymbol.DeclarationOrigin = fieldSymbol;   //für membersymbol fehlt: children, usages
            parent.Children.Add(fieldSymbol);
            SymbolTable.Add(fieldSymbol);
            
        }

        public void HandleMethod(Method memberAsMethod, SymbolInformation parent)
        {
            var methodSymbol = new SymbolInformation()
            {
                Module = CurrentModule,
                Name = memberAsMethod.Name,
                Type = Type.Field,
                Parent = parent,
                Position = new TokenPosition()
                {
                    Token = memberAsMethod.tok,
                    BodyStartToken = memberAsMethod.BodyStartTok,
                    BodyEndToken = memberAsMethod.BodyEndTok
                }
            };

            methodSymbol.DeclarationOrigin = methodSymbol;   //für membersymbol fehlt: children, usages
            parent.Children.Add(methodSymbol);
            SymbolTable.Add(methodSymbol);

            //todo argumente handlen

            //kucken was dinger sind:
            var substat = memberAsMethod.Body.SubStatements.ToList();
            var subexpr = memberAsMethod.Body.SubExpressions.ToList();
            foreach (Statement statement in memberAsMethod.Body.Body)  //substatements besser?
            {
                HandleStatement(statement, methodSymbol);
            }
        }


        public void HandleStatement(Statement s, SymbolInformation parent)
        {
            
            switch (s)  
            {
                case VarDeclStmt vds:
                    HandleVarDeclStmt(vds, parent);
                    break;

                case ConcreteUpdateStatement us:
                    HandleUpdateStatment(us, parent);
                    break;

                case ProduceStmt us:
                    break;

                case AssignStmt asgns:
                    break;

                case BlockStmt bs:
                    foreach (Statement subStatement in bs.Body)
                    {
                        HandleStatement(subStatement, parent);
                    }

                    break;

                case CalcStmt bs:
                    break;

                case PrintStmt bs:
                    break;

                //more.... if, while etc pp

            }
        }

        public void HandleVarDeclStmt(VarDeclStmt vds, SymbolInformation parent)
        {
            foreach (var localVar in vds.Locals)
            {
                var variableSymbol = new SymbolInformation()
                {
                    Module = CurrentModule,
                    Name = localVar.Name,     //es gäbe unique name - evtl geiler wegen shadowing
                    Type = Type.Variable,
                    Parent = parent,
                    Position = new TokenPosition()
                    {
                        Token = localVar.Tok,
                        BodyStartToken = localVar.Tok,
                        BodyEndToken = localVar.EndTok
                    }
                };
                variableSymbol.DeclarationOrigin = variableSymbol;  //hier fehlen usages
                variableSymbol.Children = null;
                SymbolTable.Add(variableSymbol);
                parent.Children.Add(variableSymbol);
            }
            HandleUpdateStatment(vds.Update, parent);
        }

        public void HandleUpdateStatment(ConcreteUpdateStatement us, SymbolInformation parent)
        {
            foreach (Expression e in us.Lhss)
            {
                var expressionSymbol = new SymbolInformation()
                {
                    Module = CurrentModule,
                    Name = e.tok.val,
                    Type = Type.Variable,
                    Parent = parent,
                    Position = new TokenPosition()
                    {
                        Token = e.tok,
                        BodyStartToken = e.tok,
                        BodyEndToken = e.tok
                    }
                };
                parent.Children.Add(expressionSymbol);
                expressionSymbol.Children = null; // cant have children
                var declaration = FindDeclaration(e, parent);
                expressionSymbol.DeclarationOrigin = declaration;
                declaration.Usages.Add(expressionSymbol);
            }

            if (us is UpdateStmt uss)
            {

                //what is uss.rhs?
                foreach (var rx in uss.Rhss)
                {
                    if (rx is LiteralExpr) continue;
                    if (rx is ExprRhs re)
                    {
                        var expressionSymbol = new SymbolInformation()
                        {
                            Module = CurrentModule,
                            Name = re.Tok.val, //gabs da kein name?
                            Type = Type.Variable,
                            Parent = parent,
                            Position = new TokenPosition()
                            {
                                Token = re.Tok,
                                BodyStartToken = re.Tok,
                                BodyEndToken = re.Tok
                            }
                        };
                        parent.Children.Add(expressionSymbol);
                        expressionSymbol.Children = null; // cant have children
                        //var declaration = FindDeclaration(expressionSymbol, parent);
                        //expressionSymbol.DeclarationOrigin = declaration;
                        //declaration.Usages.Add(expressionSymbol);
                    }
                    //...nm is liste, dann hat es da expr, dort ist name und tok. tok is auch eins höher.
                }
            }

        }

        private SymbolInformation FindDeclaration(Expression e, SymbolInformation parent)
        {
            foreach (SymbolInformation s in parent.Children)
            {
                if (s.Name == e.tok.val && s.IsDeclaration) return s;
            }
            //if symbol not found in current scope, search parent
            if (parent.Parent != null) { 
                return FindDeclaration(e, parent.Parent);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Symbol Declaration not found");
            }
        }
    }


    public class SymbolTableGeneratorWithVisitor : TopDownVisitor<List<SymbolInformation>> {
       


    }
    public class SymbolInformation
    {
        public TokenPosition Position { get; set; }
        public string Name { get; set; }

        //evt wieder weg
        public ModuleDefinition Module { get; set; }

        public Type Type { get; set; }
        public SymbolInformation Parent { get; set; }
        public SymbolInformation DeclarationOrigin { get; set; }
        public List<SymbolInformation> Children { get; set; } = new List<SymbolInformation>();
        public List<SymbolInformation> Usages { get; set; } = new List<SymbolInformation>();
        public bool IsDeclaration => DeclarationOrigin == this;
    }


    public enum Type
    {
        Class,
        Method,
        Function,
        Field,
        Variable,
        Call,
        Definition,
        Predicate
    }

    public class TokenPosition
    {
        public IToken BodyStartToken { get; set; }
        public IToken BodyEndToken { get; set; }
        public IToken Token { get; set; }
    }
}