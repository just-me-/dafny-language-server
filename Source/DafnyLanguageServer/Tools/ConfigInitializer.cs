using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Tools
{
    /// <summary>
    /// This is a reader service for the server configuration file. It is used once by the server start up. 
    /// </summary>
    public class ConfigInitializer
    {
        public ConfigInitializationErrors InitializationErrors { get; } = new ConfigInitializationErrors();


        private string[] LaunchArguments { get; }
        private string JSONConfigFile { get; }

        //Log Levels (starting from 0)
        //Trace - Debug - Info - Warning - Error - Critical - None

        /// <summary>
        /// This constructor uses the default config files.
        /// If you want to inject a custom config.json, use the overloaded constructor.
        /// </summary>
        public ConfigInitializer(string[] launchArguments) : this(launchArguments, FileAndFolderLocations.languageServerJSONConfigFile)
        {
        }

        /// <summary>
        /// This constructor allows injection of a custom json config file for testing.
        /// </summary>
        /// <param name="jsonConfigFile">Inject a custom json here for testing</param>
        public ConfigInitializer(string[] launchArguments, string jsonConfigFile)
        {
            LaunchArguments = launchArguments;
            JSONConfigFile = jsonConfigFile;
        }

        public void SetUp()
        {
            try
            {
                SetDefaults();
                ReadJSONConfig();
                ReadArgs();
                Validate();
                ImprovePathLayout();
            }
            catch (Exception e)
            {
                AddExceptionToError(e);
            }
        }

        private void SetDefaults()
        {
            LanguageServerConfig.ResetDefaults();
        }

        private void ReadJSONConfig()
        {
            try
            {
                if (!File.Exists(JSONConfigFile))
                {
                    throw new FileNotFoundException(ExceptionMessages.config_file_not_found + " " + JSONConfigFile);
                }

                JObject cfg = JObject.Parse(File.ReadAllText(JSONConfigFile));

                var cfgLog = cfg["logging"]["log"];
                var cfgStream = cfg["logging"]["stream"];
                var cfgLevel = cfg["logging"]["loglevel"];
                var syncKind = cfg["syncKind"];

                if (cfgLog != null && cfgStream != null && (string)cfgStream == (string)cfgLog)
                {
                    throw new ArgumentException(ExceptionMessages.stream_and_log_are_same);
                }

                if (cfgLog != null)
                {
                    LanguageServerConfig.LogFile = Path.Combine(FileAndFolderLocations.rootFolder, (string)cfgLog);
                }

                if (cfgStream != null)
                {
                    LanguageServerConfig.RedirectedStreamFile = Path.Combine(FileAndFolderLocations.rootFolder, (string)cfgStream);
                }

                if (cfgLevel != null)
                {
                    LanguageServerConfig.LogLevel = (LogLevel)(int)cfgLevel;
                }

                if (syncKind != null)
                {
                    LanguageServerConfig.SyncKind = (TextDocumentSyncKind)Enum.Parse(typeof(TextDocumentSyncKind), (string)syncKind, true);
                }

            }
            catch (NullReferenceException)
            {
                AddError(ExceptionMessages.config_could_not_be_parsed);
            }
            catch (Exception e)
            {
                AddExceptionToError(e);
            }
        }

        private void ReadArgs()
        {
            try
            {
                foreach (var arg in LaunchArguments)
                {
                    string[] splitted = arg.Split(':');
                    if (splitted.Length != 2)
                    {
                        throw new ArgumentException(ExceptionMessages.not_supported_launch_args);
                    }
                    HandleArgumentPair(splitted);
                }
            }
            catch (Exception e)
            {
                AddExceptionToError(e);
            }
        }

        private void HandleArgumentPair(IReadOnlyList<string> splitted)
        {
            string key = splitted[0];
            string value = splitted[1];

            if (value.Length < 1)
            {
                throw new ArgumentException("No Argument provided for switch " + key); // todo lang file #102

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
                    LanguageServerConfig.LogLevel = (LogLevel)int.Parse(value);
                    break;
                case "/synckind":
                    LanguageServerConfig.SyncKind = (TextDocumentSyncKind)Enum.Parse(typeof(TextDocumentSyncKind), value, true);
                    break;
                default:
                    throw new ArgumentException("Unknown switch: '" + key + "'. Please refer to readme.md"); // todo lang file #102
            }
        }

        private void Validate()
        {
            if (LanguageServerConfig.LogLevel < 0 || (int)LanguageServerConfig.LogLevel > 5)
            {
                AddError("LogLevel exceeds limits. Must be between 0 and 6. Setting to default LogLevel 4 = Error"); // todo lang file #102
                LanguageServerConfig.LogLevel = LogLevel.Error;
            }
        }

        private void ImprovePathLayout()
        {
            LanguageServerConfig.RedirectedStreamFile = Path.GetFullPath(LanguageServerConfig.RedirectedStreamFile);
            LanguageServerConfig.LogFile = Path.GetFullPath(LanguageServerConfig.LogFile);
        }


        private void AddExceptionToError(Exception e)
        {
            AddError(e.Message);
            if (e.InnerException != null)
            {
                AddError("Inner error: " + e.InnerException.Message); // todo lang file #102
            }
        }

        private void AddError(string msg)
        {
            InitializationErrors.HasErrors = true;
            InitializationErrors.MessageCollector.AppendLine(msg);
        }


    }

    public class ConfigInitializationErrors
    {
        public bool HasErrors { get; set; }
        public StringBuilder MessageCollector { get; } = new StringBuilder();
        public string ErrorMessages => MessageCollector.ToString();


    }
}
