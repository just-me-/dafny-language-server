using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides the <c>go to definition</c> position for symbols.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>GoToDefinitionServiceProvider</c>.
    /// </summary>
    public class DefinitionTaskHandler : LspBasicHandler<DefinitionCapability>, IDefinitionHandler
    {
        public DefinitionTaskHandler(ILanguageServer router, IWorkspace workspaceManager, ILoggerFactory loggingFactory)
        : base(router, workspaceManager, loggingFactory)
        {
            _method = Resources.Requests.definitions;

        }

        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        public async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation(string.Format(Resources.LoggingMessages.request_handle, _method));

            try
            {
                var uri = request.TextDocument.Uri;
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var manager = _workspaceManager.GetFileRepository(request.TextDocument.Uri).SymbolTableManager;
                var provider = new DefinitionsProvider(manager);
                return await Task.Run(() => RunAndEvaluate(provider, uri, line, col), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);
                return null;
            }

        }

        private LocationOrLocationLinks RunAndEvaluate(DefinitionsProvider provider, Uri uri, int line, int col)
        {
            var result = provider.GetDefinitionLocation(uri, line, col);

            switch (provider.Outcome)
            {
                case DefinitionsOutcome.NotFound:
                    _log.LogWarning(string.Format(Resources.LoggingMessages.goto_notfound_at, uri, line, col));
                    _mss.SendInformation(Resources.LoggingMessages.goto_notfound);
                    if (!_workspaceManager.GetFileRepository(uri).Result.IsVerified)
                    {
                        _mss.SendWarning(Resources.LoggingMessages.goto_fileNotValid);
                    }
                    break;
                case DefinitionsOutcome.WasAlreadyDefintion:
                    _mss.SendInformation(Resources.LoggingMessages.goto_alreadyIsDef);
                    break;
            }
            return result;
        }
    }
}
