using System;
using DafnyLanguageServer.Services;
using Microsoft.Boogie;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DafnyLanguageServer.ContentManager;

namespace VerificationServiceTest
{

    public class CreateDiagnosticTest
    {
        private static readonly string randomFakeSource = "aa\naa\naa\naa\n";
        private static DiagnosticsService diagnosticsService = new DiagnosticsService(null); //todo Ticket#281
        private Token token;

        private PhysicalFile createFakeFile(string name, string code)
        {
            return new PhysicalFile()
            {
                Filepath = name,
                Sourcecode = code
            };
        }

        [SetUp]
        public void setupExampleToken()
        {
            token = new Token();
            token.filename = "Random Filename";
            token.val = "This would be an error description";
            token.kind = token.pos = token.line = token.col = token.line = 3;
        }

        [Test]
        public void TestDiagnosticNoErrors()
        {
            var errors = new List<FakeErrorObject>();
            var diagnostics = diagnosticsService.CreateDafnyDiagnostics(errors, createFakeFile("NotExistingFile", randomFakeSource));
            Assert.AreEqual(0, diagnostics.Count);
        }

        [Test]
        public void TestDiagnosticOneError()
        {
            var errors = new List<FakeErrorObject>();
            var info = new FakeErrorObject(token, "Msg");
            errors.Add(info);

            var diagnostics = diagnosticsService.CreateDafnyDiagnostics(errors, createFakeFile(token.filename, randomFakeSource));

            Assert.AreEqual(1, diagnostics.Count);
            Assert.AreEqual(token.filename, diagnostics[0].Source);
        }

        [Test]
        public void TestDiagnosticSubError()
        {
            var errors = new List<FakeErrorObject>();
            var errorObject = new FakeErrorObject(token, "Msg");
            errorObject.AddAuxInfo(token, "SubMsg");
            errorObject.AddAuxInfo(token, "SubMsg2");
            errors.Add(errorObject);

            var diagnostics = diagnosticsService.CreateDafnyDiagnostics(errors, createFakeFile(token.filename, randomFakeSource));

            Assert.AreEqual(3, diagnostics.Count);
            Assert.IsNull(diagnostics.FirstOrDefault()?.RelatedInformation, "Related Diagnostic should be separate");
        }
    }
}
