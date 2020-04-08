using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This component provides virtual methods and general needed instance variables
    /// for Handler components that are basic LSP standard features.
    /// </summary>
    public class LspBasicHandler
    {
        protected readonly ILanguageServer _router;
        protected readonly WorkspaceManager _workspaceManager;
        protected readonly DocumentSelector _documentSelector;

        public LspBasicHandler(ILanguageServer router, WorkspaceManager workspaceManager)
        {
            _router = router;
            _workspaceManager = workspaceManager;
            _documentSelector = new DocumentSelector(
                new DocumentFilter()
                {
                    Pattern = "**/*.dfy"
                });
        }

        // 2do would be nice if capability could also be extracted + setter #136
    }
}
