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
            VerifyResults(true, false, failPrefix + "[L7:C13] Logical Error: assertion violation");
        }

        [Test]
        public void FailurePostconditionViolation()
        {
            RunCompilation(Files.cp_postcondition);
            VerifyResults(true, false, failPrefix + "[L4:C1] Logical Error: A postcondition might not hold on this return path");
        }


    }
}