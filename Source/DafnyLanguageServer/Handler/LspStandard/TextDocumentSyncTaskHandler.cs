using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    ///  This Handler implements OmniSharps DocumentSyncHandler.
    ///  It handles LSP-calls like when a document has been opened or an open document changes.
    ///  Whenever this is the case, the intern <c>WorkspaceManager</c> gets updated.
    ///  An update of the buffer includes also a verify check for the Dafny source code in the fileRepository.
    /// </summary>
    internal class TextDocumentSyncTaskHandler : LspBasicHandler<SynchronizationCapability>, ITextDocumentSyncHandler
    {
        public TextDocumentSyncKind Change { get; } = LanguageServerConfig.SyncKind;

        public TextDocumentSyncTaskHandler(ILanguageServer router, IWorkspace workspaceManager, ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
            _method = Resources.Requests.update;
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                SyncKind = Change
            };
        }

        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
        {
            return new TextDocumentAttributes(uri, "");
        }

        /// <summary>
        /// Updates file and sends error to the client with the diagnostics service
        /// </summary>
        private void UpdateFileAndSendDiagnostics<T>(Uri uri, T textOrChangeEvent)
        {
            try
            {
                _log.LogInformation(Resources.LoggingMessages.request_update + uri);
                FileRepository fileRepository = _workspaceManager.UpdateFile(uri, textOrChangeEvent);

                _log.LogInformation(Resources.LoggingMessages.request_update_diagnostics);
                IDiagnosticsProvider provider = new DiagnosticsProvider(_router);
                provider.SendDiagnostics(fileRepository);

                _log.LogInformation(string.Format(Resources.LoggingMessages.request_success, _method));
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);

            }
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            switch (Change)
            {
                case TextDocumentSyncKind.Full:
                    UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.ContentChanges.LastOrDefault()?.Text);
                    break;
                case TextDocumentSyncKind.Incremental:
                    UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.ContentChanges);
                    break;
            }

            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.TextDocument.Text);
            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.Text); //flush all text on save
            return Unit.Task;
        }

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions
            {
                DocumentSelector = _documentSelector
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                IncludeText = true
            };
        }
    }
}
