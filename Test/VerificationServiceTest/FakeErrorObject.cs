using Microsoft.Boogie;

namespace VerificationServiceTest
{
    public class FakeErrorObject : ErrorInformation
    {
        public FakeErrorObject(IToken tok, string msg) : base(tok, msg)
        {
        }
    }
}
