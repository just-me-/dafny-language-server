using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{
    [TestFixture, Timeout(10000)]
    public class FineExamples : VerificationBase
    {
        [Test]
        public void NoDiagnostics1()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_good);
            CollectionAssert.IsEmpty(diagnosticList);
        }

        [Test]
        public void NoDiagnostics2()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_lo_noensure);
            CollectionAssert.IsEmpty(diagnosticList);
        }
    }
}