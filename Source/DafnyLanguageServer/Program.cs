using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Handler;
using Microsoft.Boogie;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using DafnyConsolePrinter = DafnyLanguageServer.DafnyAccess.DafnyConsolePrinter;
using ILogger = Serilog.ILogger;

namespace DafnyLanguageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ExecutionEngine.printer = new DafnyConsolePrinter();

            var configReader = new ConfigReader(args);




            ILogger log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(configReader.LogFile, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //For quick manual debugging of the console reader / macht aber konsolenout kaputt - nicht nutzen xD
            //TOdo: Vor abgabe weg machen xD Ticket # 59
            //configReader.PrintState();

            

            log.Information("Server Starting");

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog(log)
                        .AddLanguageServer()
                        .SetMinimumLevel(configReader.Loglevel)
                    )

                    .WithServices(ConfigureServices)

                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<CompletionHandler>()
                    .WithHandler<CompileHandler>()
                    .WithHandler<CounterExampleHandler>()
                    .WithHandler<CodeLensHandler>()
                    .WithHandler<DefinitionHandler>()
                    
            );

            log.Information("Server Running");

            if (configReader.Error)
            {
                server.Window.SendNotification("message", "Error while setting up config: " + configReader.ErrorMsg);
                log.Warning("Error while configuring log. Error Message: " + configReader.ErrorMsg);
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(configReader.RedirectedStreamFile, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    Console.SetOut(writer);
                    await server.WaitForExit;
                }
            }
            catch
            {
                log.Error("Couldn't redirect output stream");
                server.Window.SendNotification("message", "Couldn't redirect output stream");
            }

            log.Information("Server Closed");

        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BufferManager>();
            services.AddLogging();
        }

        
    }
}
