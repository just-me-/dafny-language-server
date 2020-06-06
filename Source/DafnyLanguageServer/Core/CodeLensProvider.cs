using System;
using System.Collections.Generic;
using System.Linq;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.WorkspaceManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the related handler and contains the core logic.
    /// </summary>
    public class CodeLensProvider : ICodeLensProvider
    {
        private readonly ISymbolTableManager _manager;
        private readonly Uri _uri;

        public CodeLensProvider(IFileRepository filerepo)
        {
            _manager = filerepo.SymbolTableManager;
            _uri = filerepo.PhysicalFile.Uri;
        }

        public CodeLensContainer GetCodeLensContainer()
        {
            List<CodeLens> items = new List<CodeLens>();
            foreach (var symbolInformation in _manager.GetAllSymbolDeclarations())
            {
                if (_uri.Equals(symbolInformation.FileUri))
                {
                    items.Add(CreateCodeLensItem(symbolInformation));
                }
            }
            return new CodeLensContainer(items);
        }

        private CodeLens CreateCodeLensItem(ISymbolInformation symbolInformation)
        {
            Position position = new Position((long)symbolInformation.Line - 1, 0);
            Range range = new Range { Start = position, End = position };

            var locations = new List<Location>();
            if (symbolInformation.Usages.Any())
            {
                foreach (var usage in symbolInformation.Usages)
                {
                    var startPos = new Position
                    {
                        Line = usage.Line - 1,
                        Character = usage.Column - 1
                    };
                    var endPos = new Position
                    {
                        Line = usage.Line - 1,
                        Character = usage.IdentifierEndColumn - 1
                    };
                    locations.Add(new Location
                    {
                        Uri = usage.FileUri,
                        Range = new Range(startPos, endPos)
                    });
                }
            }

            string msgTitle;
            if (symbolInformation.Usages.Any())
            {
                if (symbolInformation.Usages.Count == 1)
                {
                    msgTitle = Resources.CodeLensMessages.one_ref + symbolInformation.Name;
                }
                else
                {
                    msgTitle = symbolInformation.Usages.Count + Resources.CodeLensMessages.multi_refs + symbolInformation.Name;
                }
            }
            else
            {
                msgTitle = Resources.CodeLensMessages.not_used + symbolInformation.Name + "?";
            }

            var args = new
            {
                Uri = _uri,
                Position = position,  // where to show the CodeLens popup
                Locations = locations // what should be displayed inside the popup 
            };
            Command command = new Command
            {
                Title = msgTitle,
                Name = Resources.Requests.showRefReq,
                Arguments = new JArray(JsonConvert.SerializeObject(args))
            };
            return new CodeLens
            {
                Data = _uri,
                Range = range,
                Command = command
            };
        }
    }
}
