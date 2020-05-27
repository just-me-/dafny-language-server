using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Tools.ConfigInitialization
{


    /// <summary>
    /// This is a service to set up the language server config.
    /// It is used once at the server start up. 
    /// </summary>
    public class ConfigInitializer
    {
        public ConfigInitializationErrorReporter Reporter { get; } = new ConfigInitializationErrorReporter();
        public ConfigInitializationErrors Errors => Reporter.InitializationErrors;

        private string[] LaunchArguments { get; }
        private string JSONConfigFile { get; }

        //Log Levels (starting from 0)
        //Trace - Debug - Info - Warning - Error - Critical - None

        /// <summary>
        /// This constructor uses the default config files.
        /// If you want to inject a custom config.json, use the overloaded constructor.
        /// </summary>
        public ConfigInitializer(string[] launchArguments) : this(launchArguments,
            FileAndFolderLocations.languageServerJSONConfigFile)
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
                Reporter.AddError(e);
                ApplyDefaults();
            }
        }

        private void ApplyDefaults()
        {
            LanguageServerConfig.ResetDefaults();
        }

        private void ApplyJSON()
        {
            new ConfigJSONParser(Reporter, JSONConfigFile).ApplyJSON();
        }

        private void ApplyArgs()
        {
            new ConfigArgumentsParser(Reporter, LaunchArguments).ApplyArgs();
        }

        private void Validate()
        {
            if (LanguageServerConfig.LogFile == LanguageServerConfig.RedirectedStreamFile)
            {
                throw new ArgumentException(ExceptionMessages.stream_and_log_are_same);
            }

            if ((int) LanguageServerConfig.LogLevel > 6 || (int) LanguageServerConfig.LogLevel < 0)
            {
                throw new ArgumentException(ExceptionMessages.level_out_of_bounds);
            }
        }
    }
}