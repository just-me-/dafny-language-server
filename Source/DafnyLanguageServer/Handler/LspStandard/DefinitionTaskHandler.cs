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
    /// This handler provides the <c>go to definition</c> position for symbols.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>GoToDefinitionServiceProvider</c>.
    /// </summary>
    public class DefinitionTaskHandler : LspBasicHandler<DefinitionCapability>, IDefinitionHandler
    {
        public DefinitionTaskHandler(ILanguageServer router, IWorkspace workspaceManager, ILoggerFactory loggingFactory)
        : base(router, workspaceManager, loggingFactory)
        {
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
            _log.LogInformation("Handling Goto Definition..."); // todo lang file #102

            try
            {
                var uri = request.TextDocument.Uri;
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var manager = _workspaceManager.SymbolTableManager;
                var provider = new DefinitionsProvider(manager);
                return await Task.Run(() => RunAndEvaluate(provider, uri, line, col), cancellationToken);
            }

            catch (Exception e)
            {
                var msg = "Internal server error handling Definition"; //todo lang
                HandleError(msg, e);
                return null;
            }

        }

        private LocationOrLocationLinks RunAndEvaluate(DefinitionsProvider provider, Uri uri, int line, int col)
        {
            var result = provider.GetDefinitionLocation(uri, line, col);

            switch (provider.Outcome)
            {
                case DefinitionsOutcome.NotFound:
                    string msg = "No Defintion found for " + uri + $" at L{line}:C{col}";
                    _log.LogWarning(msg); // todo lang file #102
                    _mss.SendInformation(msg); // todo lang file #102
                    break;
                case DefinitionsOutcome.WasAlreadyDefintion:
                    _mss.SendInformation("This is already the definition."); // todo lang file #102
                    break;
            }

            return result;
        }
    }
}
