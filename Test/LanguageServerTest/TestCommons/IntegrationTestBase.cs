using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Resources;
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

        private string Name { get; }
        private ServerProcess Server { get; set; }
        protected ILogger MyLog { get; set; }
        protected SerilogLoggerFactory LoggerFactory { get; set; }

        public IntegrationTestBase(string name) => this.Name = name;

        [SetUp]
        public void Setup()
        {
            foreach (var process in Process.GetProcessesByName("DafnyLanguageServer.exe"))
            {
                process.Kill();
            }

            CancellationSource = new CancellationTokenSource();
            CancellationSource.CancelAfter(TimeSpan.FromSeconds(30));

            MyLog = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(MyLog);

            Server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(FileAndFolderLocations.langServExe)
            {
                Arguments = $"/log:Logs/Test_{Name}.txt /loglevel:0"
            });

            //Note: Client will log to console, but server always logs to file.

            Client = new LanguageClient(LoggerFactory, Server);
            Client.Initialize(
                workspaceRoot: FileAndFolderLocations.testFilesFolder,
                initializationOptions: new { },
                cancellationToken: CancellationSource.Token
            ).Wait();

            MyLog.Information("Language server has been successfully initialized.");
            MyLog.Information($"Running {Name} Test");

        }

        [TearDown]
        public void TearDown()
        {
            MyLog.Information("Shutting down client...");
            Task.WhenAny(Client.Shutdown());
            MyLog.Information("Client shutdown is complete.");

            MyLog.Information("Shutting down server...");
            Server.Stop().Wait();
            MyLog.Information("Server shutdown is complete.");

            Client.Dispose();
            Server.Dispose();
        }
    }
}
