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
using DafnyLanguageServer.Core;
using DafnyLanguageServer.Tools;
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
        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Incremental;

        public TextDocumentSyncTaskHandler(ILanguageServer router, Workspace workspaceManager, ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
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
                _log.LogInformation("Updating File " + uri); // todo lang file #102
                FileRepository fileRepository = _workspaceManager.UpdateFile(uri, textOrChangeEvent);
                _log.LogInformation("Calculating Diagnostics"); // todo lang file #102
                IDiagnosticsProvider provider = new DiagnosticsProvider(_router);
                provider.SendDiagnostics(fileRepository);
                _log.LogInformation("Update Request successfully handled."); // todo lang file #102
            }
            catch (Exception e)
            {
                var msg = $"Internal server error handling Document Update: {e.Message}"; // todo lang file #102
                _log.LogError(msg);
                _mss.SendError(msg);
            }
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            if (Change == TextDocumentSyncKind.Full)
            {
                UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.ContentChanges.LastOrDefault()?.Text);
            }
            else if (Change == TextDocumentSyncKind.Incremental)
            {
                UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.ContentChanges);
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
            return new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                IncludeText = true
            };
        }
    }
}
