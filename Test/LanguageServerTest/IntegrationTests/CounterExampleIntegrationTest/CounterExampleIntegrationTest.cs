using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using Serilog;
using Serilog.Extensions.Logging;
using Files = PathConstants.Paths;

namespace CounterExampleIntegrationTest
{
    [TestFixture]
    public class Tests
    {
        private LanguageClient client;
        private ServerProcess server;
        private ILogger log;
        private SerilogLoggerFactory LoggerFactory;
        private string assemblyName;
        private CancellationTokenSource cancellationSource;

        private CounterExampleResults counterExampleResults;

        private readonly string keyword = "counterExample";

        [SetUp]
        public void Setup()
        {
            cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

            log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(log);

            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(Files.langServExe)
            {
                Arguments = $"/log ../Logs/{assemblyName}.txt /loglevel 0"
            });

            client = new LanguageClient(LoggerFactory, server);

            client.Initialize(
                workspaceRoot: Files.testFilesPath,
                initializationOptions: new { },
                cancellationToken: cancellationSource.Token
            ).Wait();

            log.Information("*** Language server has been successfully initialized.");

            counterExampleResults = null;
        }

        [TearDown]
        public void TearDown()
        {
            log.Information("Shutting down client...");
            Task.WhenAny(client.Shutdown());
            log.Information("Client shutdown is complete.");

            log.Information("Shutting down server...");
            server.Stop().Wait();
            log.Information("Server shutdown is complete.");

            client.Dispose();
            server.Dispose();

        }

        [Test]
        public void PostconditionFullfilled()
        {
            GetCounterExamples(Files.ce_ok);
            Assert.AreEqual(0, counterExampleResults.CounterExamples.Count);
        }


        [Test]
        public void ViolatedWith1CounterExamples()
        {
            GetCounterExamples(Files.ce_fail1);
            Assert.AreEqual(1, counterExampleResults.CounterExamples.Count);
        }

        [Test]
        public void ViolatedWith2CounterExamples()
        {
            GetCounterExamples(Files.ce_fail2);
            Assert.AreEqual(2, counterExampleResults.CounterExamples.Count);
        }


        private void GetCounterExamples(string file)
        {
            var counterExampleParams = new CounterExampleParams
            {
                DafnyFile = file
            };
            client.TextDocument.DidOpen(file, "dfy");  //notiz für später / dafny translation unit etc: das counter example will, dass das file geöffnet ist! drum mussten wir da auch iwo mal explizit ne eigenen DTU machen - kann das sein??
            counterExampleResults = client.SendRequest<CounterExampleResults>(keyword, counterExampleParams, cancellationSource.Token).Result;
        }

        private void CheckResult()
        {
            if (counterExampleResults == null)
            {
                Assert.Fail("No Counter Example Message provided by server");
            }


            foreach (CounterExampleResult ce in counterExampleResults.CounterExamples)
            {
                var line = ce.Line;
                var col = ce.Col;
                foreach (KeyValuePair<string, string> kvp in ce.Variables)
                {
                    var key = kvp.Key;
                    var val = kvp.Value;
                    //In case needed for later with detailled check
                }
            }
        }

        
    }
}