using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

namespace DafnyLanguageServer.Core
{
    public interface IDefinitionsProvider
    {
        LocationOrLocationLinks GetDefinitionLocation(Uri uri, int line, int col);
        DefinitionsOutcome Outcome { get; }
    }
}