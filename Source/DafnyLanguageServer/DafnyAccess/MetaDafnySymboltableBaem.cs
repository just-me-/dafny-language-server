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

        public MetaDafnySymboltableBaem(DafnyProgram dafnyProgram)
        {
            DafnyProgram = dafnyProgram;
            GenerateSymboltable();
        }


        public void GenerateSymboltable()
        {
            foreach (var module in DafnyProgram.Modules())
            {
                AddClasses(module);
                //AddMethods(module);
                //AddFields(module);
            }
        }

        private void AddClasses(ModuleDefinition module)
        {
            var allTopLevelDeclarations = ModuleDefinition.AllClasses(module.TopLevelDecls).Where(e => e != null);
            var werbinich = allTopLevelDeclarations.ToList();

            foreach (var classDecl in allTopLevelDeclarations)
            {
                var symbol = new SymbolInformation()
                {
                    Module = module,
                    Name = classDecl.Name,
                    Position = new TokenPosition()
                    {
                        Token = classDecl.tok,
                        BodyStartToken = classDecl.BodyStartTok,
                        BodyEndToken = classDecl.BodyEndTok
                    },
                    Type = Type.Class
                };
                symbol.DeclarationOrigin = symbol;

                symbol.Children = new List<SymbolInformation>();

                foreach (var member in classDecl.Members)
                {
                    var symbolForMember = new SymbolInformation()
                    {
                        Module = module,
                        Name = member.Name,
                        Type = member is Field ? Type.Field : Type.Method, //evtl constructor noch berücksichtigen ticket 1566
                        Parent = symbol,
                        Position = new TokenPosition()
                        {
                            Token = member.tok,
                            BodyStartToken = member.BodyStartTok,
                            BodyEndToken = member.BodyEndTok
                        }
                    };
                    symbolForMember.DeclarationOrigin = symbolForMember;   //für membersymbol fehlt: children usages

                    var werbinichdennunichweissesnicht = member.SubExpressions.ToList();
                    if (member is Method memberAsMethod)
                    {
                        var x = memberAsMethod.Body.Body;
                        foreach (var stmt in memberAsMethod.Body.Body)
                        {
  
                            if (stmt is VarDeclStmt declaration)
                            {
                                var symbolForStatement = new SymbolInformation()
                                {
                                    //... mega deep
                                };
                            }
                        }
                    }
                    symbol.Children.Add(symbolForMember);


                }

            }


        }


        


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