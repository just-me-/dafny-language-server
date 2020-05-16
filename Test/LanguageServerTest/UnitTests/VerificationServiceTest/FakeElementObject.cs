using DafnyLanguageServer.Commons;
using DafnyLanguageServer.DafnyAccess;
using Microsoft.Boogie;
using Microsoft.Dafny;

namespace VerificationServiceTest
{
    public class FakeElementObject : DiagnosticElement
    {
        public FakeElementObject(IToken tok, string msg) : base(tok, msg, ErrorLevel.Error)
        {
        }
    }
}
