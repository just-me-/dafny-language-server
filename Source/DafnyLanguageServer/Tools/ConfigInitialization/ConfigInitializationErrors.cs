using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Tools.ConfigInitialization
{
    public class ConfigInitializationErrors
    {
        public bool HasErrors { get; set; }
        public StringBuilder MessageCollector { get; } = new StringBuilder();
        public string ErrorMessages => MessageCollector.ToString();
    }
}
