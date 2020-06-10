using NUnit.Framework;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    [TestFixture]
    public class AfterDotTest : AutoCompletionBase
    {
        [Test]
        public void EmptyClass()
        {
            int line = 10;
            int col = 11;
            var exp = new List<string>() { };

            GetCompletions(Files.ac_c_empty, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void ClassWithOneMethod()
        {
            int line = 14;
            int col = 11;
            var exp = new List<string>()
            {
                "meth()",
            };

            GetCompletions(Files.ac_c_one_method, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void MultipleClasses()
        {
            int line = 24;
            int col = 11;
            var exp = new List<string>()
            {
                "right()",
            };

            GetCompletions(Files.ac_c_multiple, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void ClassInAClass()
        {
            int line = 18;
            int col = 14;
            var exp = new List<string>()
            {
                "method_c()",
            };

            GetCompletions(Files.ac_c_in_class, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void NoConstructor()
        {
            int line = 14;
            int col = 11;
            var exp = new List<string>()
            {
                "my_match()",
            };

            GetCompletions(Files.ac_c_partial, line, col);
            VerifyCompletions(exp);
        }

        [Test]
        public void OtherFile()
        {
            int line = 4;
            int col = 14;
            var exp = new List<string>()
            {
                "field",
                "increase()",
            };

            GetCompletions(Files.ac_include_main, line, col);
            VerifyCompletions(exp);
        }
    }
}