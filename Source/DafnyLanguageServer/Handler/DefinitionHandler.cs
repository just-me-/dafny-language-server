using System;
using System.Collections;
using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.ProgramServices;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides the <c>go to definition</c> position for symbols.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>GoToDefinitionServiceProvider</c>.
    /// </summary>
    public class DefinitionHandler : LspBasicHandler<DefinitionCapability>, IDefinitionHandler
    {
        public DefinitionHandler(ILanguageServer router, WorkspaceManager workspaceManager, ILoggerFactory loggingFactory)
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
            _log.LogInformation("Handling Goto Definition...");

            try
            {
                return await Task.Run(() =>
                {
                    List<LocationOrLocationLink> links = new List<LocationOrLocationLink>();
                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;
                    var manager = _workspaceManager.SymbolTableManager;
                    var selectedSymbol = manager.GetSymbolByPosition(line, col);

                    if (selectedSymbol == null)
                    {
                        throw new ArgumentNullException($"Could not find symbol at position {line}:{col}");
                    }

                    var originSymbol = manager.GetOriginFromSymbol(selectedSymbol);

                    Position position = new Position((long)originSymbol.Line - 1, (long)originSymbol.ColumnStart - 1);
                    Range range = new Range { Start = position, End = position };
                    var location = new Location { Uri = request.TextDocument.Uri, Range = range };

                    links.Add(new LocationOrLocationLink(location));

                    return new LocationOrLocationLinks(links);
                });
            }

            catch (Exception e)
            {
                _log.LogError("Internal server error handling Definition: " + e.Message);
                new MessageSenderService(_router).SendError("Internal server error handling Definition: " + e.Message);

                return null;
            }
        }
    }
}
