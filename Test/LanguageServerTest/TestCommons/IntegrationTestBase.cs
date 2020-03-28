using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using Serilog;
using Serilog.Extensions.Logging;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace TestCommons
{
    [Category("Integration")]
    public class IntegrationTestBase
    {
        protected LanguageClient Client { get; private set; }
        protected CancellationTokenSource CancellationSource { get; set; }

        private readonly string name;
        private ServerProcess server;
        protected ILogger log;
        protected SerilogLoggerFactory LoggerFactory;

        public IntegrationTestBase(string name) => this.name = name;

        [SetUp]
        public void Setup()
        {
            CancellationSource = new CancellationTokenSource();
            CancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

            log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(log);

            server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(Files.langServExe)
            {
                Arguments = $"/log ../Logs/Test_{name}.txt /loglevel 0"
            });

            Client = new LanguageClient(LoggerFactory, server);
            Client.Initialize(
                workspaceRoot: Files.testFilesPath,
                initializationOptions: new { },
                cancellationToken: CancellationSource.Token
            ).Wait();

            Thread.Sleep(1000);
            log.Information("*** Language server has been successfully initialized.");
        }

        [TearDown]
        public void TearDown()
        {
            log.Information("Shutting down client...");
            //client.Shutdown().Wait();   Why exception?? würd mich schon noch wunder nehmen...
            Task.WhenAny(Client.Shutdown());
            log.Information("Client shutdown is complete.");

            log.Information("Shutting down server...");
            server.Stop().Wait();
            log.Information("Server shutdown is complete.");

            Client.Dispose();
            server.Dispose();
        }
    }
}
