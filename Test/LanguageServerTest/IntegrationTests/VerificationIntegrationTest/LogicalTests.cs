using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture]
    public class Tests
    {
        public TestSetupManager m = new TestSetupManager("Verification");
        private bool diagnosticsHaveArrived = false;
        private List<string> diagnosticList;

        [SetUp]
        public void Setup()
        {
            m.Setup();
            diagnosticList = default;

            PublishDiagnosticsHandler diagnosticsHandler = (uri, diagList) =>
            {
                diagnosticList = diagList.ToStringList();
                diagnosticsHaveArrived = true;
            };

            m.Client.TextDocument.OnPublishDiagnostics(diagnosticsHandler);
        }

        [TearDown]
        public void TearDown()
        {
            m.TearDown();
        }


        [Test]
        public void NoDiagnostics1()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_good);

            CollectionAssert.IsEmpty(diagnosticList);
        }

        [Test]
        public void NoDiagnostics2()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_noensure);

            CollectionAssert.IsEmpty(diagnosticList);

        }


        [Test]
        public void AssertionFail()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_assertion);

            List<string> expct = new List<string>()
            {
                "R[L6 C15 - L6 C23] - Error - assertion violation - Hint: =="    //Todo dieses hint teil ist ja auch übelst behindert. das == wird ja underlined.
            };

            CollectionAssert.AreEquivalent(expct, diagnosticList);
        }

        [Test]
        public void PostConditionFail()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_postcondition);

            List<string> expct = new List<string>()
            {
                "R[L2 C16 - L2 C26] - Warning - This is the postcondition that might not hold.",
                "R[L3 C0 - L3 C1] - Error - A postcondition might not hold on this return path. - Hint: {"
            };



            CollectionAssert.AreEquivalent(expct, diagnosticList);
        }

        [Test]
        public void MultipleFails()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_multiplefails);

            List<string> expct = new List<string>()
            {
                "R[L2 C16 - L2 C26] - Warning - This is the postcondition that might not hold.",
                "R[L3 C0 - L3 C1] - Error - A postcondition might not hold on this return path. - Hint: {",
                "R[L6 C15 - L6 C23] - Error - assertion violation - Hint: =="
            };

            CollectionAssert.AreEquivalent(expct, diagnosticList);
        }


        private void WaitForDiangostics()
        {

            double waitedTime = 0.1;
            while (!diagnosticsHaveArrived && waitedTime < 5)
            {
                Thread.Sleep(100);
                waitedTime += 0.1;
            }

            diagnosticsHaveArrived = false;

            if (waitedTime >= 5)
            {
                throw new Exception("Waited 5 seconds for diagnostics, but they didn't arrive");
            }
        }

        private void SendRequestAndAwaitDiagnostics(string file)
        {
            m.Client.TextDocument.DidOpen(file, "dfy");
            WaitForDiangostics();
        }
    }

}