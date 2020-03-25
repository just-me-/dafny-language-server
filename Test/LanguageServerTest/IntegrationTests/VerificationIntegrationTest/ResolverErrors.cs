using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture, Timeout(10000)]
    public class ResolverErrors : VerificationBase
    {

        [Test]
        public void TooManyArguments()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_re_args);

            List<string> expct = new List<string>()
            {
                "R[L9 C13 - L9 C19] - Error - Syntax Error: wrong number of method arguments (got 2, expected 1) at [ ( ]"
            };

            VerifyResults(expct);
        }

        [Test]
        public void UnknownIdentifier()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_re_identifier);

            List<string> expct = new List<string>()
            {
                "R[L4 C3 - L4 C13] - Error - Syntax Error: unresolved identifier: ssss at [ ssss ]"
            };

            VerifyResults(expct);
        }

        [Test]
        public void UnknownType()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_re_type);

            List<string> expct = new List<string>()
            {
                "R[L3 C16 - L3 C22] - Error - Syntax Error: Undeclared top-level type or type parameter: XXX (did you forget to qualify a name or declare a module import 'opened?') at [ XXX ]",
                "R[L3 C9 - L3 C22] - Error - Syntax Error: new can be applied only to reference types (got XXX) at [ := ]"
            };

            VerifyResults(expct);
        }



    }

}