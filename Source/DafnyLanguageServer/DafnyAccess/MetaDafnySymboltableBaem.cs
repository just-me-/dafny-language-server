using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Boogie;
using Microsoft.Dafny;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DafnyProgram = Microsoft.Dafny.Program;

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
            //todo
            switch (s)
            {
                case VarDeclStmt vds:
                    break;

                case ConcreteUpdateStatement us:
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

                //more?

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
        public List<SymbolInformation> Children { get; set; }
        public List<SymbolInformation> Usages { get; set; }


    }


    public enum Type
    {
        Class,
        Method,
        Function,
        Field,
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