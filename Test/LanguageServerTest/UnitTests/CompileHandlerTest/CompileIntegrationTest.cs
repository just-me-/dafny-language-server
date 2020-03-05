using DafnyLanguageServer.Handler;
using DafnyLanguageServer.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Files = PathConstants.Paths;

namespace CompileHandlerTest
{
    public class CompileIntegrationTests
    {

        private static readonly string testPath = Files.testFilesPath;
        private static readonly string dafnyExe = Files.dafnyExe;

        [SetUp]
        public void DeleteFiles()
        {
            List<string> files = new List<string>
            {
                Path.Combine(testPath, Files.cp_out_dll),
                Path.Combine(testPath, Files.cp_out_exe)
            };

            foreach (string path in files)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Test]
        public void IsFine()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_fineDLL);

            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;

            Assert.IsFalse(r.Error);
            Assert.IsFalse(r.Executable ?? true);

        }

        [Test]
        public void IsFineExe()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_fineEXE);

            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;

            Assert.IsFalse(r.Error);
            Assert.IsTrue(r.Executable ?? false);

        }

        [Test]
        public void Assertion()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_assertion);

            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("assertion"));
            Assert.IsTrue(r.Message.Contains("line 7"));

        }

        [Test]
        public void Identifier()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_identifier);

            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("unresolved identifier"));
            Assert.IsTrue(r.Message.Contains("line 8"));

        }

        [Test]
        public void Postcondition()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_postcondition);

            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("postcondition might not hold"));
            Assert.IsTrue(r.Message.Contains("line 4"));

        }


        [Test]
        public void DllCreated()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_fineDLL);
            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;
            Assert.IsTrue(File.Exists(Path.Combine(testPath, Files.cp_out_dll)));
        }

        [Test]
        public void ExeCreated()
        {
            string dafnyFile = Path.Combine(testPath, Files.cp_fineEXE);
            CompilerResults r = new CompilationService(dafnyExe, dafnyFile).Compile().Result;
            Assert.IsTrue(File.Exists(Path.Combine(testPath, Files.cp_out_exe)));
        }
    }

}