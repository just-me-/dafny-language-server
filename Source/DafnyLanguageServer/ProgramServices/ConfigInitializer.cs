using System;
using System.IO;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DafnyLanguageServer.ProgramServices
{
    /// <summary>
    /// This is a reader service for the server configuration file. It is used once by the server start up. 
    /// </summary>
    public class ConfigInitializer
    {
        private const string defaultCfgFile = "./LanguageServerConfig.json"; // todo das evt auch konfigurierbar? ... #105
        private const string defaultStreamPath = "../Logs/StreamRedirection.txt";
        private const string defaultLogPath = "../Logs/Log.txt";

        public LanguageServerConfig Config { get; set; } = new LanguageServerConfig();

        private string[] LaunchArguments { get; }
        private string AssemblyPath { get; set; }
        private string PathToJSONConfigFile { get; }

        //Log Levels (starting from 0)
        //Trace - Debug - Info - Warning - Error - Critical - None

        public ConfigInitializer(string[] launchArguments)
        {
            LaunchArguments = launchArguments;
            AssemblyPath = Path.GetDirectoryName(typeof(ConfigInitializer).Assembly.Location);
            PathToJSONConfigFile = Path.Combine(AssemblyPath, defaultCfgFile);
            SetUp();
        }

        public ConfigInitializer(string[] launchArguments, string pathToJsonConfigFile)
        {
            LaunchArguments = launchArguments;
            AssemblyPath = Path.GetDirectoryName(typeof(ConfigInitializer).Assembly.Location);
            PathToJSONConfigFile = pathToJsonConfigFile;
            SetUp();
        }

        private void SetUp()
        {
            SetDefaults();
            ReadJSONConfig();
            ReadArgs();
            Validate();
            ImprovePathLayout();
        }

        private void SetDefaults()
        {
            Config.RedirectedStreamFile = Path.Combine(AssemblyPath, defaultStreamPath);
            Config.LogFile = Path.Combine(AssemblyPath, defaultLogPath);
            Config.Loglevel = LogLevel.Error;
        }

        private void ReadJSONConfig()
        {
            try
            {
                if (!File.Exists(PathToJSONConfigFile))
                {
                    throw new FileNotFoundException(Resources.ExceptionMessages.config_file_not_found + " " + PathToJSONConfigFile); // todo lang file #102
                }

                JObject cfg = JObject.Parse(File.ReadAllText(PathToJSONConfigFile));

                var cfgLog = cfg["logging"]["log"];
                var cfgStream = cfg["logging"]["stream"];
                var cfgLevel = cfg["logging"]["loglevel"];

                if (cfgLog != null && cfgStream != null && (string)cfgStream == (string)cfgLog)
                {
                    throw new ArgumentException(Resources.ExceptionMessages.stream_and_log_are_same);
                }

                if (cfgLog != null)
                {
                    Config.LogFile = Path.Combine(AssemblyPath, (string)cfgLog);
                }

                if (cfgStream != null)
                {
                    Config.RedirectedStreamFile = Path.Combine(AssemblyPath, (string)cfgStream);
                }

                if (cfgLevel != null)
                {
                    Config.Loglevel = (LogLevel)(int)cfgLevel;
                }

            }
            catch (NullReferenceException)
            {
                AddError(Resources.ExceptionMessages.config_could_not_be_parsed);
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
                        throw new ArgumentException(Resources.ExceptionMessages.not_supported_launch_args);
                    }
                    HandleArgumentPair(splitted);
                }
            }
            catch (Exception e)
            {
                AddExceptionToError(e);
            }
        }

        private void HandleArgumentPair(string[] splitted)
        {
            string key = splitted[0];
            string value = splitted[1];

            if (value.Length < 1)
            {
                throw new ArgumentException("No Argument provided for switch '" + key); // todo lang file #102

            }

            switch (key.ToLower())
            {
                case "/stream":
                    Config.RedirectedStreamFile = Path.Combine(AssemblyPath, value);
                    break;
                case "/log":
                    Config.LogFile = Path.Combine(AssemblyPath, value);
                    break;
                case "/loglevel":
                    Config.Loglevel = (LogLevel)int.Parse(value);
                    break;
                default:
                    throw new ArgumentException("Unknown switch: '" + key + "'. Please refer to readme.md"); // todo lang file #102
            }
        }

        private void Validate()
        {
            if ((int)Config.Loglevel < 0 || (int)Config.Loglevel > 7)
            {
                AddError("Loglevel exceeds limits. Must be between 0 and 7. Setting to default Loglevel 4 = Error"); // todo lang file #102
                Config.Loglevel = LogLevel.Error;
            }
        }

        private void ImprovePathLayout()
        {
            Config.RedirectedStreamFile = Path.GetFullPath(Config.RedirectedStreamFile);
            Config.LogFile = Path.GetFullPath(Config.LogFile);
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
            Config.Error = true;
            Config.ErrorMessages.AppendLine(msg);
        }
    }
}
