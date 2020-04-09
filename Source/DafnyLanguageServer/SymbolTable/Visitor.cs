using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public abstract class Visitor
    {

        public abstract void Visit(Declaration o);
        public abstract void Leave(Declaration o);

        public abstract void Visit(ClassDecl o);
        public abstract void Leave(ClassDecl o);

        public abstract void Visit(MemberDecl o);
        public abstract void Leave(MemberDecl o);

        public abstract void Visit(Field o);
        public abstract void Leave(Field o);

        public abstract void Visit(Method o);
        public abstract void Leave(Method o);

        public abstract void Visit(NonglobalVariable o);
        public abstract void Leave(NonglobalVariable o);

        public abstract void Visit(BlockStmt o);
        public abstract void Leave(BlockStmt o);

        //public abstract void Visit(VarDeclStmt o);
        //public abstract void Leave(VarDeclStmt o);

        public abstract void Visit(ConcreteUpdateStatement o);
        public abstract void Leave(ConcreteUpdateStatement o);
        public abstract void Visit(LocalVariable o);
        public abstract void Leave(LocalVariable o);
        public abstract void Visit(Expression o);
        public abstract void Leave(Expression o);

        public abstract void Visit(UpdateStmt o);
        public abstract void Leave(UpdateStmt o);

        public abstract void Visit(AssignOrReturnStmt o);
        public abstract void Leave(AssignOrReturnStmt o);


        public abstract void Visit(AssignSuchThatStmt o);
        public abstract void Leave(AssignSuchThatStmt o);

        public abstract void Visit(AssignmentRhs o);
        public abstract void Leave(AssignmentRhs o);




    }
}
