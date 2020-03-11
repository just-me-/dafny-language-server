using DafnyLanguageServer.Handler;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using PublishDiagnosticsHandler = OmniSharp.Extensions.LanguageServer.Client.PublishDiagnosticsHandler;
using Files = PathConstants.Paths;

namespace CompileIntegrationTest
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

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(log);

            cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(30));

            assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }

        [SetUp]
        public void Setup()
        {
            server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(Files.langServExe)
            {
                Arguments = $"/log ../Logs/{assemblyName}.txt /loglevel 0"
            });
            client = new LanguageClient(LoggerFactory, server);
        }

        [TearDown]
        public void TearDown()
        {
            log.Information("Shutting down client...");
            client.Shutdown().Wait();
            client.Dispose();
            log.Information("Client shutdown is complete.");

            log.Information("Shutting down server...");
            server.Stop().Wait();
            server.Dispose();
            log.Information("Server shutdown is complete.");
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}