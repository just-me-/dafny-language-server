using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.HandlerServices;
using NUnit.Framework;
using TestCommons;

namespace CompileIntegrationTest
{
    public class CompileBase : IntegrationTestBase
    {

        protected CompilerResults compilerResults;
        protected const string compileKeyword = "compile";
        protected const string failPrefix = CompilationService.failurePrefix;

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

            //Manual Debug stuff:
            Console.WriteLine("Error = \t" + compilerResults.Error);
            Console.WriteLine("Exec = \t" + compilerResults.Executable);
            Console.WriteLine("Msg = \t" + compilerResults.Message);
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
