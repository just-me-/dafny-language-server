using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dafny;
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
    /// It is an extension of the Microsoft Dafny Visitor Class.
    /// </summary>
    public class SymbolTableVisitorEverythingButDeclarations : LanguageServerVisitorBase
    {
        public SymbolTableVisitorEverythingButDeclarations(ISymbol entryPoint) : base(entryPoint)
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
                bodyStartPosAsToken: o.tok,
                bodyEndPosAsToken: o.tok,
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
                preDeclaredSymbol.BaseClasses = new List<ISymbol>();
                foreach (var baseClassType in o.TraitsTyp)
                {
                    var baseClassIdentifier = baseClassType as UserDefinedType; //trait is always userdefined, right? kann net von string erben oder so.
                    ISymbol baseSymbol = FindDeclaration(baseClassIdentifier?.Name, SurroundingScope);
                    preDeclaredSymbol.BaseClasses.Add(baseSymbol);
                    //Create Symbol for the extends ->>BASE<-- so its clickable and base gets a reference coutn.
                    var t = CreateSymbol(
                      name: baseClassIdentifier?.Name,
                      positionAsToken: baseClassIdentifier?.tok,
                      bodyStartPosAsToken: baseClassIdentifier?.tok,
                      bodyEndPosAsToken: baseClassIdentifier?.tok,
                      kind: Kind.Class,
                      type: baseClassType,
                      typeDefinition: baseClassIdentifier,
                      isDeclaration: false,
                      declarationSymbol: baseSymbol,
                      addUsageAtDeclaration: true,
                      canHaveChildren: false,
                      canBeUsed: false
                    );
                    //adjust parent because the 'Mimi extends BABA' parent is rather Mimi then whtatever.
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
            //do nothing, no need to set scope or such for fields
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

            SetScope(preDeclaredSymbol); //technically not necessary since we dont go deeper.
        }

        public override void Leave(Function o)
        {
            JumpUpInScope();
        }


        #endregion

        ///////"new" stuff from here on the other visitor didnt do from here on.


        /// <summary>
        /// Non global variables are method parameters (in and out parameters).
        /// We treat them as variable definitions.
        /// Additional Info: Base Class of non global variable is formal.
        /// </summary>
        public override void Visit(NonglobalVariable o)
        {
            UserDefinedType userType = null;
            if (o.Type != null && o.Type is UserDefinedType)       //TODO: das sind auch sonar und resharpesr issues und der code ist immer gelich -> auslagern!! ich hab aber angst das alles kaputt geht drum später machen ;) iwo in nem getter hab ich das schonmal vorbereitet gehabt.
            {
                userType = o.Type as UserDefinedType;
            }

            CreateSymbol(
                name: o.Name,
                kind: Kind.Variable,
                type: o.Type,
                typeDefinition: userType,

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

        //local variable are just locally defined vars: var bla:=2
        public override void Visit(LocalVariable o)
        {
            UserDefinedType userType = null;
            if (o.Type != null && o.Type is UserDefinedType)
            {
                userType = o.Type as UserDefinedType;
            }

            CreateSymbol(
                name: o.Name,
                kind: Kind.Variable,
                type: o.Type,
                typeDefinition: userType,

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


        //todo: if, while, blockstatement:
        //damit die als children beim parent gelten, und somit von TopDown durchsucht werden, muss man aktuell "isDeclartion: true" setzen.
        //Evtl schöner: "addAsChild" boolean... möchte jetzt aber das rename machen.
        //Weiteres Problem war: Hash Name muss ja unique sein, drum hab ich noch das + o.GetHashCode geadded.
        //Tests 3, 7, 10schlagen entsprechend fehl, da ein zusätzliches Child nun drin ist und der Name auch den Hashcode beinhatlet.
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

        /// <summary>
        /// A <c>TypeRhs</c> is the right hand side of something like var a:= new MyClass(). See also its class description.
        /// Also has some Array stuff that could be relevant for us.
        /// </summary>
        public override void Visit(TypeRhs e)
        {
            UserDefinedType t = null;
            if (e.Type != null && e.Type is UserDefinedType)
            {
                t = e.Type as UserDefinedType;
            }

            if (t == null)
            {
                throw new InvalidOperationException(Resources.SymbolTableStrings.typeRHS_vs_UserDefinedType); //todo vor der abgabe zumindest mal noch testen ob das auch bei anderen type rhs der fall ist, wie z.b. ein int arraay hust hust (spoilerarlert... ne ich sags lieber nicht).
            }


            var nav = new SymbolTableNavigator();
            var declaration = nav.GetSymbolByPosition(RootNode, t?.ResolvedClass.tok);

            CreateSymbol(
                name: t.Name,
                kind: Kind.Class,
                type: e.Type,
                typeDefinition: t,

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

        public override void Leave(TypeRhs e) { }

        //A AutoGhostIndentifierExpr is when we have a VarDeclStmt: var a:= b.
        //This VarDecl Stmt contains a Update stmt, and the left side contains ghostVars, aka the 'a'.
        //We do nth since within the VarDeclStmt, the 'a' gets registered.
        public override void Visit(AutoGhostIdentifierExpr e) { }
        public override void Leave(AutoGhostIdentifierExpr e) { }

        public override void Visit(LiteralExpr e) { }
        public override void Leave(LiteralExpr e) { }


        //ApllySuffixes are just brackets after a Method call.
        //The Visitor will redirect the accept statements to the expressions lef to the (), thus we do nth.
        public override void Visit(ApplySuffix e) { }
        public override void Leave(ApplySuffix e) { }

        //Name Segment are identifiers, especially also in methods.
        //For example two name segments in   var1 := returnsTwo(); --> var1, returnsTwo
        public override void Visit(NameSegment e)
        {
            var nav = new SymbolTableNavigator();
            var resolvedSymbol = nav.TopDown(RootNode, e.ResolvedExpression.tok);

            var declaration = FindDeclaration(e.Name, resolvedSymbol);


            //var declaration = FindDeclaration(e.SuffixName, definingItem);

            UserDefinedType userType = null;
            if (e.Type != null && e.Type is UserDefinedType)
            {
                userType = e.Type as UserDefinedType;
            }
            var symbol = CreateSymbol(
                name: e.Name,
                kind: null,
                type: e.Type,
                typeDefinition: userType,

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
                mse = e.ResolvedExpression as MemberSelectExpr; //todo mit visitor machen. //geht net weil ich brauch ja auch e ansich.
            }

            var nav = new SymbolTableNavigator();
            var definingItem = nav.TopDown(RootNode, mse?.Member.tok);

            var declaration = FindDeclaration(e.SuffixName, definingItem);

            UserDefinedType userType = null;
            if (e.Type != null && e.Type is UserDefinedType)
            {
                userType = e.Type as UserDefinedType;
            }
            CreateSymbol(
                name: e.SuffixName,
                kind: null,
                type: e.Type,
                typeDefinition: userType,

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
            //we could also just do nothing, then you couldn't click on "this." for goto def or so.
            //maybe even better cause many this names may cause trouble or such, i don't know.

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

            UserDefinedType userType = null;
            if (o.Type != null && o.Type is UserDefinedType)
            {
                userType = o.Type as UserDefinedType;
            }
            CreateSymbol(
                name: o.tok.val + Resources.SymbolTableStrings.general_expression_visit_used,
                kind: null,
                type: o.Type,
                typeDefinition: userType,

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
        { //this expr often occurs within "decrease" clauses, although ther is no decrease statement... so i guess we just skip it.
        }

        public override void Leave(IdentifierExpr e)
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
