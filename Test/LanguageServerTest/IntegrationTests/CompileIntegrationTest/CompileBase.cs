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
    public class CompileBase : IntegrationTestBase
    {

        protected CompilerResults compilerResults;
        protected const string compileKeyword = "compile";
        protected const string successMsg = "Compilation successful";
        protected const string failMsg = "Compilation failed: ";

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

            compilerResults = Client.SendRequest<CompilerResults>(compileKeyword, compilerParams, CancellationSource.Token).Result;
        }


        protected void VerifyResults(bool expectedError, bool expectedExecutable, string expectedMessage)
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedError, compilerResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, compilerResults.Executable, "Executable Created Mismatch");
            Assert.AreEqual(expectedMessage, compilerResults.Message);
        }

        protected void VerifyLoosely(bool expectedError, bool expectedExecutable, string expectedMessage = "")
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedError, compilerResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, compilerResults.Executable, "Executable Created Mismatch");
            Assert.IsTrue(compilerResults.Message.Contains(expectedMessage), $"Msg not contained:  Expected: {expectedMessage}. Is: {compilerResults.Message}");
        }


    }
}
