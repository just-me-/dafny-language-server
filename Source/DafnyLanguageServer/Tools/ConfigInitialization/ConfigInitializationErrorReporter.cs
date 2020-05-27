using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Tools.ConfigInitialization
{
    public class ConfigInitializationErrorReporter
    {
        public ConfigInitializationErrors InitializationErrors { get; } = new ConfigInitializationErrors();

        public void AddError(Exception e)
        {
            AddError(e.Message);
            if (e.InnerException != null)
            {
                AddError(Resources.LoggingMessages.inner_error + e.InnerException.Message);
            }
        }

        public void AddError(string msg)
        {
            InitializationErrors.HasErrors = true;
            InitializationErrors.MessageCollector.AppendLine(msg);
        }
    }
}
