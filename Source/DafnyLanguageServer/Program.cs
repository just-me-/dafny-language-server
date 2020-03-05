using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Handler;
using Microsoft.Boogie;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Debugging;
using DafnyConsolePrinter = DafnyLanguageServer.DafnyAccess.DafnyConsolePrinter;
using ILogger = Serilog.ILogger;

namespace DafnyLanguageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ExecutionEngine.printer = new DafnyConsolePrinter();

            SetupLog(args, out string redirectedStreamFile, out string logFile, out LogLevel minLevel, out bool errorOnLogging);

            ILogger log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            log.Information("Server Starting");

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog(log)
                        .AddLanguageServer()
                        .SetMinimumLevel(minLevel)
                    )

                    .WithServices(ConfigureServices)

                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<CompletionHandler>()
                    .WithHandler<CompileHandler>()
                    .WithHandler<CounterExampleHandler>()
                    .WithHandler<CodeLensHandler>()
                    .WithHandler<DefinitionHandler>()
                    
            );

            if (errorOnLogging)
            {
                log.Warning("Error while configuring log - default used. Check Launch Args and Config File!");
            }

            log.Information("Server Running");

            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(redirectedStreamFile, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    Console.SetOut(writer);
                    await server.WaitForExit;
                }
            }
            catch
            {
                log.Error("Couldn't redirect output stream");
            }

            log.Information("Server Closed");

        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BufferManager>();
            services.AddLogging();
        }

        private static void SetupLog(string[] args, out string redirectedStreamFile, out string logFile, out LogLevel loglevel, out bool errorOnLogs)
            //Log Levels (starting from 0)
            //Trace - Debug - Info - Warning - Error - Critical - None
        {
            string assemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            //Setting hard defaults                                             //todo remove comments
            redirectedStreamFile = Path.Combine(assemblyPath, "../Logs/StreamRedirection.txt");
            logFile = Path.Combine(assemblyPath, "../Logs/Log.txt");
            loglevel = LogLevel.None;
            errorOnLogs = false;

            try
            {
                //Overwrite with config if available
                string cfgFile = Path.Combine(assemblyPath, "LanguageServerConfig.json");
                if (File.Exists(cfgFile))
                {
                    JObject cfg = JObject.Parse(File.ReadAllText(cfgFile));
                    logFile = cfg["logging"]["log"] == null
                        ? logFile
                        : Path.Combine(assemblyPath, (string) cfg["logging"]["log"]);
                    redirectedStreamFile = cfg["logging"]["stream"] == null
                        ? redirectedStreamFile
                        : Path.Combine(assemblyPath, (string) cfg["logging"]["stream"]);
                    loglevel = cfg["logging"]["loglevel"] == null
                        ? loglevel
                        : (LogLevel) (int) cfg["logging"]["loglevel"];
                }

                //Overwrite with args if available
                if (args.Length % 2 != 0)
                {
                    throw new ArgumentException("Invalid number of arguments provided.");
                }

                for (int i = 0; i < args.Length; i += 2)
                {
                    switch (args[i].ToLower())
                    {
                        case "/stream":
                            redirectedStreamFile = Path.Combine(assemblyPath, args[i + 1]);
                            break;
                        case "/log":
                            logFile = Path.Combine(assemblyPath, args[i + 1]);
                            break;
                        case "/loglevel":
                            loglevel = (LogLevel) int.Parse(args[i + 1]);
                            break;
                        default:
                            throw new ArgumentException("Unkown Parameter: " + args[i]);
                    }
                }
            }
            catch
            {
                errorOnLogs = true;
            }
        }

    }
}
