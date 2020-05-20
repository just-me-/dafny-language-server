using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Boogie;
using Microsoft.Dafny;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the <c>TextDocumentSyncTaskHandler</c> to provide verification for Dafny files with <c>SendDiagnostics</c>. 
    /// </summary>
    public class HoverProvider : IHoverProvider
    {
        
    }
}
