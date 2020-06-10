using NUnit.Framework;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    [TestFixture]
    public class AfterNewTest : AutoCompletionBase
    {
        [Test]
        public void NewInLine()
        {
            int line = 3;
            int col = 18;
            var exp = new List<string>()
            {
                "C()"
            };

            GetCompletions(Files.ac_include_main, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void NewOneSpace()
        {
            int line = 6;
            int col = 12;
            var exp = new List<string>()
            {
                "C()"
            };

            GetCompletions(Files.ac_include_main, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void NewMultipleSpaces()
        {
            int line = 7;
            int col = 14;
            var exp = new List<string>()
            {
                "C()"
            };

            GetCompletions(Files.ac_include_main, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void NewNoSpace()
        {
            int line = 6;
            int col = 11;
            var exp = new List<string>()
            {
                "c",
                "Main",
                "C"
            };

            GetCompletions(Files.ac_include_main, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void NewMultipleClasses()
        {
            int line = 24;
            int col = 8;
            var exp = new List<string>()
            {
                "A()",
                "B()"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp);
        }
    }
}