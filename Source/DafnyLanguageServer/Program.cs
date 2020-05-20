using System.Threading.Tasks;

namespace DafnyLanguageServer
{
    /// <summary>
    /// This class provides the <C>Main</C> function.
    /// It starts the Omnisharp Language Server.
    /// </summary>
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            DafnyLanguageServer languageServer = new DafnyLanguageServer(args);
            await languageServer.StartServer();
        }
    }
}
