using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
    public class CodeLensTaskHandler : LspBasicHandler<CodeLensCapability>, ICodeLensHandler
    {
        public CodeLensTaskHandler(ILanguageServer router, Workspace workspaceManager, ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public CodeLensRegistrationOptions GetRegistrationOptions()
        {
            return new CodeLensRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false
            };
        }

        public async Task<CodeLensContainer> Handle(CodeLensParams request, CancellationToken cancellationToken)
        {

            return await Task.Run(() =>
            {
                _log.LogInformation("Handling Code Lens"); // todo lang file #102
                var manager = _workspaceManager.SymbolTableManager;
                var uri = request.TextDocument.Uri;
                try
                {
                    ICodeLensProvider provider = new CodeLensProvider(manager, uri);
                    return provider.GetCodeLensContainer();
                }
                catch (Exception e)
                {
                    _log.LogError("Internal server error handling CodeLens: " + e.Message); // todo lang file #102
                    _mss.SendError("Internal server error handling CodeLens: " + e.Message); // todo lang file #102
                    return null; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
                }
            });

        }
    }
}
