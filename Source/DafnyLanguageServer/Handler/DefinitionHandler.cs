using System.Collections;
using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides the <c>go to definition</c> position for symbols.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>GoToDefinitionServiceProvider</c>. 
    /// </summary>
    public class DefinitionHandler : LspBasicHandler<DefinitionCapability>, IDefinitionHandler
    {
        public DefinitionHandler(ILanguageServer router, WorkspaceManager workspaceManager)
        : base(router, workspaceManager)
        {
        }

        public  TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        public async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                List<LocationOrLocationLink> links = new List<LocationOrLocationLink>();
                
                var manager = _workspaceManager.SymbolTableManager;
                var selectedSymbol =
                    manager.GetSymbolByPosition((int) request.Position.Line+1, (int) request.Position.Character+1);
                if(selectedSymbol != null) { 
                    var originSymbol = manager.GetOriginFromSymbol(selectedSymbol);

                    var positionOffset = 0;
                    switch (originSymbol.Type.ToString()) // 2do use enums... 
                    {
                        case "Variable":
                            positionOffset = -1;
                            break;
                    }
                    Position position = new Position((long)originSymbol.LineStart - 1, (long)originSymbol.ColumnStart + positionOffset);
                    Range range = new Range { Start = position, End = position };
                    var location = new Location { Uri = request.TextDocument.Uri, Range = range };

                    links.Add(new LocationOrLocationLink(location));
                }
                return new LocationOrLocationLinks(links);
            });
        }
    }
}
