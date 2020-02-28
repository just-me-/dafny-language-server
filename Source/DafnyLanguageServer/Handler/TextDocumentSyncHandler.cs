using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Services;
using OmniSharp.Extensions.Embedded.MediatR;
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
    internal class TextDocumentSyncHandler : ITextDocumentSyncHandler
    {
        private readonly ILanguageServer _router;
        private readonly BufferManager _bufferManager;
        private SynchronizationCapability _capability;
        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dfy"
            }
        );
        public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full; // Incremental is not yet supported by the buffer 

        public TextDocumentSyncHandler(ILanguageServer router, BufferManager bufferManager)
        {
            _router = router;
            _bufferManager = bufferManager;
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

        private DafnyFile UpdateBuffer(Uri uri, string text)
        {
            DafnyFile file = _bufferManager.UpdateBuffer(uri, text);
            return file;
        }

        private void Verify(DafnyFile file)
        {
            var verificationService = new VerificationService(_router, file.DafnyTranslationUnit);
            verificationService.Verify(file);
        }

        private void UpdateBufferAndVerifyFile(Uri uri, string text)
        {
            var file = UpdateBuffer(uri, text);
            Verify(file);
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            UpdateBufferAndVerifyFile(request.TextDocument.Uri, request.ContentChanges.FirstOrDefault()?.Text);
            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            // evt nicht der optimale Ort + Version noch "von unten holen"
            _router.Window.SendNotification("serverStarted", "0.01");
            UpdateBufferAndVerifyFile(request.TextDocument.Uri, request.TextDocument.Text);
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

        public void SetCapability(SynchronizationCapability capability)
        {
            _capability = capability;
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