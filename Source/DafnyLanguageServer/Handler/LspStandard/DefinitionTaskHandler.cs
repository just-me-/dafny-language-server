using System;
using System.Collections;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.Tools;
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
        public DefinitionTaskHandler(ILanguageServer router, Workspace workspaceManager, ILoggerFactory loggingFactory)
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


            return await Task.Run(() =>
            {

                var uri = request.TextDocument.Uri;
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;

                var manager = _workspaceManager.SymbolTableManager;
                try
                {
                    var provider = new DefinitionsProvider(manager);
                    var result = provider.GetDefinitionLocation(uri, line, col);

                    if (provider.Outcome == DefinitionsOutcome.NotFound)
                    {
                        _log.LogWarning("No Defintion found for " + request.TextDocument.Uri + $" at L{line}:C{col}"); // todo lang file #102
                        _mss.SendInformation("No Defintion found for " + request.TextDocument.Uri + $" at L{line}:C{col}"); // todo lang file #102
                    }
                    if (provider.Outcome == DefinitionsOutcome.WasAlreadyDefintion)
                    {
                        _mss.SendInformation("This is already the definition."); // todo lang file #102
                    }
                    return result;
                }

                catch (Exception e)
                {
                    _log.LogError("Internal server error handling Definition: " + e.Message); // todo lang file #102
                    _mss.SendError("Internal server error handling Definition: " + e.Message);// todo lang file #102
                    return null;
                }
            });

        }
    }
}
