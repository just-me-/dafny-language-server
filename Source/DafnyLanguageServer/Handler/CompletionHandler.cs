using DafnyLanguageServer.ContentManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SymbolTable = DafnyLanguageServer.DafnyAccess.SymbolTable;

namespace DafnyLanguageServer.Handler
{
    public class CompletionHandler : ICompletionHandler
    {
        private readonly ILanguageServer _router;
        private readonly BufferManager _bufferManager;
        private CompletionCapability _capability;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dfy"
            }
        );

        public CompletionHandler(ILanguageServer router, BufferManager bufferManager)
        {
            _router = router;
            _bufferManager = bufferManager;
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
            return await Task.Run(() =>
            {
                var symbols = _bufferManager.GetSymboltable(request.TextDocument.Uri);
                var word = FileHelper.GetCurrentWord(
                    _bufferManager.GetSourceCodeAsText(request.TextDocument.Uri),
                    (int)request.Position.Line,
                    (int)request.Position.Character
                );
                var parentClass = symbols.GetParentForWord(word);
                return (symbols is null) ?
                    new CompletionList() :
                    ConvertListToCompletionresponse(symbols.GetList(parentClass), request);
            });
        }

        public CompletionList ConvertListToCompletionresponse(List<SymbolTable.SymbolInformation> symbols, CompletionParams request)
        {
            var complitionItems = new List<CompletionItem>();
            foreach (var symbol in symbols)
            {
                CompletionItemKind kind = CompletionItemKind.Reference;
                Enum.TryParse(symbol.SymbolType.ToString(), true, out kind);

                Range range = FileHelper.CreateRange(request.Position.Line, request.Position.Character, symbol.Name.Length);
                TextEdit textEdit = new TextEdit
                {
                    NewText = symbol.Name,
                    Range = range
                };

                complitionItems.Add(
                    new CompletionItem
                    {
#if DEBUG
                        Label = $"{symbol.Name} (Type: {symbol.SymbolType}) (Parent: {symbol.ParentClass})",
#else
                        Label = $"{symbol.Name}", 
#endif
                        Kind = kind,
                        TextEdit = textEdit
                    });
            }
            return new CompletionList(complitionItems);
        }

        public void SetCapability(CompletionCapability capability)
        {
            _capability = capability;
        }

    }
}
