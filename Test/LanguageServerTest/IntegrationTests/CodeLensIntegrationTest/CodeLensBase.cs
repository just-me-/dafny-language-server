using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;

namespace CompileIntegrationTest
{
    public class CodeLensBase : IntegrationTestBase
    {

        protected CompilerResults codelensResults;
        protected const string codelensKeyword = "codelens";

        public CodeLensBase() : base("CodeLens")
        {
        }

        [SetUp]
        public void CleanResults()
        {
            codelensResults = null;
        }


        protected void RunCodeLens(string testfile, string[] args = null)
        {
            if (args == null)
            {
                args = new string[] { };
            }

            CompilerParams compilerParams = new CompilerParams
            {
                FileToCompile = testfile,
                CompilationArguments = args
            };

            Client.TextDocument.DidOpen(testfile, "dfy");
            codelensResults = Client.SendRequest<CompilerResults>(codelensKeyword, compilerParams, CancellationSource.Token).Result;

            PrintResults();
        }

        private void PrintResults()
        {
            StringBuilder debugMsg = new StringBuilder();
            debugMsg.AppendLine("Printing Compilation Test Results:");
            debugMsg.AppendLine("Error = \t" + codelensResults.Error);
            debugMsg.AppendLine("Exec = \t" + codelensResults.Executable);
            debugMsg.AppendLine("Msg = \t" + codelensResults.Message);
        }


        protected void VerifyResults(bool expectedError, bool expectedExecutable, string expectedMessage)
        {
            if (codelensResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedMessage, codelensResults.Message);
            Assert.AreEqual(expectedError, codelensResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, codelensResults.Executable, "Executable Created Mismatch");
        }

        protected void VerifyLoosely(bool expectedError, bool expectedExecutable, string expectedMessage = "")
        {
            if (codelensResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.IsTrue(codelensResults.Message.Contains(expectedMessage), $"Msg not contained:  Expected: {expectedMessage}. Is: {codelensResults.Message}");
            Assert.AreEqual(expectedError, codelensResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, codelensResults.Executable, "Executable Created Mismatch");
        }
    }
}
