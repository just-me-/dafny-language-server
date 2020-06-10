using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

namespace DafnyLanguageServer.Core
{
    public interface IRenameProvider
    {
        WorkspaceEdit GetRenameChanges(string newName, Uri uri, int line, int col);
        RenameOutcome Outcome { get; }
    }
}