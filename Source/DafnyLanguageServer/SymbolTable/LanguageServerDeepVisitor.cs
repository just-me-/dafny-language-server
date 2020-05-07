using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie;
using Microsoft.Dafny;
using Serilog.Sinks.File;
using Function = Microsoft.Dafny.Function;
using IdentifierExpr = Microsoft.Dafny.IdentifierExpr;
using LiteralExpr = Microsoft.Dafny.LiteralExpr;
using LocalVariable = Microsoft.Dafny.LocalVariable;
using Visitor = Microsoft.Dafny.Visitor;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// This Visitor is used to generate the Symbol Table for Dafny code.
    /// It is an extension of the Microsoft Dafny Visitor Class.
    /// </summary>
    public class SymbolTableVisitorEverythingButDeclarations : LanguageServerVisitorBase
    {
        public override void Visit(ModuleDefinition o)
        {
            // Set scope but do not create new symbol.
            // Symbol should be the first symbol in table.
            var preDeclaredSymbol = CurrentModule; //VERASERASRASFE TEMP TEMP TEMP TEMP TEMP GEFährlich sonst!! todo revisit this after modulesa re impelemented... steht der wirklich immer zuoberst? darf ich das einfach so machen dann?
            if (preDeclaredSymbol.Name != o.Name || preDeclaredSymbol.Kind != Kind.Module || !preDeclaredSymbol.IsDeclaration)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.first_symbol_not_module);
            }
            SetScope(preDeclaredSymbol);
            SetModule(preDeclaredSymbol);
        }


        public override void Leave(ModuleDefinition o)
        {
            SetScope(null);
            //SetModule(null);
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
                    ISymbol baseSymbol = FindDeclaration(baseClassIdentifier.Name, SurroundingScope);
                    preDeclaredSymbol.BaseClasses.Add(baseSymbol);
                    //Create Symbol for the extends ->>BASE<-- so its clickable and base gets a reference coutn.
                    var t = CreateSymbol(
                      name: baseClassIdentifier.Name,
                      positionAsToken: baseClassIdentifier.tok,
                      bodyStartPosAsToken: baseClassIdentifier.tok,
                      bodyEndPosAsToken: baseClassIdentifier.tok,
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
            if (o.Type != null && o.Type is UserDefinedType)
            {
                userType = o.Type as UserDefinedType;
            }

            var symbol = CreateSymbol(
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

            var symbol = CreateSymbol(
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
            var name = "block-stmt-ghost" + o.GetHashCode();
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
                canBeUsed: false,
                addToSymbolTable: false
            );
            SetScope(symbol);
        }

        public override void Leave(BlockStmt o)
        {
            JumpUpInScope();
        }

        public override void Visit(WhileStmt o)
        {
            var name = "while-stmt-ghost" + o.GetHashCode();
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
                canBeUsed: false,
                addToSymbolTable: false
            );
            SetScope(symbol);
        }

        public override void Leave(WhileStmt e)
        {
            JumpUpInScope();
        }

        public override void Visit(IfStmt o)
        {
            var name = "if-stmt-ghost" + o.GetHashCode();

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
                canBeUsed: false,
                addToSymbolTable: false
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
            if (e.Type is UserDefinedType type)   //can it be anything else?
                t = type;

            var declaration = FindDeclaration(t.Name, SurroundingScope);

            UserDefinedType userType = null;
            if (e.Type != null && e.Type is UserDefinedType)
            {
                userType = e.Type as UserDefinedType;
            }
            var symbol = CreateSymbol(
                name: t.Name,
                kind: Kind.Class,
                type: e.Type,
                typeDefinition: userType,

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
        public override void Visit(AutoGhostIdentifierExpr e) { } //do nth since handled in localVar //todo nth? statt // /// verwenden fèr code doku #104
        public override void Leave(AutoGhostIdentifierExpr e) { }

        public override void Visit(LiteralExpr e) { } //do nth
        public override void Leave(LiteralExpr e) { }


        //ApllySuffixes are just brackets after a Method call.
        //The Visitor will redirect the accept statements to the expressions lef to the (), thus we do nth.
        public override void Visit(ApplySuffix e) { }
        public override void Leave(ApplySuffix e) { }

        //Name Segment are identifiers, especially also in methods.
        //For example two name segments in   var1 := returnsTwo(); --> var1, returnsTwo
        public override void Visit(NameSegment e)
        {
            var declaration = FindDeclaration(e.Name, SurroundingScope);

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
            string definingClassName = e.Lhs.Type.ToString();
            var definingClass = FindDeclaration(definingClassName, SurroundingScope, Kind.Class); // todo ist gleich wie typeDefinition
            var declaration = FindDeclaration(e.SuffixName, definingClass);

            UserDefinedType userType = null;
            if (e.Type != null && e.Type is UserDefinedType)
            {
                userType = e.Type as UserDefinedType;
            }
            var symbol = CreateSymbol(
                name: e.SuffixName,
                kind: null,
                type: e.Type,
                typeDefinition: userType,

                positionAsToken: e.tok, //nimmt den ganze, net nur den suffix.
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

            var symbol = CreateSymbol(
                name: "this",
                kind: Kind.Class,

                positionAsToken: e.tok,
                bodyStartPosAsToken: null,
                bodyEndPosAsToken: null,

                isDeclaration: false,
                declarationSymbol: declaration,
                addUsageAtDeclaration: false,    //fänd ich komisch. jedes mal "this" = zusätzliche referenz? oder?

                canHaveChildren: false,
                canBeUsed: false
            );
        }

        public override void Leave(ThisExpr e)
        {
        }

        public override void Visit(Expression o)
        {
            // todo mergen213
            var declaration = FindDeclaration(o.tok.val, SurroundingScope);

            UserDefinedType userType = null;
            if (o.Type != null && o.Type is UserDefinedType)
            {
                userType = o.Type as UserDefinedType;
            }
            var symbol = CreateSymbol(
                name: o.tok.val + "** General Expression Visit Used!! **",
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

            var symbol = CreateSymbol(
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
