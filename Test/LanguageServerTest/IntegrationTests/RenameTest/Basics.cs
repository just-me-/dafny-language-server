using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace RenameTest
{

    public class BasicTests : RenameBase
    {

        protected string f = Files.rn_scopes;

        [Test]
        public void LocalVarDecl()
        {
            Run(f, 8, 13);
            List<string> expected = new List<string>()
            {
                "newText at L7:C12 - L7:C15",
                "newText at L8:C14 - L8:C17",
                "newText at L12:C18 - L12:C21",
                "newText at L20:C14 - L20:C17"
            };
            VerifyForSingleFile(expected);
        }

        [Test]
        public void LocalVarUsage()
        {
            Run(f, 9, 17);
            List<string> expected = new List<string>()
            {
                "newText at L7:C12 - L7:C15",
                "newText at L8:C14 - L8:C17",
                "newText at L12:C18 - L12:C21",
                "newText at L20:C14 - L20:C17"
            };
            VerifyForSingleFile(expected);
        }






        [Test]
        public void DeepBlockLocalVarDecl()
        {
            Run(f, 16, 17);
            List<string> expected = new List<string>()
            {
                "newText at L15:C16 - L15:C19",
                "newText at L16:C18 - L16:C21"
            };
            VerifyForSingleFile(expected);
        }
        
        [Test]
        public void DeepBlockLocalVarUsage()
        {
            Run(f, 17, 19);
            List<string> expected = new List<string>()
            {
                "newText at L15:C16 - L15:C19",
                "newText at L16:C18 - L16:C21"
            };
            VerifyForSingleFile(expected);
        }





        //GetSymbol failed.. infet offenbar keine declarations.
        [Test]
        public void ClassVarDecl()
        {
            Run(f, 2, 10);
            List<string> expected = new List<string>()
            {
                "newText at L1:C8 - L1:C11",
                "newText at L4:C8 - L4:C11",
                "newText at L5:C14 - L5:C17",
                "newText at L9:C19 - L9:C22",
                "newText at L13:C23 - L13:C26",
                "newText at L17:C23 - L17:C26",
                "newText at L21:C19 - L21:C22"
            };
            VerifyForSingleFile(expected);
        }

        [Test]
        public void ClassVarUsage()
        {
            Run(f, 14, 24);
            List<string> expected = new List<string>()
            {
                "newText at L1:C8 - L1:C11",
                "newText at L4:C8 - L4:C11",
                "newText at L5:C14 - L5:C17",
                "newText at L9:C19 - L9:C22",
                "newText at L13:C23 - L13:C26",
                "newText at L17:C23 - L17:C26",
                "newText at L21:C19 - L21:C22"
            };
            VerifyForSingleFile(expected);
        }






        [Test]
        public void ClassAtThisExpr()
        {
            Run(f, 10, 15);
            List<string> expected = new List<string>()
            {
                "newText at L0:C6 - L0:C13",
                "newText at L28:C17 - L28:C24"
            };
            VerifyForSingleFile(expected);
        }

        //GetSymbolAtPosition Fails here
        [Test]
        public void ClassAtDecl()
        {
            Run(f, 1, 7);
            List<string> expected = new List<string>()
            {
                "newText at L0:C6 - L0:C13",
                "newText at L28:C17 - L28:C24"
            };
            VerifyForSingleFile(expected);
        }

        [Test]
        public void ClassAtNewExpr()
        {
            Run(f, 29, 18);
            List<string> expected = new List<string>()
            {
                "newText at L0:C6 - L0:C13",
                "newText at L28:C17 - L28:C24"
            };
            VerifyForSingleFile(expected);
        }









        //GetSymbol failed
        [Test]
        public void ClassMethodAtDecl()
        {
            Run(f, 4, 12);
            List<string> expected = new List<string>()
            {
                "newText at L0:C6 - L0:C13",
                "newText at L28:C17 - L28:C24"
            };
            VerifyForSingleFile(expected);
        }


        [Test]
        public void ClassMethodAtUsage()
        {
            Run(f, 30, 7);
            List<string> expected = new List<string>()
            {
                "newText at L3:C11 - L3:C18",
                "newText at L29:C6 - L29:C13"
            };
            VerifyForSingleFile(expected);
        }



        [Test]
        public void GlobalMethodAtDecl()
        {
            Run(f, 34, 8);
            List<string> expected = new List<string>()
            {
                "newText at L33:C7 - L33:C18",
                "newText at L30:C1 - L30:C12"
            };
            VerifyForSingleFile(expected);
        }

        [Test]
        public void GlobalMethodAtUsage()
        {
            Run(f, 31, 5);
            List<string> expected = new List<string>()
            {
                "newText at L33:C7 - L33:C18",
                "newText at L30:C1 - L30:C12"
            };
            VerifyForSingleFile(expected);
        }


    }
}