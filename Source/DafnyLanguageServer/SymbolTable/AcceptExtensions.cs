using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Boogie.VCExprAST;
using Microsoft.Dafny;

namespace DafnyLanguageServer.SymbolTable
{
    public static class AcceptExtensions
    {

        public static void Accept(this Declaration d, Visitor v)
        {
            v.Visit(d);
            v.Leave(d);
        }

        public static void Accept(this TopLevelDeclWithMembers d, Visitor v)
        {
            v.Visit(d);
            v.Leave(d);
        }

         public static void Accept(this ClassDecl d, Visitor v)
        {
            v.Visit(d);
            foreach (var memberino in d.Members)
            {
                memberino.Accept(v);
            }
            v.Leave(d);
        }

        public static void Accept(this MemberDecl d, Visitor v)          //braucht es diese abstrakten dinger???
        {
            //das ist blöd hier. weil virtual nicht funktioniert. corbat fraben todo.
            //das tut nur virtual simuliueren
            if (d is Field df) df.Accept(v);
            if (d is Method dm) dm.Accept(v);
            if (d is Function dfu) dfu.Accept(v);
        }

        public static void Accept(this Field d, Visitor v)          //braucht es diese abstrakten dinger???
        {
            v.Visit(d); //nix special hier
            v.Leave(d);
        }

        public static void Accept(this Function d, Visitor v)          //braucht es diese abstrakten dinger???
        {
            v.Visit(d);
            //oder so

            //todo

            v.Leave(d);
        }

        public static void Accept(this Method d, Visitor v)          //braucht es diese abstrakten dinger???
        {
            v.Visit(d);
            foreach (var arg in d.Ins) //ich glaube, das sind die argumente
            {
                arg.Accept(v);
            }

            d.Body.Accept(v);

            v.Leave(d);

        }

        public static void Accept(this Formal d, Visitor v)
        {
            v.Visit(d);
            v.Leave(d);
        }

        public static void Accept(this BlockStmt d, Visitor v)
        {
            foreach (Statement stmt in d.Body)
            {
                stmt.Accept(v);
            }
        }

        public static void Accept(this Statement d, Visitor v)
        {
            //muss hier halt wieder delgieren, wtf.
            switch (d)
            {
                case VarDeclStmt c:
                    c.Accept(v);
                    break;

                case ConcreteUpdateStatement c:
                    c.Accept(v);
                    break;

                case ProduceStmt c:
                    c.Accept(v);
                    break;

                case AssignStmt c:
                    c.Accept(v);
                    break;

                case BlockStmt c:
                    c.Accept(v);
                    break;

                case CalcStmt c:
                    c.Accept(v);
                    break;

                case PrintStmt c:
                    c.Accept(v);
                    break;

                //more.... if, while etc pp
            }
        }

        public static void Accept(this VarDeclStmt d, Visitor v)
        {
            foreach (var local in d.Locals)
            {
                local.Accept(v);
            }
            d.Update.Accept(v);

        }

        public static void Accept(this LocalVariable d, Visitor v)
        {
            v.Visit(d);
            v.Leave(d);
        }

        public static void Accept(this ConcreteUpdateStatement d, Visitor v)
        {

            //lhs ist immer bei allen drei, haben wir drum hier.
            foreach (Expression e in d.Lhss)
            {
                e.Accept(v);
            }

            if (d is UpdateStmt du) du.Accept(v);
            if (d is AssignOrReturnStmt dm) dm.Accept(v);
            if (d is AssignSuchThatStmt dfu) dfu.Accept(v);

        }



        public static void Accept(this Expression e, Visitor v)  //todo splitten
        {
            v.Visit(e);
            v.Leave(e);
        }

        public static void Accept(this UpdateStmt e, Visitor v)
        {
            foreach (AssignmentRhs rhs in e.Rhss)
            {
                rhs.Accept(v);
            }
            
        }

        public static void Accept(this AssignOrReturnStmt e, Visitor v)
        {
            e.Rhs.Accept(v);

        }

        public static void Accept(this AssignSuchThatStmt e, Visitor v)
        {
            e.Expr.Accept(v);
        }

        public static void Accept(this AssignmentRhs d, Visitor v)
        {

            if (d is ExprRhs du) du.Accept(v);
            //havoc und so?
        }

        public static void Accept(this ExprRhs d, Visitor v)
        {

            d.Expr.Accept(v);
            //havoc und so?
        }
    }
}
