﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public class CodeLensProvider : ICodeLensProvider
    {
        private IManager _manager;
        private Uri _uri;

        public CodeLensProvider(IManager manager, Uri uri)
        {
            this._manager = manager;
            this._uri = uri;
        }

        public CodeLensContainer GetCodeLensContainer()
        {
            List<CodeLens> items = new List<CodeLens>();

            foreach (var symbolInformation in _manager.GetAllSymbolDeclarations())
            {
                items.Add(CreateCodeLensItem(symbolInformation));
            }

            return new CodeLensContainer(items);
        }

        private CodeLens CreateCodeLensItem(ISymbol symbolInformation)
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
                        Line = usage.Position.BodyStartToken.line - 1,
                        Character = usage.Position.BodyStartToken.col
                    };
                    var endPos = new Position
                    {
                        Line = usage.Position.BodyEndToken.line - 1,
                        Character = usage.Position.BodyEndToken.col
                    };
                    locations.Add(new Location
                    {
                        Uri = usage.File,
                        Range = new Range(startPos, endPos)
                    });
                }
            }

            // todo lang file #102
            // =?: operator
            string msgTitle;
            if (symbolInformation.Usages.Any())
            {
                if (symbolInformation.Usages.Count == 1)
                {
                    msgTitle = $"1 reference to {symbolInformation.Name}";
                }
                else
                {
                    msgTitle = $"{symbolInformation.Usages.Count} references to {symbolInformation.Name}";
                }
            }
            else
            {
                msgTitle = $"Never used yet. Can you remove {symbolInformation.Name}?";
            }

            var args = new
            {
                Uri = _uri,
                Position = position, // where to show the CodeLens popup
                Locations = locations // what should be in the pop up displayed 
            };
            Command command = new Command
            {
                Title = msgTitle,
                Name = "dafny.showReferences",
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