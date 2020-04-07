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
    public class DefinitionHandler : IDefinitionHandler
    {
        private DefinitionCapability _capability;
        private readonly ILanguageServer _router;
        private readonly WorkspaceManager _workspaceManager;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dfy"
            }
        );

        public DefinitionHandler(ILanguageServer router, WorkspaceManager workspaceManager)
        {
            _router = router;
            _workspaceManager = workspaceManager;
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
            return await Task.Run(() =>
            {
                List<LocationOrLocationLink> links = new List<LocationOrLocationLink>();
                var symbols = _workspaceManager.GetFileRepository(request.TextDocument.Uri).SymboleProcessor();
                var word = FileHelper.GetFollowingWord(
                    _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.Sourcecode,
                    (int)request.Position.Line,
                    (int)request.Position.Character
                );
                // todo not optimized yet - ticket #40
                foreach (var symbol in symbols.GetFullList())
                {
                    if (word == symbol.Name)
                    {
                        long column = (long) symbol.Column;
                        var positionOffset = 0; 
                        switch (symbol.SymbolType.ToString())
                        {
                            case "Class":
                                positionOffset = -1;
                                break;
                            case "Method":
                                positionOffset = -1;
                                break; 
                            case "Definition":
                                positionOffset = 1;
                                break; 
                        }
                        Position position = new Position((long)symbol.Line - 1, column + positionOffset);
                        Range range = new Range { Start = position, End = position };
                        var location = new Location { Uri = request.TextDocument.Uri, Range = range };

                        links.Add(new LocationOrLocationLink(location));
                        break;
                    }
                }
                return new LocationOrLocationLinks(links);
            });
        }

        public void SetCapability(DefinitionCapability capability)
        {
            _capability = capability;
        }
    }
}
