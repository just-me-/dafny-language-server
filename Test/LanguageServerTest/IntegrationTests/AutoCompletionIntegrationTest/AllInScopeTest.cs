using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    /// <summary>
    /// This class is testing method suggestion based on a scope range. 
    /// </summary>
    [TestFixture]
    public class AllInScopeTest : AutoCompletionBase
    {
        [Test]
        public void OneSimpleVar()
        {
            int line = 3;
            int col = 4;
            var exp = new List<string>()
            {
                "a",
                "Main",
            };
            GetCompletions(Files.ac_basic_var, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void VarAndClassInMain()
        {
            int line = 15;
            int col = 4;
            var exp = new List<string>()
            {
                "c",
                "acc1",
                "acc2",
                "Main",
                "C"
            };

            GetCompletions(Files.ac_basic_var_class, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void MultipleClasses()
        {
            int line = 22;
            int col = 4;
            var exp = new List<string>()
            {
                "c",
                "Main",
                "A",
                "B"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void Empty()
        {
            int line = 1;
            int col = 1;

            GetCompletions(Files.ac_empty, line, col);
            var exp = new List<string>();
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void FirstPosition()
        {
            int line = 1;
            int col = 1;
            var exp = new List<string>()
            {
                "A",
                "B",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void FirstInClassScope()
        {
            int line = 1;
            int col = 10;
            var exp = new List<string>()
            {
                "A",
                "B",
                "m"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void InClassMethod()
        {
            int line = 7;
            int col = 20;
            var exp = new List<string>()
            {
                "A",
                "am",
                "B",
                "m"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void LastInClassMethod()
        {
            int line = 8;
            int col = 4;
            var exp = new List<string>()
            {
                "A",
                "am",
                "B",
                "m"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void OutOfClassMethod()
        {
            int line = 8;
            int col = 5;
            var exp = new List<string>()
            {
                "A",
                "B",
                "m"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void LastInClass()
        {
            int line = 9;
            int col = 3;
            var exp = new List<string>()
            {
                "A",
                "B",
                "m"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void OutOfClass()
        {
            int line = 9;
            int col = 4;
            var exp = new List<string>()
            {
                "A",
                "B",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void LastInMain()
        {
            int line = 23;
            int col = 1;
            var exp = new List<string>()
            {
                "A",
                "B",
                "c",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void FirstInMain()
        {
            int line = 20;
            int col = 16;
            var exp = new List<string>()
            {
                "A",
                "B",
                "c",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        //[Test]
        public void FirstNotInMain()
        {
            int line = 20;
            int col = 15;
            var exp = new List<string>()
            {
                "A",
                "B",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void FirstCharOnLineNotInMain()
        {
            int line = 20;
            int col = 1;
            var exp = new List<string>()
            {
                "A",
                "B",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void LineBeforeMain()
        {
            int line = 19;
            int col = 1;
            var exp = new List<string>()
            {
                "A",
                "B",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void DefaultScope()
        {
            int line = 25;
            int col = 1;
            var exp = new List<string>()
            {
                "A",
                "B",
                "Main"
            };

            GetCompletions(Files.ac_multiple_classes, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void IncludedDefaultScope()
        {
            int line = 9;
            int col = 1;
            var exp = new List<string>()
            {
                "C",
                "Main"
            };

            GetCompletions(Files.ac_include_main, line, col);
            VerifyCompletions(exp, line, col);
        }
    }
}