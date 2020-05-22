using DafnyLanguageServer.Handler;
using NUnit.Framework;
using System;
using System.Diagnostics;
using TestCommons;
using Files = TestCommons.Paths;

namespace PerformanceTest
{
    public class Tests : IntegrationTestBase
    {

        public Tests() : base("Performance")
        {
        }

        [Test]
        public void Test()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Client.TextDocument.DidOpen(Files.pf_large, "dfy");

            CompilerParams p = new CompilerParams
            {
                CompilationArguments = new string[] { },
                FileToCompile = Files.pf_large
            };
            Client.SendRequest<CompilerResults>("compile", p, CancellationSource.Token).Wait();

            stopwatch.Stop();
            var elapsed_time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Performancetest took {elapsed_time}ms");
            Assert.Less(elapsed_time, 5e3, "Runtime takes too long!" + elapsed_time);
        }
    }
}