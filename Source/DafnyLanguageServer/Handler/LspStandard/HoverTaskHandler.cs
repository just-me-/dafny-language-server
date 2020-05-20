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


        public async Task<Hover> Handle(HoverParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation("Handling Hover...");
            try
            {
                var manager = _workspaceManager.SymbolTableManager;
                var uri = request.TextDocument.Uri;
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var provider = new HoverProvider(manager);
                return await Task.Run(() => provider.GetHoverInformation(uri, line, col), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError("Error while processing Hover request", e); //todo lang

                return null;
            }
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
