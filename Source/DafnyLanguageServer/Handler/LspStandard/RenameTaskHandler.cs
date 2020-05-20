using System;
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

        private readonly MessageSenderService _mss;
        public RenameTaskHandler(ILanguageServer router, Workspace workspaceManager,
            ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
            _mss = new MessageSenderService(_router);

        }

        public RenameRegistrationOptions GetRegistrationOptions()
        {
            return new RenameRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                PrepareProvider = true
            };
        }

        public Task<WorkspaceEdit> Handle(RenameParams request, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    var manager = _workspaceManager.SymbolTableManager;
                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;
                    var uri = request.TextDocument.Uri;

                    var provider = new RenameProvider(manager);
                    var result = provider.GetRenameChanges(request.NewName, uri, line, col);

                    if (provider.Outcome.Error)
                    {
                        _mss.SendWarning(provider.Outcome.Msg);
                    }

                    return result;
                }
                catch (Exception e)
                {
                    _log.LogError("Error Handling Rename Execution: " + e.Message);
                    _mss.SendError("Error Handling Rename Request.");
                    return new WorkspaceEdit();

                }
            });
        }
    }
}


