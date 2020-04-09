using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    class SymbolTableGeneratorVisitor : Visitor
    {
        public List<NewSymbolInformation> SymbolTable { get; set; } = new List<NewSymbolInformation>();

        public NewSymbolInformation ParentScope { get; set; }


        public override void Visit(Declaration o)
        {
            throw new NotImplementedException();
        }

        public override void Leave(Declaration o)
        {
            throw new NotImplementedException();
        }

        public override void Visit(ClassDecl o)
        {
            
            var classSymbol = new NewSymbolInformation()
            {
                Name = o.Name,
                Position = new TokenPosition()
                {
                    Token = o.tok,
                    BodyStartToken = o.BodyStartTok,
                    BodyEndToken = o.BodyEndTok
                },
                Type = Type.Class
            };
            classSymbol.DeclarationOrigin = classSymbol;  //für class symbol fehlt: usages
            ParentScope = classSymbol;
            SymbolTable.Add(classSymbol);


        }




        public override void Leave(ClassDecl o)
        {
            ParentScope = ParentScope.Parent;
        }

        public override void Visit(MemberDecl o)
        {
            throw new NotImplementedException("This should be overwritten");
        }

        public override void Leave(MemberDecl o) 
        {
            throw new NotImplementedException("This should be overwritten");
        }

        public override void Visit(Field o)
        {
            var fieldSymbol = new NewSymbolInformation()
            {
                Name = o.Name,
                Type = Type.Field,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = o.tok,
                    BodyStartToken = o.BodyStartTok,
                    BodyEndToken = o.BodyEndTok
                }
            };
            fieldSymbol.DeclarationOrigin = fieldSymbol;   //für membersymbol fehlt: children, usages
            ParentScope.Children.Add(fieldSymbol);
            SymbolTable.Add(fieldSymbol);
        }

        public override void Leave(Field o)
        {
        }

        public override void Visit(Method o)
        {
            var methodSymbol = new NewSymbolInformation()
            {
                Name = o.Name,
                Type = Type.Field,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = o.tok,
                    BodyStartToken = o.BodyStartTok,
                    BodyEndToken = o.BodyEndTok
                }
            };

            methodSymbol.DeclarationOrigin = methodSymbol;   //für membersymbol fehlt: children, usages
            ParentScope.Children.Add(methodSymbol);
            SymbolTable.Add(methodSymbol);

            ParentScope = methodSymbol;
        }


        public override void Leave(Method o)
        {
            ParentScope = ParentScope.Parent;
        }




        public override void Visit(NonglobalVariable o)
        {
            var symbol = new NewSymbolInformation()
            {
                Name = o.Name,
                Type = Type.Variable,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = o.Tok,
                    BodyStartToken = o.Tok,
                    BodyEndToken = o.Tok
                }
            };
            symbol.Children = null;
            symbol.DeclarationOrigin = symbol;
            symbol.Parent = ParentScope;
            SymbolTable.Add(symbol);
        }

        public override void Leave(NonglobalVariable o)   //todo evtl outs noch kcuken
        { 
        }

        public override void Visit(LocalVariable o)
        {
            var symbol = new NewSymbolInformation()
            {
                Name = o.Name, //höätte auch son unique name und so.
                Type = Type.Variable,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = o.Tok,
                    BodyStartToken = o.Tok,
                    BodyEndToken = o.Tok
                }
            };
            symbol.Children = null;
            symbol.DeclarationOrigin = symbol;
            symbol.Parent = ParentScope;
            SymbolTable.Add(symbol);
        }

        public override void Leave(LocalVariable o)
        {
        }



        public override void Visit(BlockStmt o)
        {
            //todo: hier noch vermerken dass neuer scope irgendwie... für zukunftsmarcel und zukunftstom ticket 46548
        }

        public override void Leave(BlockStmt o)
        {
            //todo siehe oben
        }

        public override void Visit(ConcreteUpdateStatement o)
        {
            throw new NotImplementedException();
        }

        public override void Leave(ConcreteUpdateStatement o)
        {
            throw new NotImplementedException();
        }

   

        public override void Visit(Expression o)
        {
            var expressionSymbol = new NewSymbolInformation()
            {
                Name = o.tok.val,
                Type = Type.Variable,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = o.tok,
                    BodyStartToken = o.tok,
                    BodyEndToken = o.tok
                }
            };
            ParentScope.Children.Add(expressionSymbol);
            expressionSymbol.Children = null; // cant have children

            var declaration = FindDeclaration(expressionSymbol, ParentScope);
            expressionSymbol.DeclarationOrigin = declaration;
            declaration.Usages.Add(expressionSymbol);
            SymbolTable.Add(expressionSymbol);
        }

        public override void Leave(Expression o)
        {
        }

        public override void Visit(UpdateStmt o)
        {
            throw new NotImplementedException();
        }

        public override void Leave(UpdateStmt o)
        {
            throw new NotImplementedException();
        }

        public override void Visit(AssignOrReturnStmt o)
        {
            throw new NotImplementedException();
        }

        public override void Leave(AssignOrReturnStmt o)
        {
            throw new NotImplementedException();
        }

        public override void Visit(AssignSuchThatStmt o)
        {
            throw new NotImplementedException();
        }

        public override void Leave(AssignSuchThatStmt o)
        {
            throw new NotImplementedException();
        }

        public override void Visit(AssignmentRhs o)
        {
            var symbol = new NewSymbolInformation()
            {
                Name = o.Tok.val,
                Type = Type.Variable,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = o.Tok,
                    BodyStartToken = o.Tok,
                    BodyEndToken = o.Tok
                }
            };
            ParentScope.Children.Add(symbol);
            symbol.Children = null; // cant have children

            var declaration = FindDeclaration(symbol, ParentScope);
            symbol.DeclarationOrigin = declaration;
            declaration.Usages.Add(symbol);
            SymbolTable.Add(symbol);
        }

        public override void Leave(AssignmentRhs o)
        {
            throw new NotImplementedException();
        }

        private NewSymbolInformation FindDeclaration(NewSymbolInformation target, NewSymbolInformation scope)  //evtl bei leave iwie
        {
            foreach (NewSymbolInformation s in scope.Children)
            {
                if (s.Name == target.Name && s.IsDeclaration) return s;
            }
            //if symbol not found in current scope, search scope
            if (scope.Parent != null)
            {
                return FindDeclaration(target, scope.Parent);
            }
            else
            {
                //fujnzt noch nicht, z.b. bei methjodenargumenten.
                //throw new ArgumentOutOfRangeException("Symbol Declaration not found");
                return new NewSymbolInformation();
            }
        }


    }
}
