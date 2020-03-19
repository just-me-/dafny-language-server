using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture]
    public class LogicalTests : VerificationBase
    {

        [Test]
        public void AssertionFail()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_assertion);

            List<string> expct = new List<string>()
            {
                "R[L6 C15 - L6 C23] - Error - Logical Error: assertion violation at [ == ]"
            };

            VerifyResults(expct);
        }

        [Test]
        public void PostConditionFail()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_postcondition);

            List<string> expct = new List<string>()
            {
                "R[L3 C0 - L3 C1] - Error - Logical Error: A postcondition might not hold on this return path at [ { ] Related Information: R[L2 C16 - L2 C26] This is the postcondition that might not hold."
            };

            VerifyResults(expct);
        }

        [Test]
        public void MultipleFails()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_multiplefails);

            List<string> expct = new List<string>()
            {
                "R[L3 C0 - L3 C1] - Error - Logical Error: A postcondition might not hold on this return path at [ { ] Related Information: R[L2 C16 - L2 C26] This is the postcondition that might not hold.",
                "R[L6 C15 - L6 C23] - Error - Logical Error: assertion violation at [ == ]"
            };

            VerifyResults(expct);
        }


    }

}