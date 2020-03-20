using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;

namespace VerificationIntegrationTest
{
    public class VerificationBase : IntegrationTestBase
    {
        protected List<string> diagnosticList;
        private readonly System.Threading.EventWaitHandle waitHandle = new System.Threading.AutoResetEvent(false);
        

        public VerificationBase() : base("Verification")
        {
        }

        [SetUp]
        public void CleanResults()
        {
            diagnosticList = default;
        }

        [SetUp]
        public void RegisterDiagnosticHandler()
        {
            PublishDiagnosticsHandler diagnosticsHandler = (uri, diagList) =>
            {
                diagnosticList = diagList.ToStringList();
                waitHandle.Set();
            };

            Client.TextDocument.OnPublishDiagnostics(diagnosticsHandler);
        }


        protected void SendRequestAndAwaitDiagnostics(string file)
        {
            Client.TextDocument.DidOpen(file, "dfy");
            waitHandle.WaitOne();
        }

        protected void VerifyResults(List<string> expct)
        {
            CollectionAssert.AreEquivalent(expct, diagnosticList);
        }
    }
}
