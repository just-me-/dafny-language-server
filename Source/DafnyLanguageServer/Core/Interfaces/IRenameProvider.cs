using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface IRenameProvider
    {
        WorkspaceEdit GetRenameChanges(string newName, Uri uri, int line, int col);
    }
}
