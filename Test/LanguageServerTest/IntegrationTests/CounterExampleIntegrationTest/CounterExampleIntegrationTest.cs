using System;
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

            //compilerResults = null;
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
        public void B()
        {
            Assert.Pass();
        }

        [Test]
        public void C()
        {
            Assert.Pass();
        }
    }
}