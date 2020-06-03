using DafnyLanguageServer.Handler;
using NUnit.Framework;
using System;
using System.Diagnostics;
using DafnyLanguageServer.CustomDTOs;
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
        public void Test2109()
        {
            var f = Files.pf_large1;

            var l = new LargeFileGenerator(10_000, 2109);
            l.Generate();
            l.Store(f);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Client.TextDocument.DidOpen(f, "dfy");

            CompilerParams p = new CompilerParams
            {
                CompilationArguments = new string[] { },
                FileToCompile = f
            };

            CancellationSource.CancelAfter(TimeSpan.FromMinutes(5));
            
            Client.SendRequest<CompilerResults>("compile", p, CancellationSource.Token).Wait();

            stopwatch.Stop();

            var elapsed_time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Performancetest 1 with 10'000 LOC took {elapsed_time}ms");
            Assert.Less(elapsed_time, 60e3, "Runtime takes too long! Was: " + elapsed_time);
        }




        [Test]
        public void Test1986()
        {
            var f = Files.pf_large2;
            var l = new LargeFileGenerator(10_000, 1986);
            l.Generate();
            l.Store(f);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Client.TextDocument.DidOpen(f, "dfy");

            CompilerParams p = new CompilerParams
            {
                CompilationArguments = new string[] { },
                FileToCompile = f
            };

            CancellationSource.CancelAfter(TimeSpan.FromMinutes(5));

            Client.SendRequest<CompilerResults>("compile", p, CancellationSource.Token).Wait();

            stopwatch.Stop();

            var elapsed_time = stopwatch.ElapsedMilliseconds;
            Console.WriteLine($"Performancetest 2 with 10'000 LOC took {elapsed_time}ms");
            Assert.Less(elapsed_time, 60e3, "Runtime takes too long! Was: " + elapsed_time);
        }


        [Test]
        public void TestMedium()
        {
          var f = Files.pf_large3;
          var l = new LargeFileGenerator(1_000, 1986);
          l.Generate();
          l.Store(f);

          var stopwatch = new Stopwatch();
          stopwatch.Start();

          Client.TextDocument.DidOpen(f, "dfy");

          CompilerParams p = new CompilerParams
          {
            CompilationArguments = new string[] { },
            FileToCompile = f
          };

          CancellationSource.CancelAfter(TimeSpan.FromMinutes(1));

          Client.SendRequest<CompilerResults>("compile", p, CancellationSource.Token).Wait();

          stopwatch.Stop();

          var elapsed_time = stopwatch.ElapsedMilliseconds;
          Console.WriteLine($"Performancetest 2 with 1'000 LOC took {elapsed_time}ms");
          Assert.Less(elapsed_time, 60e3, "Runtime takes too long! Was: " + elapsed_time);
        }


        [Test]
        public void TestSmall()
        {
          var f = Files.pf_large4;
          var l = new LargeFileGenerator(100, 1986);
          l.Generate();
          l.Store(f);

          var stopwatch = new Stopwatch();
          stopwatch.Start();

          Client.TextDocument.DidOpen(f, "dfy");

          CompilerParams p = new CompilerParams
          {
            CompilationArguments = new string[] { },
            FileToCompile = f
          };

          CancellationSource.CancelAfter(TimeSpan.FromMinutes(1));

          Client.SendRequest<CompilerResults>("compile", p, CancellationSource.Token).Wait();

          stopwatch.Stop();

          var elapsed_time = stopwatch.ElapsedMilliseconds;
          Console.WriteLine($"Performancetest 2 with 100 LOC took {elapsed_time}ms");
          Assert.Less(elapsed_time, 60e3, "Runtime takes too long! Was: " + elapsed_time);
        }

        //Do not execute this theoretical test on gitlab, since it triggers the 1h timeout.
        //[Test]
        public void TestUltraLarge()
        {
          var f = Files.pf_large5;
          var l = new LargeFileGenerator(50_000, 1986);
          l.Generate();
          l.Store(f);

          var stopwatch = new Stopwatch();
          stopwatch.Start();

          Client.TextDocument.DidOpen(f, "dfy");

          CompilerParams p = new CompilerParams
          {
            CompilationArguments = new string[] { },
            FileToCompile = f
          };

          CancellationSource.CancelAfter(TimeSpan.FromMinutes(20));

          Client.SendRequest<CompilerResults>("compile", p, CancellationSource.Token).Wait();

          stopwatch.Stop();

          var elapsed_time = stopwatch.ElapsedMilliseconds;
          Console.WriteLine($"Performancetest 2 with 50k LOC took {elapsed_time}ms");
          Assert.Less(elapsed_time, 600e3, "Runtime takes too long! Was: " + elapsed_time);
        }


  }
}