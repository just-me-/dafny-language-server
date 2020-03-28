using System.Collections;
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
            ArrayList exp = new ArrayList(){ };

            GetCompletions(Files.ac_c_empty, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void ClassWithOneMethod()
        {
            int line = 14;
            int col = 11;
            ArrayList exp = new ArrayList()
            {
                "meth",
            };

            GetCompletions(Files.ac_c_one_method, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void MultipleClasses()
        {
            int line = 24;
            int col = 11;
            ArrayList exp = new ArrayList()
            {
                "right",
            };

            GetCompletions(Files.ac_c_multiple, line, col);
            VerifyCompletions(exp, line, col);
        }

        [Test]
        public void ClassInAClass()
        {
            int line = 18;
            int col = 13;
            ArrayList exp = new ArrayList()
            {
                "method_c",
            };

            GetCompletions(Files.ac_ac, line, col);
            VerifyCompletions(exp, line, col);
        }


        // eine methode vorschlagen nach .
        // eine methode vorschlagen nach .me
        // es gibt keine methode .
        // es gibt methode aber passt nicht .meh

    }
}