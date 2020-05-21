using DafnyLanguageServer.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Handler.LspStandard;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
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
    public class DafnyLanguageServer
    {
        private ILogger log;
        private MessageSenderService msgSender;
        private readonly ConfigInitializationErrors configInitErrors;

        public DafnyLanguageServer(string[] args)
        {
            var configInitializer = new ConfigInitializer(args);
            configInitializer.SetUp();
            configInitErrors = configInitializer.InitializationErrors;
            log = LoggerCreator.GetLogger();
        }

        public async Task StartServer()
        {
            log.Debug(Resources.LoggingMessages.server_starting);

            var server = await LanguageServer.From(options =>
                options

                    //Log and Console
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
                    .WithHandler<RenameTaskHandler>()
                    .WithHandler<HoverTaskHandler>()
                    .WithHandler<ShutdownHandler>()
            );

            CreateMsgSender(server);
            SendServerStartedInformation();
            CheckForConfigErrors();

            // Redirect OutPutStream for plain LSP output (avoid Boogie output printer stuff) and start server 
            // code should no longer make prints but lets keep it for additional safety.
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(LanguageServerConfig.RedirectedStreamFile, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    Console.SetOut(writer);
                    await server.WaitForExit;
                }
            }
            catch (Exception e)
            {
                string msg = Resources.LoggingMessages.could_not_redirect_outputstream + e.Message;
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

        private void CheckForConfigErrors()
        {
            if (!configInitErrors.HasErrors)
            {
                return;
            }
            var msg = $"{Resources.LoggingMessages.could_not_setup_config} {Resources.LoggingMessages.error_msg} {configInitErrors.ErrorMessages}";
            msgSender.SendWarning(msg);
            log.Warning(msg);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWorkspace, Workspace>();
            services.AddLogging();
        }
    }
}
