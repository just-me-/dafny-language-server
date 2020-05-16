using System.Text;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Tools
{
    public class LanguageServerConfig
    {
        public string RedirectedStreamFile { get; set; }
        public string LogFile { get; set; }
        public LogLevel Loglevel { get; set; }

        public bool Error { get; set; } = false;
        public string ErrorMsg => ErrorMessages.ToString();
        public StringBuilder ErrorMessages { get; } = new StringBuilder();

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine($"Log: {LogFile}");
            s.AppendLine($"Stream: {RedirectedStreamFile}");
            s.AppendLine($"LogLevel: {Loglevel}");
            s.AppendLine($"Error: {Error}");
            s.AppendLine($"ErrorMsg: {ErrorMsg}");
            return s.ToString();
        }
    }
}
