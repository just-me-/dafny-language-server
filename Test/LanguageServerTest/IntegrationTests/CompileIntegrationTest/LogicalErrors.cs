using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class LogicalErrors : CompileBase
    {


        [Test]
        public void FailureAssertionViolation()
        {
            RunCompilation(Files.cp_assertion);
            VerifyResults(true, false, failMsg + "assertion violation in line 7.");
        }

        [Test]
        public void FailurePostconditionViolation()
        {
            RunCompilation(Files.cp_postcondition);
            VerifyResults(true, false, failMsg + "BP5003: A postcondition might not hold on this return path. in line 4.");
        }


    }
}