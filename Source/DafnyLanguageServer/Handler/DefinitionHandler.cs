using DafnyLanguageServer.ContentManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Handler
{
    public class DefinitionHandler : IDefinitionHandler
    {
        private DefinitionCapability _capability;
        private readonly ILanguageServer _router;
        private readonly BufferManager _bufferManager;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dfy"
            }
        );

        public DefinitionHandler(ILanguageServer router, BufferManager bufferManager)
        {
            _router = router;
            _bufferManager = bufferManager;
        }

        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        public async Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                List<LocationOrLocationLink> links = new List<LocationOrLocationLink>();
                var symbols = _bufferManager.GetSymboltable(request.TextDocument.Uri);
                var word = FileHelper.GetFollowingWord(
                    _bufferManager.GetSourceCodeAsText(request.TextDocument.Uri),
                    (int)request.Position.Line,
                    (int)request.Position.Character
                );
                // not optimized yet - ticket #40
                foreach (var symbol in symbols.GetFullList())
                {
                    if (word == symbol.Name)
                    {
                        Position position = new Position((long)symbol.Line - 1, (long)symbol.Column);
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
