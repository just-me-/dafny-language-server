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

            string assemblyPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            string logPath = Path.Combine(assemblyPath, "../Logs");
            Directory.CreateDirectory(logPath);
            string redirectedStreamPath = Path.Combine(logPath, "./StreamRedirection.txt");
            string loggerOutputPath = Path.Combine(logPath, "./Log.txt");

            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Invalid Number of Arguments provided");
            }

            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i].ToLower() == "--stream")
                {
                    redirectedStreamPath = Path.Combine(logPath, args[i+1]);
                }
                if (args[i].ToLower() == "--log")
                {
                    loggerOutputPath = Path.Combine(logPath, args[i+1]);
                }
            }

            ILogger log = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(loggerOutputPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            log.Information("Server Starting");

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .ConfigureLogging(x => x
                        .AddSerilog(log)
                        .AddLanguageServer()
                        .SetMinimumLevel(LogLevel.Trace)
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

            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(redirectedStreamPath, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    Console.SetOut(writer);
                    await server.WaitForExit;
                }
            }
            catch (Exception e)
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
    }
}
