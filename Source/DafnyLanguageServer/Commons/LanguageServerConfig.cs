using System.Security.RightsManagement;
using System.Text;
using DafnyLanguageServer.Resources;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DafnyLanguageServer.Commons
{
    public static class LanguageServerConfig
    {
        public static string RedirectedStreamFile { get; set; }
        public static string LogFile { get; set; }
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
            RedirectedStreamFile = FileAndFolderLocator.defaultStreamFile;
            LogFile = FileAndFolderLocator.defaultLogFile;
            LogLevel = LogLevel.Error;
            SyncKind = TextDocumentSyncKind.Incremental;
        }
    }
}
