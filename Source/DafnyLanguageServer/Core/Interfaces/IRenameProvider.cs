using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    public interface IRenameProvider
    {
        WorkspaceEdit GetRenameChanges(string newName, Uri uri, int line, int col);
    }
}
