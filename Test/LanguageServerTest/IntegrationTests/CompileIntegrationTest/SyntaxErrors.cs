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
            VerifyResults(true, false, failPrefix + "[L8:C4] Syntax Error: \n unresolved identifier: bruder");
        }

        [Test]
        public void FailureSyntaxErrorSemiExpected()
        {
            RunCompilation(Files.cp_semiexpected);
            VerifyResults(true, false, failPrefix + "[L7:C4] Syntax Error: \n semicolon expected");
        }
    }
}