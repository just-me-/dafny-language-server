using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using TestCommons;
using Files = TestCommons.Paths;




namespace VerificationIntegrationTest
{

    [TestFixture]
    public class Tests
    {
        public IntegrationTestBase m = new IntegrationTestBase("Verification");
        private List<string> diagnosticList;
        private readonly System.Threading.EventWaitHandle waitHandle = new System.Threading.AutoResetEvent(false);

        [SetUp]
        public void Setup()
        {
            m.Setup();
            diagnosticList = default;

            PublishDiagnosticsHandler diagnosticsHandler = (uri, diagList) =>
            {
                diagnosticList = diagList.ToStringList();
                waitHandle.Set();
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

        private void SendRequestAndAwaitDiagnostics(string file)
        {
            m.Client.TextDocument.DidOpen(file, "dfy");
            waitHandle.WaitOne();
        }
    }

}