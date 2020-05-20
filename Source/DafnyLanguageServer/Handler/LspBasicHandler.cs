using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This component provides virtual methods and general needed instance variables
    /// for Handler components that are basic LSP standard features.
    /// For same - but small different typed - code (like capability) this class is generic.
    /// </summary>
    public class LspBasicHandler<T>
    {
        protected readonly ILanguageServer _router;
        protected readonly IWorkspace _workspaceManager;
        protected readonly DocumentSelector _documentSelector;
        protected readonly ILoggerFactory _loggingFactory;
        protected readonly ILogger _log;
        protected readonly MessageSenderService _mss;

        protected T _capability; // needed by OmniSharp
        protected T Capability => _capability;

        public LspBasicHandler(ILanguageServer router, IWorkspace workspaceManager, ILoggerFactory loggingFactory = null)
        {
            _router = router;
            _workspaceManager = workspaceManager;
            _documentSelector = new DocumentSelector(
                new DocumentFilter()
                {
                    Pattern = Resources.ConfigurationStrings.file_watch_pattern
                });
            _loggingFactory = loggingFactory;
            _log = _loggingFactory?.CreateLogger("");
            _mss = new MessageSenderService(_router);
        }

        public void SetCapability(T capability)
        {
            _capability = capability;
        }

        protected void HandleError(string msg, Exception e)
        {
            _log.LogError(msg + "\nException: " + e.Message + "\nStackTrace: " + e.StackTrace);
            _mss.SendError(msg);
        }
    }
}
