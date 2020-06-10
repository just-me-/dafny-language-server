using System.Text;

namespace DafnyLanguageServer.Tools.ConfigInitialization
{
    public class ConfigInitializationErrors
    {
        public bool HasErrors { get; set; }
        public StringBuilder MessageCollector { get; } = new StringBuilder();
        public string ErrorMessages => MessageCollector.ToString();
    }
}