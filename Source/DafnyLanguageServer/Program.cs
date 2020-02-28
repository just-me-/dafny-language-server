using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Handler;
using Microsoft.Boogie;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using DafnyConsolePrinter = DafnyLanguageServer.DafnyAccess.DafnyConsolePrinter;

namespace DafnyLanguageServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ExecutionEngine.printer = new DafnyConsolePrinter();

            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithLoggerFactory(new LoggerFactory())
                    .AddDefaultLoggingProvider()
                    .WithMinimumLogLevel(LogLevel.Trace)
                    .WithServices(ConfigureServices)

                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<CompletionHandler>()
                    .WithHandler<CompileHandler>()
                    .WithHandler<CounterExampleHandler>()
                    .WithHandler<CodeLensHandler>()
                    .WithHandler<DefinitionHandler>()
            );

            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "./Dafny");
                Directory.CreateDirectory(tempPath);
                string path = Path.Combine(tempPath, "./MsgLogger.txt");

                using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write)))
                {
                    Console.SetOut(writer);
                    await server.WaitForExit;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open MsgLogger.txt for writing");
                Console.WriteLine(e.Message);
            }
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BufferManager>();
        }
    }
}
