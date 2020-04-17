using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public class VisitorThatGeneratesSymbolTable : Visitor
    {
        public List<SymbolInformation> SymbolTable { get; set; } = new List<SymbolInformation>();
        public SymbolInformation ParentScope { get; set; }
        public SymbolInformation CurrentModule { get; set; }
        public SymbolInformation CurrentClass { get; set; }



        public override void Visit(IAstElement o) { }

        public override void Leave(IAstElement o) { }

        public override void Visit(ModuleDefinition o)
        {
            var symbol = new SymbolInformation()
            {
                Name = o.Name,
                Type = Type.Module,
                Position = new TokenPosition()
                {
                    Token = o.tok,
                    BodyStartToken = o.BodyStartTok,
                    BodyEndToken = o.BodyEndTok
                }

            };
            symbol.DeclarationOrigin = symbol;
            symbol.Usages = null;
            SymbolTable.Add(symbol);

            ParentScope = symbol;
            CurrentModule = symbol;

        }

        public override void Leave(ModuleDefinition o)
        {
            CurrentModule = null;
            ParentScope = null;
        }

        public override void Visit(ClassDecl o)
        {
            var classSymbol = new SymbolInformation()
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
            classSymbol.Parent = ParentScope;
            ParentScope.Children.Add(classSymbol);
            classSymbol.DeclarationOrigin = classSymbol;
            ParentScope = classSymbol;
            SymbolTable.Add(classSymbol);

            CurrentClass = classSymbol;

            //todo evtl heir alle member kurz definieren, weil eine decl auch nach der nutzung kommen kann.
            //dafür dann unten bei field, method defintiion etc muss man das dann net mehr machen.
            //lassen wir aber erstmal. erstmal das einfache.
        }

        public override void Leave(ClassDecl o)
        {
            ParentScope = ParentScope.Parent;
            CurrentClass = CurrentClass.Parent;
        }

        public override void Visit(Field o)
        {
            var fieldSymbol = new SymbolInformation()
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
            var methodSymbol = new SymbolInformation()
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

        /// <summary>
        /// Nonglobal Variables are Method Parameters. Base Class is Formal.
        /// </summary>
        public override void Visit(NonglobalVariable o)
        {
            var symbol = new SymbolInformation()
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
            ParentScope.Children.Add(symbol);
            SymbolTable.Add(symbol);
        }

        public override void Leave(NonglobalVariable o)   
          //todo evtl outs noch kcuken
        { 
        }

        //local variable are just locally defined vars: var bla:=2
        public override void Visit(LocalVariable o)
        {
            var symbol = new SymbolInformation()
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
            ParentScope.Children.Add(symbol);
            SymbolTable.Add(symbol);
        }

        public override void Leave(LocalVariable o) {
        }

    public override void Visit(BlockStmt o)
        {
            //todo: hier noch vermerken dass neuer scope irgendwie... für zukunftsmarcel und zukunftstom ticket 46548
            //..> BlockStmt als eigenes special-symbol handhaben, welches dann parent sein kann.
        }

        public override void Leave(BlockStmt o)
        {
            //todo siehe oben
        }

        public override void Visit(TypeRhs e)
        {
            UserDefinedType t = null;
            if (e.Type is UserDefinedType type)   //naja was kann denn sonst noch komen???
                t = type;

            var symbol = new SymbolInformation()
            {
                
                Name = t.Name, 
                Type = Type.Class,
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = t.tok,
                    BodyStartToken = e.Tok,
                    BodyEndToken = t.tok
                }
            };
            symbol.Children = null;
            symbol.Usages = null;
            symbol.Parent = ParentScope;

            //ctor der basisklasse usages +1
            //Declaration bei classdecl setzen.
            var declaration = FindDeclaration(symbol, ParentScope);
            declaration.Usages.Add(symbol);

            symbol.DeclarationOrigin = declaration; //müsste eig den ctor nehmen vlt aber, naja, geht auch so (und ist einfacher9.

            SymbolTable.Add(symbol);
        } 
        public override void Leave(TypeRhs e) { }

        //I encountered these when a declStatemnt is used, within the declartion's update statement, the left side is then a ghost thing.
        public override void Visit(AutoGhostIdentifierExpr e) { } //do nth since handled in localVar
        public override void Leave(AutoGhostIdentifierExpr e) { }

        public override void Visit(LiteralExpr e) { } //do nth
        public override void Leave(LiteralExpr e) { }

        public override void Visit(ApplySuffix e) { } //klammern nach einem methoden-call: do nth, visitor leitet das weiter ans namesegment und co.
        public override void Leave(ApplySuffix e) { }

        //Name Segment are identifiers, also from methods vor example two name segments in   var1 := returnsTwo(); --> var1, returnsTwo
        public override void Visit(NameSegment e)
        {
            var symbol = new SymbolInformation()
            {
                Name = e.Name,
                Type = Type.Variable,  //könnte auch methode sein, wissen wir jetzt halt net mehr, is aber eh bei definitionen wichitger schätz ich ma.
                Parent = ParentScope,
                Position = new TokenPosition()
                {
                    Token = e.tok,
                    BodyStartToken = e.tok,
                    BodyEndToken = e.tok
                }
            };
            symbol.Children = null;
            var declaration = FindDeclaration(symbol, ParentScope);
            declaration.Usages.Add(symbol);

            symbol.DeclarationOrigin = declaration;
            symbol.Usages = null;
            symbol.Children = null;
            symbol.Parent = ParentScope;

            SymbolTable.Add(symbol);
        }

        public override void Leave(NameSegment e)
        {
        }

        public override void Visit(Expression o)
        {
            //if we already know the symbol by a vardeclstatemnt, skip it.
            foreach (var symbol in SymbolTable)
            {
                if (symbol.Line == o.tok.line && symbol.Col == o.tok.col)
                {
                    return;
                }
            }

            //todo zu grobgranular
            var expressionSymbol = new SymbolInformation()
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
            expressionSymbol.Children = null; // cant have children

            var declaration = FindDeclaration(expressionSymbol, ParentScope);
            expressionSymbol.DeclarationOrigin = declaration;
            declaration.Usages.Add(expressionSymbol);
            SymbolTable.Add(expressionSymbol);
        }

        public override void Leave(Expression o)
        {
        }

        public override void Visit(AssignmentRhs o)
        {
            var symbol = new SymbolInformation()
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
            symbol.Children = null; // cant have children

            var declaration = FindDeclaration(symbol, ParentScope);
            symbol.DeclarationOrigin = declaration;
            declaration.Usages.Add(symbol);
            SymbolTable.Add(symbol);
        }


        public override void Leave(AssignmentRhs o)
        {
        }


        //todo hat da beim constructor das eine nicht gefuinden. das ctorarg glaubs. [glaub das ist wegen Left hand side vergessen]
        private SymbolInformation FindDeclaration(SymbolInformation target, SymbolInformation scope)  //evtl bei leave iwie
        {
            foreach (SymbolInformation s in scope.Children)
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

                //damit es nicht immer crashed erstmal soft-mässiges handling here:
                //throw new ArgumentOutOfRangeException("Symbol Declaration not found");
                return new SymbolInformation()
                {
                    Name = "*ERROR - DECLARATION SYMBOL NOT FOUND*"
                };
            }
        }


    }
}
