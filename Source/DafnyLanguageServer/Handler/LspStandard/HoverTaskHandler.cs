using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>.
    /// </summary>
    public class HoverTaskHandler : LspBasicHandler<HoverCapability>, IHoverHandler
    {
        public HoverTaskHandler(ILanguageServer router, IWorkspace workspaceManager,
            ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
            _method = Resources.Requests.hover;

        }


        public async Task<Hover> Handle(HoverParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation(string.Format(Resources.LoggingMessages.request_handle, _method));

            try
            {
                var manager = _workspaceManager.SymbolTreeSymbolTree;
                var uri = request.TextDocument.Uri;
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var provider = new HoverProvider(manager);
                return await Task.Run(() => provider.GetHoverInformation(uri, line, col), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);

                return null;
            }
        }


        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }
    }
}
