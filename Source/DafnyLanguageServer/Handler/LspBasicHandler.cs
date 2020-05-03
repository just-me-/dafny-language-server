﻿using DafnyLanguageServer.FileManager;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

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
        protected readonly IWorkspaceManager _workspaceManager;
        protected readonly DocumentSelector _documentSelector;
        protected readonly ILoggerFactory _loggingFactory;
        protected readonly ILogger _log;

        protected T _capability; // needed by OmniSharp

        public LspBasicHandler(ILanguageServer router, IWorkspaceManager workspaceManager, ILoggerFactory loggingFactory = null)
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
        }

        public void SetCapability(T capability)
        {
            _capability = capability;
        }
    }
}
