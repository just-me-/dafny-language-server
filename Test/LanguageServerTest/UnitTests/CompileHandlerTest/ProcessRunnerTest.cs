using DafnyLanguageServer.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Files = TestCommons.Paths;

namespace CompileHandlerTest
{
    class ProcessRunnerTest
    {

        [SetUp]
        public void DeleteFiles()
        {
            List<string> files = new List<string>
            {
                Files.cp_out_dll,
                Files.cp_out_exe
            };

            foreach (string f in files)
            {
                if (File.Exists(f))
                {
                    File.Delete(f);
                }
            }
        }

        [Test]
        public void Test()
        {

            Process p = new Process
            {
                StartInfo =
                    {
                        FileName = Files.dafnyExe,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    },
                EnableRaisingEvents = true
            };

            ProcessRunner r = new ProcessRunner(p);

            try
            {
                r.Run();
                Assert.IsTrue(r.IsFinished || r.IsRunning);
            }
            catch
            {
                Assert.Fail();
            }
            finally
            {
                r.Process.Close();
                r.Process.Dispose();
            }

        }
    }
}
