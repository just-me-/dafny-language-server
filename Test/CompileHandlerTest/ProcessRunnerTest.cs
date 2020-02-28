using DafnyLanguageServer.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CompileHandlerTest
{
    class ProcessRunnerTest
    {

        private static readonly string testPath = PathConstants.testPath;
        private static readonly string dafnyExe = PathConstants.dafnyExe;

        [SetUp]
        public void DeleteFiles()
        {
            List<string> files = new List<string>
            {
                Path.Combine(testPath, PathConstants.fineDLLOutput),
                Path.Combine(testPath, PathConstants.fineEXEOutput)
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
        public void Test()
        {

            Process p = new Process
            {
                StartInfo =
                    {
                        FileName = dafnyExe,
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
