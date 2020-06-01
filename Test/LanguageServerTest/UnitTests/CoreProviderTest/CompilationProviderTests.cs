using System;
using System.Collections.Generic;
using System.IO;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Boogie;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace CoreProviderTest
{
    [TestFixture]
    [Category("Unit")]
    public class CompilationProviderTest
    {
        private CompilerResults compilerResults;
        private readonly string failurePrefix = DafnyLanguageServer.Resources.CompilationResults.compilation_failed;

        [SetUp]
        public void DeleteFiles()
        {
            List<string> files = new List<string>
            {
                Files.cp_out_dll,
                Files.cp_out_exe
            };

            foreach (string path in files)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            compilerResults = null;
        }

        [Test]
        public void SuccessWithExeAsResult()
        {
            RunCompilation(Files.cp_fineEXE);
            VerifyResults(false, true, "Compiled assembly into compiles_as_exe.exe");
        }

        [Test]
        public void SuccessWithDllAsResult()
        {
            RunCompilation(Files.cp_fineDLL);
            VerifyResults(false, false, "Compiled assembly into compiles_as_dll.dll");
        }

        [Test]
        public void FailureAssertionViolation()
        {
            RunCompilation(Files.cp_assertion);
            VerifyResults(true, false, failurePrefix + "[L7:C13] Logical Error: assertion violation");
        }

        [Test]
        public void FailurePostconditionViolation()
        {
            RunCompilation(Files.cp_postcondition);
            VerifyResults(true, false, failurePrefix + "[L4:C1] Logical Error: A postcondition might not hold on this return path.");
        }

        [Test]
        public void FailureSyntaxErrorUnknownIdentifier()
        {
            RunCompilation(Files.cp_identifier);
            VerifyResults(true, false, failurePrefix + "[L8:C4] Syntax Error: unresolved identifier: bruder");
        }

        [Test]
        public void FailureSyntaxErrorSemiExpected()
        {
            RunCompilation(Files.cp_semiexpected);
            VerifyResults(true, false, failurePrefix + "[L7:C4] Syntax Error: semicolon expected");
        }

        [Test]
        public void Included_File()
        {
            RunCompilation(Files.cp_include_main);
            VerifyResults(false, true, "Compiled assembly into include_main.exe");
        }


        [Test]
        public void Inexistant_File()
        {
            Assert.Throws<FileNotFoundException>(() => RunCompilation(Files.int_inexistant));
        }

        [Test]
        public void Empty_File()
        {
            RunCompilation(Files.cp_empty);
            VerifyResults(false, false, "Compiled assembly into empty.dll");
        }

        [Test]
        public void Otherlang_Java()
        {
            RunCompilation(Files.cp_otherlang_java);
            VerifyResults(true, false, failurePrefix + "Can only compile .dfy files.");
        }

        [Test]
        public void Otherlang_Java_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_java_dfyending);
            VerifyResults(true, false, failurePrefix + "[L1:C1] Syntax Error: EOF expected");
        }

        [Test]
        public void Otherlang_Py()
        {
            RunCompilation(Files.cp_otherlang_py);
            VerifyResults(true, false, failurePrefix + "Can only compile .dfy files.");
        }

        [Test]
        public void Otherlang_Py_DfyEnding()
        {
            RunCompilation(Files.cp_otherlang_py_dfyending);
            VerifyResults(true, false, failurePrefix + "[L1:C1] Syntax Error: EOF expected");
        }

        [Test]
        public void WithArgumentsNoCompile()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/compile:0" });
            VerifyResultsLoosely(true, false);
        }

        [Test]
        public void WithArgumentsLegit()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/compile:1", "/nologo" });
            VerifyResults(false, false, "Compiled assembly into compiles_as_dll.dll");
        }

        [Test]
        public void WithArgumentsGarbage()
        {
            RunCompilation(Files.cp_fineDLL, new string[] { "/bababutz sagt das kind!" });
            VerifyResultsLoosely(true, false);
        }


        private void RunCompilation(string filePath, string[] args = null)
        {

            if (args == null)
            {
                args = new string[] { };
            }

            var physFile = new PhysicalFile()
            {
                Filepath = filePath,
                Sourcecode = File.ReadAllText(filePath),
                Uri = new Uri(filePath)
            };

            ExecutionEngine.printer = new ConsolePrinter();

            var results = new DafnyTranslationUnit(physFile).Verify();

            var repo = new FileRepository(physFile, results);

            compilerResults = new CompileProvider(repo, args).Compile();
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


        private void VerifyResultsLoosely(bool expectedError, bool expectedExecutable, string expectedMessage = "")
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedError, compilerResults.Error, "CompilationError Mismatch");
            Assert.AreEqual(expectedExecutable, compilerResults.Executable, "Executable Created Mismatch");
            Assert.IsTrue(compilerResults.Message.Contains(expectedMessage), $"Message not contained. Expected: {expectedMessage}. Is: {compilerResults.Message}");
        }
    }

}