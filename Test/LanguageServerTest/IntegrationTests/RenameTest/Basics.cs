using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace RenameTest
{
    public class Tests : RenameBase
    {

        protected string f = Files.rn_scopes;

        [Test]
        public void RenameAtLocalVarDecl()
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


    }
}