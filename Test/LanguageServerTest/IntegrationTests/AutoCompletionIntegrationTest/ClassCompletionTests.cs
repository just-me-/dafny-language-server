using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    /// <summary>
    /// This class is testing method suggestion based on a class reference. 
    /// </summary>
    [TestFixture]
    public class ClassCompletionTests : AutoCompletionBase
    {
        [Test]
        public void EmptyClass()
        {
            int line = 10;
            int col = 11;
            var exp = new List<string>() { };

            GetCompletions(Files.ac_c_empty, line, col);
            VerifyCompletions(exp, line, col);
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
            VerifyCompletions(exp, line, col);
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
            VerifyCompletions(exp, line, col);
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
            VerifyCompletions(exp, line, col);
        }

        /* This does not work yet... Only "clean" suggestions are supported server side. Partial is a VSCode feature */
        /*
        [Test]
        public void PartialCorrect()
        {
            int line = 14;
            int col = 15;
            ArrayList exp = new ArrayList()
            {
                "my_match",
            };

            GetCompletions(Files.ac_c_partial, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void PartialNotExists()
        {
            int line = 15;
            int col = 15;
            ArrayList exp = new ArrayList(){};

            GetCompletions(Files.ac_c_partial, line, col);
            VerifyCompletions(exp, line, col);
        }
        */
    }
}