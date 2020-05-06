using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler.LspStandard;
using DafnyLanguageServer.ProgramServices;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using Serilog;
using DidChangeWatchedFilesHandler = DafnyLanguageServer.Handler.DidChangeWatchedFilesHandler;
using ILanguageServer = OmniSharp.Extensions.LanguageServer.Server.ILanguageServer;
using ILogger = Serilog.ILogger;
using ShutdownHandler = DafnyLanguageServer.Handler.ShutdownHandler;

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
            log.Debug(Resources.LoggingMessages.server_starting);

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
                    .WithHandler<TextDocumentSyncTaskHandler>()
                    .WithHandler<DidChangeWatchedFilesHandler>()
                    .WithHandler<CompletionTaskHandler>()
                    .WithHandler<CompileHandler>()
                    .WithHandler<CounterExampleHandler>()
                    .WithHandler<CodeLensTaskHandler>()
                    .WithHandler<DefinitionTaskHandler>()
                    //.WithHandler<RenamePreparationTaskHandler>()
                    .WithHandler<RenameTaskHandler>()
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
                string msg = Resources.LoggingMessages.could_not_redirect_outputstream + " " + e.Message;
                msgSender.SendError(msg);
                log.Error(msg);
            }

            log.Debug(Resources.LoggingMessages.server_closed);
        }

        private void CreateMsgSender(ILanguageServer server)
        {
            this.msgSender = new MessageSenderService(server);
        }

        private void SendServerStartedInformation()
        {
            var dafnyVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion;
            msgSender.SendServerStarted(dafnyVersion);
            log.Debug(Resources.LoggingMessages.server_running);
        }

        private void CheckForConfigReader()
        {
            if (config.Error)
            {
                var msg = $"{Resources.LoggingMessages.could_not_setup_config} {Resources.LoggingMessages.error_msg} {config.ErrorMsg}";
                msgSender.SendWarning(msg);
                log.Warning(msg);
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
