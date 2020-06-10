using NUnit.Framework;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{
    [TestFixture, Timeout(10000)]
    public class LogicalTests : VerificationBase
    {
        [Test]
        public void AssertionFail()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_assertion);

            List<string> expct = new List<string>()
            {
                "R[L6 C15 - L6 C23] - Error - Logical Error: \n assertion violation \n at ==\n "
            };

            VerifyResults(expct);
        }

        [Test]
        public void PostConditionFail()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_postcondition);

            List<string> expct = new List<string>()
            {
                "R[L3 C0 - L6 C1] - Error - Logical Error: \n A postcondition might not hold on this return path\n at Demo",
                "R[L2 C16 - L2 C26] - Information - This is the postcondition that might not hold."
            };

            VerifyResults(expct);
        }

        [Test]
        public void MultipleFails()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_multiplefails);

            List<string> expct = new List<string>()
            {
                "R[L3 C0 - L7 C1] - Error - Logical Error: \n A postcondition might not hold on this return path\n at Demo",
                "R[L2 C16 - L2 C26] - Information - This is the postcondition that might not hold.",
                "R[L6 C15 - L6 C23] - Error - Logical Error: \n assertion violation \n at ==\n "
            };

            VerifyResults(expct);
        }
    }
}