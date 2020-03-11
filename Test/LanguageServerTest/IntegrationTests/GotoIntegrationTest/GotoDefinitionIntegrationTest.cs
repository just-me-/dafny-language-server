using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Serilog;
using Serilog.Extensions.Logging;
using Files = PathConstants.Paths;

namespace GotoIntegrationTest
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

        private LocationOrLocationLinks goneTo;


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

            log.Information("Language server has been successfully initialized.");

            goneTo = null;
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
        public void GoTo()
        {
            SetGoToDefinitionWithoutZeroIndexing(Files.gt_goto, 21, 22);
            VerifyResult(Files.gt_goto, 8, 7);
        }




        private void SetGoToDefinitionWithoutZeroIndexing(string file, int line, int col)
        {
            client.TextDocument.DidOpen(file, "dfy");
            goneTo = client.TextDocument.Definition(file, line-1, col-1).Result;
        }

        private void VerifyResult(string expectedFile, int expectedLine, int expectedCol)
        {
            long line = goneTo.FirstOrDefault().Location.Range.Start.Line;
            long col = goneTo.FirstOrDefault().Location.Range.Start.Character;
            Uri uri = goneTo.FirstOrDefault().Location.Uri;

            Assert.AreEqual(line, expectedLine);
            Assert.AreEqual(col, expectedCol);
            Assert.AreEqual(new Uri(expectedFile), uri);
        }




    }
}