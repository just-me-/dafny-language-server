using DafnyLanguageServer.Handler;
using NUnit.Framework;
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
using DafnyLanguageServer.DafnyAccess;
using MediatR;

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



    //Todo evtl diese extension methode iwo anders parkieren... ist nur für einfache stringausgabe jetzt zum tesetn, is aber sicher auch in zukunft nützlich.
    public static class Extension
    {
        public static string ToCustomString(this Range r)
        {
            return $"[Range: L{r.Start.Line} C{r.Start.Character} - L{r.End.Line} C{r.End.Character}]";
        }
    }


    public class Tests
    {

        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(Tests).Assembly.Location);
        internal static readonly string serverExe = Path.GetFullPath(Path.Combine(assemblyPath, "../Binaries/DafnyLanguageServer.exe"));
        internal static readonly string aDfyFile = Path.GetFullPath(Path.Combine(assemblyPath, "../Test/LanguageServerTest/UnitTests/CounterExampleTest/CounterExampleTestFiles/fail1.dfy"));
        internal static readonly string workspaceDir = Path.GetFullPath(Path.Combine(assemblyPath, ".../Test/LanguageServerTest/UnitTests/CounterExampleTest/"));

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


            //LoggerProviderCollection providers = new LoggerProviderCollection();

            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Debug()   //set to debug here for full information
            //    .WriteTo.Console()
            //    .WriteTo.Providers(providers)
            //    .CreateLogger();

            ILogger log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var LoggerFactory = new SerilogLoggerFactory(log);

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(
                TimeSpan.FromSeconds(30)
            );

            ServerProcess server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(serverExe)
            {
                Arguments = "--log TestLog.txt"
            });
            LanguageClient client = new LanguageClient(LoggerFactory, server);
            //IHandler h = new DiagHandler();
            //client.RegisterHandler(h)


            try
            {

                log.Information("**** Initialising language server...");

                client.Initialize(
                    workspaceRoot: workspaceDir,
                    initializationOptions: new { },
                    cancellationToken: cancellationSource.Token
                ).Wait();

                //var myDiagHandler = new DiagHandler();
                //client.RegisterHandler(myDiagHandler);  //todo


                log.Information("*** Language server has been successfully initialised. ");

                log.Information("*** Sending DidOpen.....");

                client.TextDocument.DidOpen(aDfyFile, "dfy");


                log.Information("*** Sending DidChange.....");
                client.TextDocument.DidChange(aDfyFile, "dfy");


                //es kommt an:
                //[{"label":"a (Type: Method) (Parent: _default)","kind":2,"deprecated":false,"preselect":false,"insertTextFormat":0,"textEdit":{"range":{"start":{"line":2,"character":5},"end":{"line":2,"character":6}},"newText":"a"}}]


                

                log.Information("*** Sending Completions.....");
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
                    log.Information("Got completion list!");
                    foreach (var cl in completions.Items)
                    {
                        log.Information( $"Completion Item: {cl.TextEdit.NewText} with label {cl.Label} at {cl.TextEdit.Range.ToCustomString()}");
                    }
                }
                else
                {
                    log.Warning("No hover info available at ({Line}, {Column}).", 7, 3);
                }


                //kommt an:
                //[21:01:03 DBG] Read response body {"jsonrpc":"2.0","id":"3","result":{"counterExamples":[{"line":4,"col":19,"variables":{"inp1":"((- 160))","more":"((- 320))"}}]}}.
                //[21:01:03 DBG] Received response 3 from language server: {"counterExamples":[{"line":4,"col":19,"variables":{"inp1":"((- 160))","more":"((- 320))"}
                //}]}

                //geiler scheiss alter


                var counterExampleParam = new CounterExampleParams
                {
                    DafnyFile = aDfyFile
                };
                

                log.Information("*** Sending counterExample.....");
                var counterExamples = client.SendRequest<CounterExampleResults>("counterExample",
                    counterExampleParam, cancellationSource.Token).Result;


                

                if (counterExamples != null)
                {
                    log.Information("*** Got counter examples: ");

                    foreach (CounterExampleResult ce in counterExamples.CounterExamples)
                    {
                        log.Information("Line {0}, Col {1}", ce.Line, ce.Col);
                        foreach (KeyValuePair<string, string> kvp in ce.Variables)
                        {
                            log.Information("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                        }
                    }
                }
                else
                {
                    log.Warning("***No counter Examples available");

                }

    




            }
            catch (Exception e)
            {
                log.Error(e, "Error Msg:", e.Message);
            }
            finally
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

        }
    }
}
