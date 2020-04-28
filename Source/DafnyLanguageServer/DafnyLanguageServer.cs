using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using DafnyLanguageServer.ProgramServices;
using Serilog;
using ILogger = Serilog.ILogger;

namespace DafnyLanguageServer
{
    /// <summary>
    /// Creates and starts the Dafny Language Server.
    /// It starts the Omnisharp Language Server and registers all handlers as well as the services.
    /// It does also use owr <c>ConfigInitializer</c> to provide customized settings for the server.
    /// It also redirects the output stream. 
    /// </summary>
    class DafnyLanguageServer
    {
        private ILogger log;
        private LanguageServerConfig config;
        private MessageSenderService msgSender;

        public DafnyLanguageServer(string[] args)
        {
            config = new ConfigInitializer(args).Config;
            //For quick manual debugging of the console reader / macht aber konsolenout kaputt - nicht nutzen xD
            //TOdo: Vor abgabe weg machen xD Ticket # 59
            //Console.WriteLine(config);
            SetupLogger();
        }

        public async Task StartServer()
        {
            log.Debug("Server is starting..."); // todo lang file #102
            log.Debug("Server is starting..." + Resources.ExceptionMessages.test); // todo lang file #102

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog(log)
                        .AddLanguageServer()
                    )
                    // Service group 
                    .WithServices(ConfigureServices)
                    // Handler group 
                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<DidChangeWatchedFilesHandler>()
                    .WithHandler<CompletionHandler>()
                    .WithHandler<CompileHandler>()
                    .WithHandler<CounterExampleHandler>()
                    .WithHandler<CodeLensHandler>()
                    .WithHandler<DefinitionHandler>()
                    .WithHandler<ShutdownHandler>()
            );

            CreateMsgSender(server);
            SendServerStartedInformation();
            CheckForConfigReader();

            // Redirect OutPutStream for plain LSP output (avoid Boogie output printer stuff) and start server 
            // code should no longer make prints but lets keep it for additional safety.
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(config.RedirectedStreamFile, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    Console.SetOut(writer);
                    await server.WaitForExit;
                }
            }
            catch (Exception e)
            {
                string msg = "Could not redirect output stream. " + e.Message; // todo lang file #102
                msgSender.SendError(msg);
                log.Error(msg);
            }

            log.Debug("Server closed."); // todo lang file #102
        }

        private void CreateMsgSender(ILanguageServer server)
        {
            this.msgSender = new MessageSenderService(server);
        }

        private void SendServerStartedInformation()
        {
            var dafnyVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
            msgSender.SendServerStarted(dafnyVersion);
            log.Debug("Server is running."); // todo lang file #102
        }

        private void CheckForConfigReader()
        {
            if (config.Error) // todo lang file #102
            {
                msgSender.SendWarning("Error while setting up config. Some Defaults may be in use. Error Message: " + config.ErrorMsg);
                log.Warning("Error while configuring log. Some Defaults may be in use. Error Message: " + config.ErrorMsg);
            }
        }

        private void SetupLogger()
        {
            var loggerconfig = new LoggerConfiguration();

            switch (config.Loglevel)
            {
                case LogLevel.Trace:
                    loggerconfig.MinimumLevel.Verbose();
                    break;
                case LogLevel.Debug:
                    loggerconfig.MinimumLevel.Debug();
                    break;
                case LogLevel.Information:
                    loggerconfig.MinimumLevel.Information();
                    break;
                case LogLevel.Warning:
                    loggerconfig.MinimumLevel.Warning();
                    break;
                case LogLevel.Error:
                default:
                    loggerconfig.MinimumLevel.Error();
                    break;
                case LogLevel.Critical:
                    loggerconfig.MinimumLevel.Fatal();
                    break;
            }

            log = loggerconfig
                .Enrich.FromLogContext()
                .WriteTo.File(config.LogFile, fileSizeLimitBytes: 1024 * 1024)
                .CreateLogger();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WorkspaceManager>();
            services.AddLogging();
        }
    }
}
