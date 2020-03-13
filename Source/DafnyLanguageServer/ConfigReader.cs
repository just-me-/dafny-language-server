using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DafnyLanguageServer
{

    public class ConfigReader
    {
        private string[] LaunchArguments { get; set; }
        private string AssemblyPath { get; set; }
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
            SetDefaults();
            ReadConfig();
            ReadArgs();
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
                string cfgFile = Path.Combine(AssemblyPath, "LanguageServerConfig.json");
                if (!File.Exists(cfgFile))
                {
                    return;
                }
                
                JObject cfg = JObject.Parse(File.ReadAllText(cfgFile));

                var cfgLog = cfg["logging"]["log"];
                if (cfgLog != null)
                {
                    LogFile = Path.Combine(AssemblyPath, (string)cfgLog);
                }

                var cfgStream = cfg["logging"]["stream"];
                if (cfgStream != null)
                {
                    RedirectedStreamFile = Path.Combine(AssemblyPath, (string)cfgStream);
                }

                var cfgLevel = cfg["logging"]["loglevel"];
                if (cfgLevel != null)
                {
                    Loglevel = (LogLevel)(int)cfgLevel;
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
