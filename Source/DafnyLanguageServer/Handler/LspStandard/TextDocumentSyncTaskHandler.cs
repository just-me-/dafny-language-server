using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.WorkspaceManager;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            switch (Change)
            {
                case TextDocumentSyncKind.Full:
                    ProcessUpdateRequest(request.TextDocument.Uri, request.ContentChanges.LastOrDefault()?.Text);
                    break;
                case TextDocumentSyncKind.Incremental:
                    ProcessUpdateRequest(request.TextDocument.Uri, request.ContentChanges);
                    break;
            }

            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            ProcessUpdateRequest(request.TextDocument.Uri, request.TextDocument.Text);
            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            ProcessUpdateRequest(request.TextDocument.Uri, request.Text); //flush all text on save
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

        /// <summary>
        /// Updates file and sends diagnostics and error count to the client with the diagnostics service
        /// </summary>
        private void ProcessUpdateRequest<T>(Uri uri, T textOrChangeEvent)
        {
            try
            {
                var fileRepository = UpdateFile(uri, textOrChangeEvent);
                PublishDiagnosticsParams diags = GetDiagnostics(uri, fileRepository);
                PublishResults(diags);

                _log.LogInformation(string.Format(Resources.LoggingMessages.request_success, _method));
            }
            catch (Exception e)
            {
                HandleError(string.Format(Resources.LoggingMessages.request_error, _method), e);
            }
        }

        /// <summary>
        /// Delegates the update to the file repository.
        /// </summary>
        /// <returns>The updated file repository</returns>
        private IFileRepository UpdateFile<T>(Uri uri, T textOrChangeEvent)
        {
            _log.LogInformation(Resources.LoggingMessages.request_update + uri);
            IFileRepository fileRepository = _workspaceManager.UpdateFile(uri, textOrChangeEvent);
            return fileRepository;
        }

        /// <summary>
        /// Invokes the DiagnosticProvider to assemble diagnsotics
        /// </summary>
        /// <returns>Diagnostics ready to publish</returns>
        private PublishDiagnosticsParams GetDiagnostics(Uri uri, IFileRepository fileRepository)
        {
            _log.LogInformation(Resources.LoggingMessages.request_update_diagnostics);
            _mss.SendCurrentDocumentInProcess(fileRepository.PhysicalFile.Filepath);
            IFileRepository repo = _workspaceManager.GetFileRepository(uri);
            IDiagnosticsProvider provider = new DiagnosticsProvider(repo);
            var diags = provider.GetPublishableDiagnostics();
            return diags;
        }

        /// <summary>
        /// Publishes the Diagnostics using the <c>_router</c>
        /// </summary>
        /// <param name="diags">The Diagnostics to publish.</param>
        private void PublishResults(PublishDiagnosticsParams diags)
        {
            _router.Document.PublishDiagnostics(diags);
            _mss.SendCountedErrors(diags.Diagnostics.Count());
        }
    }
}