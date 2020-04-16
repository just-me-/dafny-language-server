using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture, Timeout(10000)]
    public class Warnings : VerificationBase
    {

        [Test]
        public void Warning()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_warning);
            List<string> expct = new List<string>()
            {
                "R[L3 C22 - L3 C23] - Warning - the semi-colon that used to terminate a sub-module declaration has been deprecated; in the new syntax, just leave off the semi-colon at [ ; ]"
            };

            VerifyResults(expct);
        }




    }

}