using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This class provides CodeLens information for VS Code.
    /// This class is work in progress. The content of this huge class should be outsourced into a <c>CodeLensServiceProvider</c>.
    /// </summary>
    public class HoverTaskHandler : LspBasicHandler<HoverCapability>, IHoverHandler
    {
        public HoverTaskHandler(ILanguageServer router, Workspace workspaceManager,
            ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
        }


        public Task<Hover> Handle(HoverParams request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try { 
                var manager = _workspaceManager.SymbolTableManager;
                var uri = request.TextDocument.Uri;
                var line = (int) request.Position.Line + 1;
                var col = (int) request.Position.Character + 1;

                var provider = new HoverProvider(manager);
                return provider.GetHoverInformation(uri, line, col);
                }
                catch (Exception e)
                {
                    _log.LogError("Error Handling Rename Execution: " + e.Message);
                    new MessageSenderService(_router).SendError("Error Handling Rename Request.");
                    return null;
                }
            });
        }


        public TextDocumentRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }
    }
}
