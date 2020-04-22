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
        private const string defaultCfgFile = "LanguageServerConfig.json";
        private const string defaultStreamPath = "../Logs/StreamRedirection.txt";
        private const string defaultLogPath = "../Logs/Log.txt";


        public LanguageServerConfig Config { get; set; } = new LanguageServerConfig();


        private string[] LaunchArguments { get; set; }
        private string AssemblyPath { get; set; }
        private string PathToJSONConfigFile { get; set; }


        //Log Levels (starting from 0)
        //Trace - Debug - Info - Warning - Error - Critical - None

        public ConfigInitializer(string[] launchArguments) : this(launchArguments, defaultCfgFile) { }

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
                    throw new FileNotFoundException("Config file not found at: " + PathToJSONConfigFile);
                }

                JObject cfg = JObject.Parse(File.ReadAllText(PathToJSONConfigFile));

                var cfgLog = cfg["logging"]["log"];
                var cfgStream = cfg["logging"]["stream"];
                var cfgLevel = cfg["logging"]["loglevel"];

                if (cfgLog != null && cfgStream != null && (string)cfgStream == (string)cfgLog)
                {
                    throw new ArgumentException("StreamRedirection and Log must not be the same files");
                }

                if (cfgLog != null)
                {
                    Config.LogFile = Path.Combine(AssemblyPath, (string) cfgLog);
                }

                if (cfgStream != null)
                {
                    Config.RedirectedStreamFile = Path.Combine(AssemblyPath, (string) cfgStream);
                }

                if (cfgLevel != null)
                {
                    Config.Loglevel = (LogLevel) (int) cfgLevel;
                }

            }
            catch (NullReferenceException)
            {
                AddError("Error while parsing json config");
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
                        throw new ArgumentException("Error parsing launch arguments. Please refer to the readme.md");
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
                throw new ArgumentException("No Argument provided for switch '" + key);

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
                    throw new ArgumentException("Unknown switch: '" + value + "'. Please refer to readme.md");
            }
        }

        private void Validate()
        {
            if ((int)Config.Loglevel < 0 || (int)Config.Loglevel > 7)
            {
                AddError("Loglevel exceeds limits. Must be between 0 and 7. Setting to default Loglevel 4 = Error");
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
                AddError("Inner 3rr0r: " + e.InnerException.Message);
            }
        }

        private void AddError(string msg)
        {
            Config.Error = true;
            Config.ErrorMessages.AppendLine(msg);
        }


    }
}
