using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides auto completion as defined in the LSP.
    /// There is additional information (like symbol type and parent class) added to each symbol if the DEBUG mode is used.
    /// </summary>
    public class CompletionTaskHandler : LspBasicHandler<CompletionCapability>, ICompletionHandler
    {
        public CompletionTaskHandler(ILanguageServer router, Workspace workspaceManager,
            ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation("Handling Completions"); // todo lang file #102

            try
            {
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var codeLine = _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.GetSourceLine(line - 1); //todo das eig schon logik für den provider aber ich glaub ist ja nicht schlimm.
                var provider = new CompletionProvider(_workspaceManager.SymbolTableManager);
                return await Task.Run(() => provider.FindCompletionItems(request.TextDocument.Uri, line, col, codeLine), cancellationToken);
            }
            catch (Exception e)
            {
                _log.LogError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                _mss.SendError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                return null;
            }

        }

    }
}
