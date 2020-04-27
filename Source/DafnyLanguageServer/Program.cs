using Microsoft.Boogie;
using System.Threading.Tasks;

namespace DafnyLanguageServer
{
    /// <summary>
    /// This class provides the <C>Main</C> function.
    /// It starts the Omnisharp Language Server.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            DafnyLanguageServer languageServer =  new DafnyLanguageServer(args);
            await languageServer.StartServer();
        }
    }
}
