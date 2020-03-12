using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using Serilog;
using Serilog.Extensions.Logging;
using Files = TestCommons.Paths;

namespace TestCommons
{
    public class TestSetupManager
    {
        public LanguageClient Client { get; private set; }
        public CancellationTokenSource CancellationSource { get; private set; }

        private readonly string name;
        private ServerProcess server;
        private ILogger log;
        private SerilogLoggerFactory LoggerFactory;


        public TestSetupManager(string name) => this.name = name;


        public void Setup()
        {
            CancellationSource = new CancellationTokenSource();
            CancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

            log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(log);

            server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(Files.langServExe)
            {
                Arguments = $"/log ../Logs/Test_{name}.txt /loglevel 1"
            });

            Client = new LanguageClient(LoggerFactory, server);

            Client.Initialize(
                workspaceRoot: Files.testFilesPath,
                initializationOptions: new { },
                cancellationToken: CancellationSource.Token
            ).Wait();

            log.Information("*** Language server has been successfully initialized.");
        }


        

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
