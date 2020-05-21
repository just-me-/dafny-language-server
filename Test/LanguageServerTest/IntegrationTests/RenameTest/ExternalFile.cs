using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace RenameTest
{

    public class ExternalFile : RenameBase
    {

        protected string f = Files.rn_include_main;

        [Test]
        public void ExternalClass()
        {
            Run(f, 3, 18);

            List<string> expected = new List<string>()
            {
                "newText at L0:C6 - L0:C7 in include_includee.dfy",
                "newText at L2:C17 - L2:C18 in include_main.dfy"
            };
            VerifyForMultiFiles(expected);

        }

        [Test]
        public void ExternalClassMethod()
        {
            Run(f, 4, 18);
            List<string> expected = new List<string>()
            {
                "newText at L2:C20 - L2:C28 in include_includee.dfy",
                "newText at L3:C13 - L3:C21 in include_main.dfy"
            };
            VerifyForMultiFiles(expected);
        }


        [Test]
        public void ExternalClassField()
        {
            Run(f, 5, 7);
            List<string> expected = new List<string>()
            {
                "newText at L5:C8 - L5:C13 in include_includee.dfy",
                "newText at L4:C6 - L4:C11 in include_main.dfy"
            };
            VerifyForMultiFiles(expected);
        }

    }
}