using System.Collections.Generic;
using System.Linq;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Boogie;
using Microsoft.Dafny;
using NUnit.Framework;

namespace CoreProviderTest
{
    class DiagnosticsProviderTests
    {
        public class CreateDiagnosticTest
        {
            private static readonly string randomFakeSource = "aa\naa\naa\naa\n";
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
                var errors = new List<FakeElementObject>();
                DiagnosticsProvider diagnosticsService = new DiagnosticsProvider(new FileRepository(new PhysicalFile()));

                var diagnostics = diagnosticsService.ConvertToLSPDiagnostics(errors);
                Assert.AreEqual(0, diagnostics.Count);
            }

            [Test]
            public void TestDiagnosticOneError()
            {
                var errors = new List<FakeElementObject>();
                var info = new FakeElementObject(token, "Msg");
                errors.Add(info);

                DiagnosticsProvider diagnosticsService = new DiagnosticsProvider(new FileRepository(createFakeFile(token.filename, randomFakeSource)));


                var diagnostics = diagnosticsService.ConvertToLSPDiagnostics(errors);

                Assert.AreEqual(1, diagnostics.Count);
                Assert.AreEqual(token.filename, diagnostics[0].Source);
            }

            [Test]
            public void TestDiagnosticSubError()
            {
                var errors = new List<FakeElementObject>();
                var errorObject = new FakeElementObject(token, "Msg");
                errorObject.AddAuxInfo(token, "SubMsg");
                errorObject.AddAuxInfo(token, "SubMsg2");
                errors.Add(errorObject);

                DiagnosticsProvider diagnosticsService = new DiagnosticsProvider(new FileRepository(createFakeFile(token.filename, randomFakeSource)));

                var diagnostics = diagnosticsService.ConvertToLSPDiagnostics(errors);

                Assert.AreEqual(3, diagnostics.Count);
                Assert.IsNull(diagnostics.FirstOrDefault()?.RelatedInformation, "Related Diagnostic should be separate");
            }
        }

        public class FakeElementObject : DiagnosticElement
        {
            public FakeElementObject(IToken tok, string msg) : base(tok, msg, ErrorLevel.Error)
            {
            }
        }
    }
}
