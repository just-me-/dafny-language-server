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
    /// <summary>
    /// This class provides the <c>Main</c> function.
    /// It starts the Omnisharp Language Server and registers all handlers as well as the services.
    /// It does also use owr <c>ConfigReader</c> to provide customized settings for the server.
    /// It also redirects the output stream. 
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            ExecutionEngine.printer = new DafnyConsolePrinter();

            var configReader = new ConfigReader(args);

            ILogger log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(configReader.LogFile, rollingInterval: RollingInterval.Day, fileSizeLimitBytes:1024*1024)
                .CreateLogger();

            //For quick manual debugging of the console reader / macht aber konsolenout kaputt - nicht nutzen xD
            //TOdo: Vor abgabe weg machen xD Ticket # 59
            //configReader.PrintState();

            log.Debug("Server Starting");

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog(log)
                        .AddLanguageServer()
                        .SetMinimumLevel(configReader.Loglevel)
                    )
                    // Service group 
                    .WithServices(ConfigureServices)
                    // Handler group 
                    .WithHandler<TextDocumentSyncHandler>()
                    //.WithHandler<DidChangeWatchedFilesHandler>()   // Todo rm io? 
                    .WithHandler<CompletionHandler>()
                    .WithHandler<CompileHandler>()
                    .WithHandler<CounterExampleHandler>()
                    .WithHandler<CodeLensHandler>()
                    .WithHandler<DefinitionHandler>()
                    
            );
            var msgSender = new MessageSender(server);
            var dafnyVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
            msgSender.SendServerStarted(dafnyVersion);
            log.Debug("Server Running");

            if (configReader.Error)
            {
                msgSender.SendWarning("Error while setting up config: " + configReader.ErrorMsg);
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
            catch (Exception e)
            {
                string msg = "Could not redirect output stream. " + e.Message;
                msgSender.SendError(msg);
                log.Error(msg);
            }

            log.Debug("Server Closed");
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BufferManager>();
            services.AddLogging();
        }
    }
}
