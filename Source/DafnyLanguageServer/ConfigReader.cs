using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace DafnyLanguageServer
{

    public class ConfigReader
    {
        private const string defaultCfgFile = "LanguageServerConfig.json";

        private string[] LaunchArguments { get; set; }
        private string AssemblyPath { get; set; }
        private string ConfigFile { get; set; }

        public string RedirectedStreamFile { get; private set; }
        public string LogFile { get; private set; }
        public LogLevel Loglevel { get; private set; }

        public bool Error { get; private set; } = false;
        public string ErrorMsg { get; private set; } = "";

        //Log Levels (starting from 0)
        //Trace - Debug - Info - Warning - Error - Critical - None

        public ConfigReader(string[] launchArguments)
        {
            LaunchArguments = launchArguments;
            AssemblyPath = Path.GetDirectoryName(typeof(ConfigReader).Assembly.Location);
            ConfigFile = Path.Combine(AssemblyPath, defaultCfgFile);
            SetProperties();
        }

        public ConfigReader(string[] launchArguments, string configFile)
        {
            LaunchArguments = launchArguments;
            AssemblyPath = Path.GetDirectoryName(typeof(ConfigReader).Assembly.Location);
            ConfigFile = configFile;
            SetProperties();
        }

        private void SetProperties()
        {
            SetDefaults();
            ReadConfig();
            ReadArgs();
            Validate();
            ImprovePathLayout();

        }


        private void SetDefaults()
        {
            RedirectedStreamFile = Path.Combine(AssemblyPath, "../Logs/StreamRedirection.txt");
            LogFile = Path.Combine(AssemblyPath, "../Logs/Log.txt");
            Loglevel = LogLevel.Error;
        } 

        private void ReadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFile))
                {
                    throw new FileNotFoundException("Config file not found at: " + ConfigFile);
                }

                JObject cfg = JObject.Parse(File.ReadAllText(ConfigFile));

                var cfgLog = cfg["logging"]["log"];
                var cfgStream = cfg["logging"]["stream"];
                var cfgLevel = cfg["logging"]["loglevel"];



                if (cfgLog != null && cfgStream != null && (string)cfgStream == (string)cfgLog)
                {
                    throw new ArgumentException("StreamRedirection and Log must not be the same files");
                }

                if (cfgLog != null)
                {
                    LogFile = Path.Combine(AssemblyPath, (string) cfgLog);
                }

                if (cfgStream != null)
                {

                    RedirectedStreamFile = Path.Combine(AssemblyPath, (string) cfgStream);
                }

                if (cfgLevel != null)
                {
                    Loglevel = (LogLevel) (int) cfgLevel;
                }

            }
            catch (NullReferenceException)
            {
                Error = true;
                ErrorMsg += "\nError while parsing json config";
            }
            catch (Exception e)
            {
                Error = true;
                ErrorMsg += "\n" + e.Message;
                if (e.InnerException != null)
                {
                    ErrorMsg += "\n\nInner Exception:\n" + e.InnerException.Message;
                }
            }
        }

        private void ReadArgs()
        {
            try
            {
                if (LaunchArguments.Length % 2 != 0)
                {
                    throw new ArgumentException("Invalid number of arguments provided. Must be dividable by 2.");
                }

                for (int i = 0; i < LaunchArguments.Length; i += 2)
                {
                    switch (LaunchArguments[i].ToLower())
                    {
                        case "/stream":
                            RedirectedStreamFile = Path.Combine(AssemblyPath, LaunchArguments[i + 1]);
                            break;
                        case "/log":
                            LogFile = Path.Combine(AssemblyPath, LaunchArguments[i + 1]);
                            break;
                        case "/loglevel":
                            Loglevel = (LogLevel) int.Parse(LaunchArguments[i + 1]);
                            break;
                        default:
                            throw new ArgumentException("Unkown Parameter: " + LaunchArguments[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Error = true;
                ErrorMsg += "\n" + e.Message;
                if (e.InnerException != null)
                {
                    ErrorMsg += "\n\nInner Exception:\n" + e.InnerException.Message;
                }
            }
        }

        private void ImprovePathLayout()
        {
            RedirectedStreamFile = Path.GetFullPath(RedirectedStreamFile);
            LogFile = Path.GetFullPath(LogFile);
        }

        private void Validate()
        {
            if ((int)Loglevel < 0 || (int)Loglevel > 7)
            {
                Error = true;
                ErrorMsg += "\nLoglevel exceeds limits. Must be between 0 and 7. Setting to default Loglevel 4";
                Loglevel = LogLevel.Error;
            }

        
        }
    

        public void PrintState()
        {
            Console.WriteLine($"Log: {LogFile}");
            Console.WriteLine($"Stream: {RedirectedStreamFile}");
            Console.WriteLine($"LogLevel: {Loglevel}");
            Console.WriteLine($"Error: {Error}");
            Console.WriteLine($"ErrorMsg: {ErrorMsg}");
        }
    }
}
