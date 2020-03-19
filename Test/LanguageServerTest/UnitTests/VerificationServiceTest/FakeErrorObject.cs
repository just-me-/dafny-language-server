using DafnyLanguageServer.DafnyAccess;
using Microsoft.Boogie;

namespace VerificationServiceTest
{
    public class FakeErrorObject : DiagnosticError
    {
        public FakeErrorObject(IToken tok, string msg) : base(tok, msg)
        {
        }
    }
}
