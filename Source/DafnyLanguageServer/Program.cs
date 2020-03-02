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

            //Note: TempPath goes to C:\Users\[user]\AppData\Local\Temp\ on Windows
            string tempPath = Path.Combine(Path.GetTempPath(), "./Dafny");
            Directory.CreateDirectory(tempPath);
            string redirectedStreamPath = Path.Combine(tempPath, "./StreamRedirection.txt");
            string loggerOutputPath = Path.Combine(tempPath, "./LoggerOutput.txt");

            ILogger log = new LoggerConfiguration()
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
                Console.WriteLine("Cannot open MsgLogger.txt for writing");
                Console.WriteLine(e.Message);
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
