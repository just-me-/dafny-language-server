using System;
using System.Collections.Generic;
using System.IO;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Tools.ConfigInitialization
{
    /// <summary>
    /// This class tries to parse the launch arguments provided by the client,
    /// for example <c>/log:Logs/log.txt /loglevel:3</c>
    /// The results are directly saved into the global config entity
    /// Errors are reporter to the ErrorReporter.
    /// </summary>
    public class ConfigArgumentsParser
    {
        private ConfigInitializationErrorReporter Reporter { get; }
        private string[] LaunchArguments { get; }

        public ConfigArgumentsParser(ConfigInitializationErrorReporter r, string[] args)
        {
            Reporter = r;
            LaunchArguments = args;
        }

        public void ApplyArgs()
        {
            foreach (var arg in LaunchArguments)
            {
                try
                {
                    string[] splitted = arg.Split(':');
                    if (splitted.Length != 2)
                    {
                        Reporter.AddError(ExceptionMessages.not_supported_launch_args);
                    }

                    HandleArgumentPair(splitted);
                }
                catch (Exception e)
                {
                    Reporter.AddError(e);
                }
            }
        }

        private void HandleArgumentPair(IReadOnlyList<string> splitted)
        {
            string key = splitted[0];
            string value = splitted[1];

            if (value.Length < 1)
            {
                Reporter.AddError(ExceptionMessages.no_arg_for_switch + key);
                return;

            }

            switch (key.ToLower())
            {
                case "/stream":
                    LanguageServerConfig.RedirectedStreamFile = Path.Combine(FileAndFolderLocations.rootFolder, value);
                    break;
                case "/log":
                    LanguageServerConfig.LogFile = Path.Combine(FileAndFolderLocations.rootFolder, value);
                    break;
                case "/loglevel":
                    LanguageServerConfig.LogLevel = (LogLevel) int.Parse(value);
                    break;
                case "/synckind":
                    LanguageServerConfig.SyncKind =
                        (TextDocumentSyncKind) Enum.Parse(typeof(TextDocumentSyncKind), value, true);
                    break;
                default:
                    Reporter.AddError(string.Format(ExceptionMessages.unknown_switch, key));
                    break;
            }
        }
    }
}
