using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace AutoCompletionIntegrationTest
{
    /// <summary>
    /// This tests if in a basic auto completion on a new line every found symbol in the Dafny file space gets suggested. 
    /// </summary>
    [TestFixture]
    public class BasicCompletionTests : AutoCompletionBase
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
            Assert.IsNull(completions);
        }
    }
}