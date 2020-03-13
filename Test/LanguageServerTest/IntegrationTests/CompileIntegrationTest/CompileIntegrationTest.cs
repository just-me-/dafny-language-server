using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;
using Files = TestCommons.Paths;

namespace CompileIntegrationTest
{
    [TestFixture]
    public class Tests
    {

        public TestSetupManager m = new TestSetupManager("Compilation");
        private CompilerResults compilerResults;
        private const string compileKeyword = "compile";
        private const string successMsg = "Compilation successful";

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
            VerifyResults(true, false, "Compilation failed: \"assertion violation\" in line 7.");
        }

        [Test]
        public void FailurePostconditionViolation()
        {
            RunCompilation(Files.cp_postcondition);
            VerifyResults(true, false, "Compilation failed: \"BP5003: A postcondition might not hold on this return path.\" in line 4.");
        }

        [Test]
        public void FailureSyntaxErrorUnknownIdentifier()
        {
            RunCompilation(Files.cp_identifier);
            VerifyResults(true, false, "Compilation failed: \"unresolved identifier: bruder\" in line 8.");
        }

        [Test]
        public void FailureSyntaxErrorSemiExpected()
        {
            RunCompilation(Files.cp_semiexpected);
            VerifyResults(true, false, "Compilation failed: \"semicolon expected\" in line 7.");
        }

        [Test]
        public void Included_File()
        {
            RunCompilation(Files.ic_basic);
            VerifyResults(false, false, successMsg);
        }


        private void RunCompilation(string testfile)
        {
            CompilerParams compilerParams = new CompilerParams
            {
                DafnyFilePath = testfile,
                DafnyExePath = Files.dafnyExe
            };

            compilerResults = m.Client.SendRequest<CompilerResults>(compileKeyword, compilerParams, m.CancellationSource.Token).Result;
        }

        //todo so was null kein file mässiges ticket 210 und was kein dafny ist.

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