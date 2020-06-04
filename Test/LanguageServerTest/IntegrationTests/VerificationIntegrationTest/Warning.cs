using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture, Timeout(10000)]
    public class Warnings : VerificationBase
    {

        [Test]
        public void Warning_BasedOnInclude()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_warning_include);
            List<string> expct = new List<string>()
            {
                "R[L3 C22 - L3 C23] - Warning - the semi-colon that used to terminate a sub-module declaration has been deprecated; in the new syntax, just leave off the semi-colon \n at ;\n "
            };

            VerifyResults(expct);
        }

        [Test]
        public void ForAllWarning()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_warning);
            List<string> expct = new List<string>()
          {
              "R[L8 C11 - L8 C57] - Warning - /!\\ No terms found to trigger on \n at forall\n ",
              "R[L2 C8 - L2 C45] - Information - Selected triggers: {t in s} \n at forall\n ",
              "R[L0 C7 - L0 C46] - Information - decreases s \n at minimum\n "
          };

            VerifyResults(expct);
        }

        [Test]
        public void Information()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_information);
            List<string> expct = new List<string>()
          {
              "R[L9 C10 - L9 C54] - Information - Selected triggers: {square(i)} \n at forall\n ",
              "R[L15 C16 - L15 C61] - Information - Selected triggers: {square(k)} \n at forall\n "
          };

            VerifyResults(expct);
        }
    }
}