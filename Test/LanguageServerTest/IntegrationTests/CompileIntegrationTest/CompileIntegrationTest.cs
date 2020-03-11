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
        private readonly string kaiwaasas = "compile"; //todo


        [SetUp]
        public void Setup()
        {
            cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(TimeSpan.FromSeconds(30));

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

            compilerResults = null;
        }

        [TearDown]
        public void TearDown()
        {
            log.Information("Shutting down client...");
            client.Shutdown().Wait();
            
            log.Information("Client shutdown is complete.");

            log.Information("Shutting down server...");
            server.Stop().Wait();
            log.Information("Server shutdown is complete.");

            client.Dispose();
            server.Dispose();

        }

        [Test]
        public void SuccessWithExeAsResult()
        {
            RunCompilation(Files.cp_fineEXE);
            VerifyResults(false, true, successMsg);
        }

        [Test]
        public void SuccessWithDllAsResult()
        {
            RunCompilation(Files.cp_fineDLL);
            VerifyResults(false, false, successMsg);
        }

        [Test]
        public void FailureAssertionViolation()
        {
            RunCompilation(Files.cp_assertion);
            VerifyResults(true, false, "Compilation failed: \"assertion violation\" in line 7.");
        }

        [Test]
        public void FailurePostconditionViolation()
        {
            RunCompilation(Files.cp_postcondition);
            VerifyResults(true, false, "Compilation failed: \"BP5003: A postcondition might not hold on this return path.\" in line 4.");
        }

        [Test]
        public void FailureSyntaxErrorUnknownIdentifier()
        {
            RunCompilation(Files.cp_identifier);
            VerifyResults(true, false, "Compilation failed: \"unresolved identifier: bruder\" in line 8.");
        }


        private void RunCompilation(string testfile)
        {
            CompilerParams compilerParams = new CompilerParams
            {
                DafnyFilePath = testfile,
                DafnyExePath = Files.dafnyExe
            };

            compilerResults = client.SendRequest<CompilerResults>(compileKeyword, compilerParams, cancellationSource.Token).Result;
        }

        private void VerifyResults(bool expectedError, bool expectedExecutable, string expectedMessage)
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