using System.Collections;
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
            ArrayList exp = new ArrayList()
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
            ArrayList exp = new ArrayList()
            {
                "a",
                "b",
                "m",
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
            int line = 15;
            int col = 4;
            ArrayList exp = new ArrayList()
            {
                "a",
                "am",
                "m",
                "b",
                "bm",
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
            int line = 15;
            int col = 4;
            ArrayList exp = new ArrayList(){};

            GetCompletions(Files.ac_empty, line, col);
            VerifyCompletions(exp, line, col);
        }
    }
}