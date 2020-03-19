using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture]
    public class PraserErrors : VerificationBase
    {


        [Test]
        public void SemiExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_assertion);

            List<string> expct = new List<string>()
            {
                "R[L6 C15 - L6 C23] - Error - Logical Error: assertion violation at [ == ]"
            };

            VerifyResults(expct);
        }


        [Test]
        public void BracketExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_assertion);

            List<string> expct = new List<string>()
            {
                "R[L6 C15 - L6 C23] - Error - Logical Error: assertion violation at [ == ]"
            };

            VerifyResults(expct);
        }

        [Test]
        public void CurlyExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_assertion);

            List<string> expct = new List<string>()
            {
                "R[L6 C15 - L6 C23] - Error - Logical Error: assertion violation at [ == ]"
            };

            VerifyResults(expct);
        }






    }

}