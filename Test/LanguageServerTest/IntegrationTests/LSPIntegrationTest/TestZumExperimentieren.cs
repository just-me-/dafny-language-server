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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using MediatR;
using PublishDiagnosticsHandler = OmniSharp.Extensions.LanguageServer.Client.PublishDiagnosticsHandler;
using Files = PathConstants.Paths;

namespace LSPIntegrationTests
{




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

        private bool barrierIsDown = true;

        private void WaitUntilDiagnosticsArrived(ILogger log)
        {
            log.Information("Waiting here in hope to get a diagnostics....");

            double waitedTime = 0.1;
            while (barrierIsDown && waitedTime < 5)
            {
                Thread.Sleep(100);
                waitedTime += 0.1;
            }
            barrierIsDown = true;

            if (waitedTime >= 5)
            {
                log.Error("Waited max time... Continuing now.");
            }
            else
            {
                log.Information($"Success: Continuing... Time Waited: {waitedTime}");
            }
            

        }

        [SetUp]
        public void CheckFiles()
        {
            if (!File.Exists(Files.dafnyExe))
            {
                throw new AssertionException("File not existing: Server Exe");
            }

            if (!File.Exists(Files.int_demofile))
            {
                throw new AssertionException("File not existing: a Dfy File");
            }
        }

        [Test]
        public void DemoTest()
        {

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

            ServerProcess server = new StdioServerProcess(LoggerFactory, new ProcessStartInfo(Files.langServExe)
            {
                Arguments = "/log ../Logs/TestLog.txt /loglevel 0"
            });
            LanguageClient client = new LanguageClient(LoggerFactory, server);


            try
            {

                log.Information("**** Initialising language server...");

                client.Initialize(
                    workspaceRoot: Files.testFilesPath,
                    initializationOptions: new { },
                    cancellationToken: cancellationSource.Token
                ).Wait();


                ///////////////////OPEN, CHANGE EXAMPLE ///////////////////////////////////////////


                log.Information("*** Language server has been successfully initialised. ");



                PublishDiagnosticsHandler diagnosticsHandler = (uri, diagList) =>
                {
                    
                    log.Information("%%%%% Received Diagnostics!");
                    log.Information("Uri: " + uri);
                    foreach (var d in diagList)
                    {
                        log.Information(
                            $"Severity: {d.Severity} / Range: {d.Range.ToCustomString()} / Message: {d.Message}");
                        log.Information("Related Information: " + d.RelatedInformation?.FirstOrDefault().Message);
                    }

                    barrierIsDown = false;
                };

                client.TextDocument.OnPublishDiagnostics(diagnosticsHandler);



                log.Information("*** Sending DidOpen.....");
                client.TextDocument.DidOpen(Files.int_demofile, "dfy");
                WaitUntilDiagnosticsArrived(log);                                      //Todo: mit event arbeiten ?



                log.Information("*** Sending DidChange.....");
                client.TextDocument.DidChange(Files.int_demofile, "dfy");
                WaitUntilDiagnosticsArrived(log);





                ///////////////////AUTO COMPLETION EXAMPLE ///////////////////////////////////////////



                log.Information("*** Sending Completions.....");
                var completions = client.TextDocument.Completions(
                    filePath: Files.int_demofile,
                    line: 2,
                    column: 5,
                    cancellationToken: cancellationSource.Token
                ).Result;



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

                

                ///////////////////COUNTER EXAMPLE EXAMPLE ///////////////////////////////////////////

                var counterExampleParam = new CounterExampleParams
                {
                    DafnyFile = Files.int_demofile
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

    
                //////////////////////////////////GOTO EXAMPLE/////////////////////////

                log.Information("Diong Goto Definition");

                var gonetodef = client.TextDocument.Definition(Files.int_demofile, 9, 12).Result;
                if (gonetodef != null && gonetodef.Count() == 1)
                {
                    log.Information($"Got Location for goto " + gonetodef.First().Location.Range.ToCustomString() + " in file " + gonetodef.First().Location.Uri.AbsolutePath);
                }


                //Code lens erstma weglassen bis da symbol zeug rady würd ich sagen
                //ausserdem seh ich grad nicht wie man das senden / empfangen kann hust hust



                ////////////////////////COMPILE Example///////////////////////////
                CompilerParams compilerParams = new CompilerParams
                {
                    DafnyFilePath = Files.int_demofile,
                    DafnyExePath = Files.dafnyExe
                };

                log.Information("Sending that compile ;_)");

                var compilerResults = client.SendRequest<CompilerResults>("compile", compilerParams, cancellationSource.Token).Result;
                log.Information($"Got compile answer: Error: {compilerResults.Error} / Exe?:{compilerResults.Executable} / Massage: {compilerResults.Message}");






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
