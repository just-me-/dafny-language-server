using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
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
        private static readonly Container<string> TriggerChars = new Container<string>(".", "new ");

        public CompletionTaskHandler(ILanguageServer router, IWorkspace workspaceManager,
            ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
            _method = Resources.Requests.completion;
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                ResolveProvider = false,
                TriggerCharacters = TriggerChars
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
        {
            _log.LogInformation(string.Format(Resources.LoggingMessages.request_handle, _method));

            try
            {
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var filerepo = _workspaceManager.GetFileRepository(request.TextDocument.Uri);
                var codeLine = filerepo.PhysicalFile.GetSourceLine(line - 1);
                var symbolmanager = filerepo.SymbolTableManager;

                ICompletionProvider provider = new CompletionProvider(symbolmanager);
                return await Task.Run(() => provider.FindCompletionItems(request.TextDocument.Uri, line, col, codeLine),
                    cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                _mss.SendInformation(e.Message);
                return null;
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);
                return null;
            }
        }
    }
}
