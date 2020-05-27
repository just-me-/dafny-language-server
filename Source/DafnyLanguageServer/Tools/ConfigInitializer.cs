using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Tools
{



    /// <summary>
    /// This is a service to set up the language server config.
    /// It is used once at the server start up. 
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
        /// <param name="launchArguments">Launch Arguments</param>
        /// <param name="jsonConfigFile">Inject a custom json here for testing</param>
        public ConfigInitializer(string[] launchArguments, string jsonConfigFile)
        {
            LaunchArguments = launchArguments;
            JSONConfigFile = jsonConfigFile;
        }

        /// <summary>
        /// This methods sets up the config.
        /// First, defaults are set.
        /// Then, they are overwritten by the JSON, if provided.
        /// Then, they are overwritten by launch arguments, if provided.
        /// <remarks>
        /// The method will run to completion and provide defaults in case of errors. 
        /// In case of errors, they are available in the <c>InitializationErrors</c> property.
        /// </remarks>
        /// </summary>
        public void SetUp()
        {
            try
            {
                ApplyDefaults();
                ApplyJSON();
                ApplyArgs();
                Validate();
            }
            catch (Exception e)
            {
                AddError(e);
                ApplyDefaults();
            }
        }

        private void Validate()
        {
            if (LanguageServerConfig.LogFile == LanguageServerConfig.RedirectedStreamFile)
            {
                throw new ArgumentException(ExceptionMessages.stream_and_log_are_same);
            }

            if ((int)LanguageServerConfig.LogLevel > 6 || (int)LanguageServerConfig.LogLevel < 0)
            {
                throw new ArgumentException(ExceptionMessages.level_out_of_bounds);

            }
        }

        private void ApplyDefaults()
        {
            LanguageServerConfig.ResetDefaults();
        }

        private void ApplyJSON()
        {

            if (!File.Exists(JSONConfigFile))
            {
                AddError(ExceptionMessages.config_file_not_existing);
                return;
            }

            JObject cfg = JObject.Parse(File.ReadAllText(JSONConfigFile));

            

            SafelyPerform(() =>
            {
                var token = cfg["logging"]["log"];
                LanguageServerConfig.LogFile = Path.Combine(FileAndFolderLocations.rootFolder, (string)token);
            });

            SafelyPerform(() =>
            {
                var token = cfg["logging"]["stream"];
                LanguageServerConfig.RedirectedStreamFile =
                        Path.Combine(FileAndFolderLocations.rootFolder, (string)token);
            });

            SafelyPerform(() =>
            {
                var token = cfg["logging"]["loglevel"];
                LanguageServerConfig.LogLevel = (LogLevel) (int)token;
            });

            SafelyPerform(() =>
            {
                var token = cfg["synckind"];
                LanguageServerConfig.SyncKind =
                        (TextDocumentSyncKind) Enum.Parse(typeof(TextDocumentSyncKind), (string)token, true);
            });

        }

        private void SafelyPerform(Action a)
        {
            try
            {
                a.Invoke();
                
            }
            catch (NullReferenceException)
            {
                //Skip if entry was null
            }
            catch (ArgumentNullException)
            {
                //Skip if entry was null
            }
            catch (Exception e)
            {
                AddError(e);
            }
        }

        private void ApplyArgs()
        {

            foreach (var arg in LaunchArguments)
            {
                try
                {
                    string[] splitted = arg.Split(':');
                    if (splitted.Length != 2)
                    {
                        AddError(ExceptionMessages.not_supported_launch_args);
                    }

                    HandleArgumentPair(splitted);
                }
                catch (Exception e)
                {
                    AddError(e);
                }
            }
        }

        private void HandleArgumentPair(IReadOnlyList<string> splitted)
        {
            string key = splitted[0];
            string value = splitted[1];

            if (value.Length < 1)
            {
                AddError(ExceptionMessages.no_arg_for_switch + key);
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
                    LanguageServerConfig.LogLevel = (LogLevel)int.Parse(value);
                    break;
                case "/synckind":
                    LanguageServerConfig.SyncKind = (TextDocumentSyncKind)Enum.Parse(typeof(TextDocumentSyncKind), value, true);
                    break;
                default:
                    AddError(string.Format(ExceptionMessages.unknown_switch, key));
                    break;
            }
        }



        private void AddError(Exception e)
        {
            AddError(e.Message);
            if (e.InnerException != null)
            {
                AddError(Resources.LoggingMessages.inner_error + e.InnerException.Message);
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
