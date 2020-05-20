using System;
using System.Collections.Generic;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
// ReSharper disable PossibleInvalidOperationException
// Nullchecks are done elsewhere.

namespace DafnyLanguageServer.Core
{

    public class DefinitionsProvider : IDefinitionsProvider
    {
        private readonly IManager _symbolTable;
        public DefinitionsOutcome Outcome { get; set; }

        public DefinitionsProvider(IManager symbolTable)
        {
            _symbolTable = symbolTable;
        }

        public LocationOrLocationLinks GetDefinitionLocation(Uri uri, int line, int col)
        {
            List<LocationOrLocationLink> links = new List<LocationOrLocationLink>();
            var symbol = _symbolTable.GetSymbolByPosition(uri, line, col);

            if (symbol == null)
            {
                Outcome = DefinitionsOutcome.NotFound;
                return new LocationOrLocationLinks();
            }
            if (symbol.IsDeclaration)
            {
                Outcome = DefinitionsOutcome.WasAlreadyDefintion;
            }

            var originSymbol = symbol.DeclarationOrigin;

            Position position = new Position((long)originSymbol.Line - 1, (long)originSymbol.ColumnStart - 1);
            Range range = new Range { Start = position, End = position };
            var location = new Location { Uri = originSymbol.File, Range = range };

            links.Add(new LocationOrLocationLink(location));

            Outcome = DefinitionsOutcome.Success;
            return new LocationOrLocationLinks(links);
        }
    }

    public enum DefinitionsOutcome { Success, WasAlreadyDefintion, NotFound}
}