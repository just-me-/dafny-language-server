using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Tools.ConfigInitialization
{
    /// <summary>
    /// This class tries to parse the provided JSON file.
    /// The results are directly saved into the global config entity
    /// Errors are reporter to the ErrorReporter.
    /// </summary>
    public class ConfigJSONParser
    {
        private ConfigInitializationErrorReporter Reporter { get; }
        private string JSONConfigFile { get; }

        public ConfigJSONParser(ConfigInitializationErrorReporter r, string json)
        {
            Reporter = r;
            JSONConfigFile = json;
        }

        public void ApplyJSON()
        {

            if (!File.Exists(JSONConfigFile))
            {
                Reporter.AddError(ExceptionMessages.config_file_not_existing);
                return;
            }

            JObject baseToken = JObject.Parse(File.ReadAllText(JSONConfigFile));

            SafelyPerform((token) =>
            {
                LanguageServerConfig.LogFile = Path.Combine(FileAndFolderLocations.rootFolder, (string)token);
            }, baseToken, "logging", "log");

            SafelyPerform((token) =>
            {
                LanguageServerConfig.RedirectedStreamFile =
                    Path.Combine(FileAndFolderLocations.rootFolder, (string)token);
            }, baseToken, "logging", "stream");


            SafelyPerform((token) =>
            {
                LanguageServerConfig.LogLevel = (LogLevel)(int)token;
            }, baseToken, "logging", "loglevel");


            SafelyPerform((token) =>
            {
                LanguageServerConfig.SyncKind = (TextDocumentSyncKind)Enum.Parse(typeof(TextDocumentSyncKind), (string)token, true);
            }, baseToken, "synckind");


        }

        private void SafelyPerform(Action<JToken> a, JToken baseToken, params string[] subTokens)
        {
            var token = baseToken;
            try
            {
                foreach (string st in subTokens)
                {
                    token = token[st];
                    if (token == null)
                    {
                        return;
                    }
                }
                a.Invoke(token);

            }
            catch (Exception e)
            {
                Reporter.AddError(e);
            }
        }
    }
}
