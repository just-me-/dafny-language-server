using DafnyLanguageServer.Handler;
using DafnyLanguageServer.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Files = TestCommons.Paths;

namespace CompileHandlerTest
{
    public class CompileIntegrationTests
    {

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
        }

        [Test]
        public void IsFine()
        {
            string dafnyFile = Path.Combine(Files.testFilesPath, Files.cp_fineDLL);

            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;

            Assert.IsFalse(r.Error);
            Assert.IsFalse(r.Executable ?? true);

        }

        [Test]
        public void IsFineExe()
        {
            string dafnyFile = Files.cp_fineEXE;

            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;

            Assert.IsFalse(r.Error);
            Assert.IsTrue(r.Executable ?? false);

        }

        [Test]
        public void Assertion()
        {
            string dafnyFile = Files.cp_assertion;

            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("assertion"));
            Assert.IsTrue(r.Message.Contains("line 7"));

        }

        [Test]
        public void Identifier()
        {
            string dafnyFile = Files.cp_identifier;

            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("unresolved identifier"));
            Assert.IsTrue(r.Message.Contains("line 8"));

        }


        [Test]
        public void Semi()
        {
            string dafnyFile = Files.cp_semiexpected;

            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("semicolon expected"));
            Assert.IsTrue(r.Message.Contains("line 7"));

        }

        [Test]
        public void Postcondition()
        {
            string dafnyFile = Files.cp_postcondition;

            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;

            Assert.IsTrue(r.Error);
            Assert.IsFalse(r.Executable ?? true);
            Assert.IsTrue(r.Message.Contains("postcondition might not hold"));
            Assert.IsTrue(r.Message.Contains("line 4"));

        }


        [Test]
        public void DllCreated()
        {
            string dafnyFile = Files.cp_fineDLL;
            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;
            Assert.IsTrue(File.Exists(Files.cp_out_dll));
        }

        [Test]
        public void ExeCreated()
        {
            string dafnyFile = Files.cp_fineEXE;
            CompilerResults r = new CompilationService(Files.dafnyExe, dafnyFile).Compile().Result;
            Assert.IsTrue(File.Exists(Files.cp_out_exe));
        }
    }

}