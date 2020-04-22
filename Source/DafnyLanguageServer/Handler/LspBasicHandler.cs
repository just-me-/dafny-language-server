using DafnyLanguageServer.FileManager;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This component provides virtual methods and general needed instance variables
    /// for Handler components that are basic LSP standard features.
    /// </summary>
    public abstract class LspBasicHandler
    {
        protected readonly ILanguageServer _router;
        protected readonly WorkspaceManager _workspaceManager;
        protected readonly DocumentSelector _documentSelector;
        protected readonly ILoggerFactory _loggingFactory;
        
        protected readonly ILogger _log;


        public LspBasicHandler(ILanguageServer router, WorkspaceManager workspaceManager, ILoggerFactory loggingFactory = null)
        {
            _router = router;
            _workspaceManager = workspaceManager;
            _documentSelector = new DocumentSelector(
                new DocumentFilter()
                {
                    Pattern = "**/*.dfy"
                });
            _loggingFactory = loggingFactory;
            _log = _loggingFactory?.CreateLogger("");
        }

        // 2do would be nice if capability could also be extracted + setter #136
    }
}
