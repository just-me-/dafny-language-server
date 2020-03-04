using DafnyLanguageServer.Services;
using Microsoft.Boogie;
using NUnit.Framework;
using System.Collections.Generic;

namespace VerificationServiceTest
{

    public class CreateDiagnosticTest
    {
        private static readonly string randomFakeSource = "aa\naa\naa\naa\n";
        private static VerificationService verificationService = new VerificationService(null, null); //2do
        private Token token;

        [SetUp]
        public void setupExampleToken()
        {
            token = new Token();
            token.filename = "FakedFile";
            token.val = "This would be an error description";
            token.kind = token.pos = token.line = token.col = token.line = 3;
        }

        [Test]
        public void TestDiagnosticNoErrors()
        {
            var errors = new List<FakeErrorObject>();
            var diagnostics = verificationService.CreateDafnyDiagnostics(errors, "NotExistingFile", randomFakeSource);
            Assert.AreEqual(0, diagnostics.Count);
        }

        [Test]
        public void TestDiagnosticOneError()
        {
            var errors = new List<FakeErrorObject>();
            var info = new FakeErrorObject(token, "Msg");
            errors.Add(info);

            var diagnostics = verificationService.CreateDafnyDiagnostics(errors, token.filename, randomFakeSource);

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(token.filename, diagnostics[0].Source);
        }

        [Test]
        public void TestDiagnosticSubError()
        {
            var errors = new List<FakeErrorObject>();
            var info = new FakeErrorObject(token, "Msg");
            info.AddAuxInfo(token, "SubMsg");
            info.AddAuxInfo(token, "TraceData");
            errors.Add(info);

            var diagnostics = verificationService.CreateDafnyDiagnostics(errors, token.filename, randomFakeSource);

            Assert.AreEqual(2, diagnostics.Count);
        }
    }
}
