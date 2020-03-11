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

        private CompilerResults compilerResults;

        private readonly string compileKeyword = "compile";
        private readonly string successMsg = "Compilation successful";
        private readonly string kaiwaasas = "compile";


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

            client.Initialize(
                workspaceRoot: Files.testFilesPath,
                initializationOptions: new { },
                cancellationToken: cancellationSource.Token
            ).Wait();

            log.Information("*** Language server has been successfully initialized.");

            compilerResults = null;
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
        public void SuccessWithExeAsResult()
        {
            CompilerParams compilerParams = new CompilerParams
            {
                DafnyFilePath = Files.cp_fineEXE,
                DafnyExePath = Files.dafnyExe
            };

            compilerResults = client.SendRequest<CompilerResults>(compileKeyword, compilerParams, cancellationSource.Token).Result;
            VerifyCompileResults(false,true, successMsg);
        }

        private void VerifyCompileResults(bool expectedError, bool expectedExecutable, string expectedMessage)
        {
            if (compilerResults == null)
            {
                Assert.Fail("compilerResults are null - no results received!");
            }
            Assert.AreEqual(expectedError, compilerResults.Error);
            Assert.AreEqual(expectedExecutable, compilerResults.Executable);
            Assert.AreEqual(expectedMessage, compilerResults.Message);
        }
    }
}