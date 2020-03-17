using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class SyntaxErrors : CompileBase
    {

        [Test]
        public void FailureSyntaxErrorUnknownIdentifier()
        {
            RunCompilation(Files.cp_identifier);
            VerifyResults(true, false, failMsg + "unresolved identifier: bruder in line 8.");
        }

        [Test]
        public void FailureSyntaxErrorSemiExpected()
        {
            RunCompilation(Files.cp_semiexpected);
            VerifyResults(true, false, failMsg + "semicolon expected in line 7.");
        }


    }
}