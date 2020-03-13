using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class RegularTests
    {

        public TestSetupManager m = new TestSetupManager("Compilation");
        private CompilerResults compilerResults;
        private const string compileKeyword = "compile";
        private const string successMsg = "Compilation successful";
        private const string failMsg = "Compilation failed: ";

        [SetUp]
        public void Setup()
        {
            m.Setup();
            compilerResults = default;
        }

        [TearDown]
        public void TearDown()
        {
            m.TearDown();
        }

        [Test]
        public void SuccessWithExeAsResult()
        {
            RunCompilation(Files.cp_fineEXE);
            VerifyResults(false, true, successMsg);
        }

        [Test]
        public void SuccessWithDllAsResult()
        {
            RunCompilation(Files.cp_fineDLL);
            VerifyResults(false, false, successMsg);
        }

        [Test]
        public void FailureAssertionViolation()
        {
            RunCompilation(Files.cp_assertion);
            VerifyResults(true, false, failMsg + "assertion violation in line 7.");
        }

        [Test]
        public void FailurePostconditionViolation()
        {
            RunCompilation(Files.cp_postcondition);
            VerifyResults(true, false, failMsg + "BP5003: A postcondition might not hold on this return path. in line 4.");
        }

        [Test]
        public void FailureSyntaxErrorUnknownIdentifier()
        {
            RunCompilation(Files.cp_identifier);
            VerifyResults(true, false, failMsg + "unresolved identifier: bruder in line 8.");
        }

        [Test]
        public void FailureSyntaxErrorSemiExpected()
        {
            RunCompilation(Files.cp_semiexpected);
            VerifyResults(true, false, failMsg + "semicolon expected in line 7.");
        }

        [Test]
        public void Included_File()
        {
            RunCompilation(Files.ic_basic);
            VerifyResults(false, false, successMsg);
        }


        [Test]
        public void Inexistant_File()
        {
            RunCompilation(Files.int_inexistant);
            VerifyResults(true, false, failMsg + "Dafny Source File does not exist");
        }

        [Test]
        public void Empty_File()
        {
            RunCompilation(Files.cp_empty);
            VerifyResults(false, false, successMsg);
        }

        [Test]
        public void Otherlang_Java()
        {
            RunCompilation(Files.cp_otherlang_java);
            VerifyResults(true, false, failMsg + "Can only compile .dfy files");
        }

        [Test]
        public void Otherlang_Java_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_java_dfyending);
            VerifyResults(true, false, failMsg + "EOF expected in line 1.");
        }

        [Test]
        public void Otherlang_Py()
        {
            RunCompilation(Files.cp_otherlang_py);
            VerifyResults(true, false, failMsg + "Can only compile .dfy files");
        }

        [Test]
        public void Otherlang_Py_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_py_dfyending);
            VerifyResults(true, false, failMsg + "EOF expected in line 1.");
        }

        [Test]
        public void WithArgumentsNoCompile()
        {
            RunCompilation(Files.cp_fineDLL, new string[] {"/compile:0"});
            VerifyResults(true, false, failMsg + " in line 0.");
        }

        [Test]
        public void WithArgumentsLegit()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/compile:1", "/nologo" });
            VerifyResults(false, false, successMsg);
        }

        [Test]
        public void WithArgumentsGarbage()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/bababutz sagt das kind!" });
            VerifyResults(true, false, failMsg + " in line 0.");
        }



        private void RunCompilation(string testfile, string[] args = null)
        {
            if (args == null)
            {
                args = new string[] {};
            }

            CompilerParams compilerParams = new CompilerParams
            {
                FileToCompile = testfile,
                CompilationArguments = args
            };

            compilerResults = m.Client.SendRequest<CompilerResults>(compileKeyword, compilerParams, m.CancellationSource.Token).Result;
        }

        

        private void VerifyResults(bool expectedError, bool expectedExecutable, string expectedMessage)
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedError, compilerResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, compilerResults.Executable, "Executable Mismatch");
            Assert.AreEqual(expectedMessage, compilerResults.Message);
        }
    }
}