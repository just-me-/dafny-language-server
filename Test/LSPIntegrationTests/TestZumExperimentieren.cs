using DafnyLanguageServer.Handler;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OmniSharp.Extensions.Embedded.MediatR;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Handlers;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using OmniSharp.Extensions.LanguageServer.Protocol.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LSPIntegrationTests
{

    internal class DiagHandler : IPublishDiagnosticsHandler, IHandler
    {
        public string Method => "diagnostics";

        public Type PayloadType => typeof(PublishDiagnosticsParams);

        private List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public Task<Unit> Handle(PublishDiagnosticsParams request, CancellationToken cancellationToken)
        {
            var a = request.Diagnostics;
            foreach (var item in a)
            {
                Diagnostics.Add(item);
            }
            return Unit.Task;
        }
    }
    public class Tests
    {

        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(Tests).Assembly.Location);
        internal static readonly string serverExe = Path.GetFullPath(Path.Combine(assemblyPath, "../Binaries/DafnyLanguageServer.exe"));
        internal static readonly string aDfyFile = Path.GetFullPath(Path.Combine(assemblyPath, "../Test/CounterExampleFiles/fail1.dfy"));
        internal static readonly string workspaceDir = Path.GetFullPath(Path.Combine(assemblyPath, "../Test/CounterExampleFiles/"));

        [SetUp]
        public void CheckFiles()
        {
            if (!File.Exists(serverExe))
            {
                throw new AssertionException("File not existing: Server Exe");
            }

            if (!File.Exists(aDfyFile))
            {
                throw new AssertionException("File not existing: a Dfy File");
            }
        }

        [Test]
        public void DemoTest()
        {


            LoggerProviderCollection providers = new LoggerProviderCollection();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()   //set to debug here for full information
                .WriteTo.Console()
                .WriteTo.Providers(providers)
                .CreateLogger();

            ILoggerFactory LoggerFactory = new SerilogLoggerFactory(Log.Logger);

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(
                TimeSpan.FromSeconds(30)
            );

            ServerProcess server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(serverExe)
            {
                Arguments = ""
            });
            LanguageClient client = new LanguageClient(LoggerFactory, server);
            //IHandler h = new DiagHandler();
            //client.RegisterHandler(h)


            try
            {

                Log.Information("\n**** Initialising language server...\n");

                client.Initialize(
                    workspaceRoot: workspaceDir,
                    initializationOptions: new { },
                    cancellationToken: cancellationSource.Token
                ).Wait();

                //var myDiagHandler = new DiagHandler();
                //client.RegisterHandler(myDiagHandler);  //todo


                Log.Information("\n\n*** Language server has been successfully initialised. \n");

                Log.Information("\n\n*** Sending DidOpen.....\n");

                client.TextDocument.DidOpen(aDfyFile, "dfy");


                Log.Information("\n\n*** Sending DidChange.....\n");
                client.TextDocument.DidChange(aDfyFile, "dfy");


                //es kommt an:
                //[{"label":"a (Type: Method) (Parent: _default)","kind":2,"deprecated":false,"preselect":false,"insertTextFormat":0,"textEdit":{"range":{"start":{"line":2,"character":5},"end":{"line":2,"character":6}},"newText":"a"}}]


                Log.Information("\\nn*** Sending Completions.....\n");
                var c = client.TextDocument.Completions(
                    filePath: aDfyFile,
                    line: 2,
                    column: 5,
                    cancellationToken: cancellationSource.Token
                );

                c.Wait();

                var completions = c.Result;

                //Test completions for correctness here

                if (completions != null)
                {
                    Log.Information("\n\nGot completion list" + completions);
                }
                else
                {
                    Log.Warning("\nNo hover info available at ({Line}, {Column}).", 7, 3);
                }


                //kommt an:
                //[21:01:03 DBG] Read response body {"jsonrpc":"2.0","id":"3","result":{"counterExamples":[{"line":4,"col":19,"variables":{"inp1":"((- 160))","more":"((- 320))"}}]}}.
                //[21:01:03 DBG] Received response 3 from language server: {"counterExamples":[{"line":4,"col":19,"variables":{"inp1":"((- 160))","more":"((- 320))"}
                //}]}

                //geiler scheiss alter


                //also es kommt iwie an aber die variablen hier werden nicht passend gesetzt.

                var counterExampleParam = new CounterExampleParams
                {
                    DafnyFile = aDfyFile
                };


                Log.Information("*** \n\nSending counterExample.....\n");
                CounterExampleResult counterExamples = client.SendRequest<CounterExampleResult>("counterExample",
                    counterExampleParam, cancellationSource.Token).Result;

                if (counterExamples != null)
                {
                    Log.Information("\n\n*** Got counter examples: ");
                    Log.Information("Line {0}, Col {1}", counterExamples.Line, counterExamples.Col);
                    foreach (KeyValuePair<string, string> kvp in counterExamples.Variables)
                    {
                        Log.Information("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    Log.Warning("\n\n***No counter Examples available");

                }




            }
            catch (Exception e)
            {
                Log.Error(e, "Error Msg:", e.Message);
            }
            finally
            {
                Log.Information("\n\nShutting down client...");
                client.Shutdown().Wait();
                Log.Information("\\nClient shutdown is complete.");

                Log.Information("\n\nShutting down server...");
                server.Stop().Wait();
                Log.Information("\n\nServer shutdown is complete.");

                client.Dispose();
                server.Dispose();
            }

        }
    }
}
