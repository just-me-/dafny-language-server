using System.IO;
using System.Text;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Commons
{
    public static class LanguageServerConfig
    {
        private static string redirectedStreamFile;
        private static string logFile;

        public static string RedirectedStreamFile
        {
            get => redirectedStreamFile;
            set => redirectedStreamFile = Path.GetFullPath(value);
        }

        public static string LogFile
        {
            get => logFile;
            set => logFile = Path.GetFullPath(value);
        }

        public static LogLevel LogLevel { get; set; }
        public static TextDocumentSyncKind SyncKind { get; set; }


        public new static string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine($"Log: {LogFile}");
            s.AppendLine($"Stream: {RedirectedStreamFile}");
            s.AppendLine($"LogLevel: {LogLevel}");
            s.AppendLine($"SyncKind: {SyncKind}");
            return s.ToString();
        }

        public static void ResetDefaults()
        {
            RedirectedStreamFile = FileAndFolderLocations.defaultStreamFile;
            LogFile = FileAndFolderLocations.defaultLogFile;
            LogLevel = LogLevel.Error;
            SyncKind = TextDocumentSyncKind.Incremental;
        }
    }
}
