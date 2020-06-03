using System.Text;
using DafnyLanguageServer.CustomDTOs;
using NUnit.Framework;
using TestCommons;

namespace CompileIntegrationTest
{
    public class CompileBase : IntegrationTestBase
    {

        protected CompilerResults compilerResults;
        protected const string compileKeyword = "compile";
        protected readonly string failPrefix = DafnyLanguageServer.Resources.CompilationResults.compilation_failed;

        public CompileBase() : base("Compile")
        {
        }

        [SetUp]
        public void CleanResults()
        {
            compilerResults = null;
        }


        protected void RunCompilation(string testfile, string[] args = null)
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
            compilerResults = Client.SendRequest<CompilerResults>(compileKeyword, compilerParams, CancellationSource.Token).Result;

            PrintResults();
        }

        private void PrintResults()
        {
            StringBuilder debugMsg = new StringBuilder();
            debugMsg.AppendLine("Printing Compilation Test Results:");
            debugMsg.AppendLine("Error = \t" + compilerResults.Error);
            debugMsg.AppendLine("Exec = \t" + compilerResults.Executable);
            debugMsg.AppendLine("Msg = \t" + compilerResults.Message);
            MyLog.Debug(debugMsg.ToString());
        }


        protected void VerifyResults(bool expectedError, bool expectedExecutable, string expectedMessage)
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedMessage, compilerResults.Message);
            Assert.AreEqual(expectedError, compilerResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, compilerResults.Executable, "Executable Created Mismatch");
        }

        protected void VerifyLoosely(bool expectedError, bool expectedExecutable, string expectedMessage = "")
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.IsTrue(compilerResults.Message.Contains(expectedMessage), $"Msg not contained:  Expected: {expectedMessage}. Is: {compilerResults.Message}");
            Assert.AreEqual(expectedError, compilerResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, compilerResults.Executable, "Executable Created Mismatch");
        }


    }
}
