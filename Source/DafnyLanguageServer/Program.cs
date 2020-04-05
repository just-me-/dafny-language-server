using Microsoft.Boogie;
using System.Threading.Tasks;

namespace DafnyLanguageServer
{
    /// <summary>
    /// This class provides the <c>Main</c> function.
    /// It starts the Omnisharp Language Server.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            ExecutionEngine.printer = new ConsolePrinter();

            DafnyLanguageServer languageServer =  new DafnyLanguageServer(args);
            await languageServer.StartServer();
        }
    }
}
