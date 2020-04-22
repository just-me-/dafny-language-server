using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.ProgramServices
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
            string result = $"Log: {LogFile}";
            result += $"Stream: {RedirectedStreamFile}";
            result += $"LogLevel: {Loglevel}";
            result += $"Error: {Error}";
            result += $"ErrorMsg: {ErrorMsg}";
            return result;
        }


    }
}
