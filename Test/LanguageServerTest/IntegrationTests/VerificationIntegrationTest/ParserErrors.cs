using System.Collections.Generic;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace VerificationIntegrationTest
{

    [TestFixture]
    public class ParserErrors : VerificationBase
    {

        [Test]
        public void CurlyExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_curly);

            List<string> expct = new List<string>()
            {
                "R[L6 C0 - L6 C39] - Error - Syntax Error: rbrace expected",
                "R[L6 C0 - L6 C39] - Error - Syntax Error: rbrace expected"
            };

            VerifyResults(expct);
        }

        [Test]
        public void EOFExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_eof);

            List<string> expct = new List<string>()
            {
                "R[L4 C1 - L4 C2] - Error - Syntax Error: EOF expected",
                "R[L4 C1 - L4 C2] - Error - Syntax Error: EOF expected"
            };

            VerifyResults(expct);
        }

        [Test]
        public void InvalidNameSegment1()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_ns);

            List<string> expct = new List<string>()
            {
                "R[L3 C8 - L3 C18] - Error - Syntax Error: invalid NameSegment",
                "R[L3 C8 - L3 C18] - Error - Syntax Error: invalid NameSegment"
            };

            VerifyResults(expct);
        }

        [Test]
        public void InvalidUpdateStatement()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_update);

            List<string> expct = new List<string>()
            {
                "R[L3 C2 - L3 C5] - Error - Syntax Error: invalid UpdateStmt",
                "R[L3 C2 - L3 C5] - Error - Syntax Error: invalid UpdateStmt"
            };

            VerifyResults(expct);
        }

        [Test]
        public void InvalidSuffix()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_suffix);

            List<string> expct = new List<string>()
            {
                "R[L3 C5 - L3 C7] - Error - Syntax Error: invalid Suffix",
                "R[L3 C5 - L3 C7] - Error - Syntax Error: invalid Suffix"
            };

            VerifyResults(expct);
        }

        [Test]
        public void ParenthesisExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_parenthesis);

            List<string> expct = new List<string>()
            {
                "R[L4 C0 - L4 C1] - Error - Syntax Error: closeparen expected",
                "R[L4 C0 - L4 C1] - Error - Syntax Error: closeparen expected"
            };

            VerifyResults(expct);
        }

        [Test]
        public void RBraceExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_bracer);

            List<string> expct = new List<string>()
            {
                "R[L3 C20 - L3 C21] - Error - Syntax Error: rbrace expected",
                "R[L3 C20 - L3 C21] - Error - Syntax Error: rbrace expected"
            };

            VerifyResults(expct);
        }

        [Test]
        public void SemiExpected()
        {
            SendRequestAndAwaitDiagnostics(Files.vc_sy_semi);

            List<string> expct = new List<string>()
            {
                "R[L4 C0 - L4 C1] - Error - Syntax Error: semicolon expected",
                "R[L4 C0 - L4 C1] - Error - Syntax Error: semicolon expected"
            };

            VerifyResults(expct);
        }





    }

}