using DafnyLanguageServer.FileManager;
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
using DafnyLanguageServer.HandlerServices;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    ///  This Handler implements OmniSharps DocumentSyncHandler.
    ///  It handles LSP-calls like when a document has been opened or an open document changes.
    ///  Whenever this is the case, the intern <c>WorkspaceManager</c> gets updated.
    ///  An update of the buffer includes also a verify check for the Dafny source code in the fileRepository. 
    /// </summary>
    internal class TextDocumentSyncHandler : LspBasicHandler, ITextDocumentSyncHandler
    {
        private SynchronizationCapability _capability; //needed by omnisharp

        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full; // Incremental is not yet supported by the buffer 

        public TextDocumentSyncHandler(ILanguageServer router, WorkspaceManager workspaceManager)
            : base(router, workspaceManager)
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
        public void SetCapability(SynchronizationCapability capability)
        {
            _capability = capability;
        }

        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
        {
            return new TextDocumentAttributes(uri, "");
        }

        /// <summary>
        /// Updates file and sends error to the client with the diagnostics service
        /// </summary>
        private void UpdateFileAndSendDiagnostics(Uri uri, string text)
        {
            FileRepository fileRepository = _workspaceManager.UpdateFile(uri, text);
            new DiagnosticsService(_router).SendDiagnostics(fileRepository);
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            UpdateFileAndSendDiagnostics(request.TextDocument.Uri, request.ContentChanges.FirstOrDefault()?.Text);
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