﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler.LspStandard
{
    public class RenameTaskHandler : LspBasicHandler<RenameCapability>, IRenameHandler
    {

        public RenameTaskHandler(ILanguageServer router, Workspace workspaceManager,
            ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public RenameRegistrationOptions GetRegistrationOptions()
        {
            return new RenameRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                PrepareProvider = true
            };
        }

        public async Task<WorkspaceEdit> Handle(RenameParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation("Handling Rename...");
            try
            {
                var manager = _workspaceManager.SymbolTableManager;
                var line = (int) request.Position.Line + 1;
                var col = (int) request.Position.Character + 1;
                var uri = request.TextDocument.Uri;
                var newName = request.NewName;

                var provider = new RenameProvider(manager);

                return await Task.Run(() => RunAndEvaulate(provider, newName, uri, line, col), cancellationToken);
            }
            catch (Exception e)
            {
                HandleError("Error while processing rename request", e); //todo lang
                return null;
            }
        }

        private WorkspaceEdit RunAndEvaulate(RenameProvider provider, string newName, Uri uri, int line, int col) {
        
            var result = provider.GetRenameChanges(newName, uri, line, col );

            if (provider.Outcome.Error)
            {
                _mss.SendWarning(provider.Outcome.Msg);
            }
            return result;
        }
    }
    
}


