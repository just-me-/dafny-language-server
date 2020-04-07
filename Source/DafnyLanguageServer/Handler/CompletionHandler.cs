using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyServer;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides auto completion as defined in the LSP.
    /// There is additional information (like symbol type and parent class) added to each symbol if the DEBUG mode is used.
    /// </summary>
    public class CompletionHandler : ICompletionHandler
    {
        private readonly ILanguageServer _router;
        private readonly WorkspaceManager _workspaceManager;
        private CompletionCapability _capability;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.dfy"
            }
        );

        public CompletionHandler(ILanguageServer router, WorkspaceManager workspaceManager)
        {
            _router = router;
            _workspaceManager = workspaceManager;
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
                var symbols = _workspaceManager.GetFileRepository(request.TextDocument.Uri).SymboleProcessor();
                var word = FileHelper.GetCurrentWord(
                    _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.Sourcecode,
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
                CompletionItemKind kind = Enum.TryParse(symbol.SymbolType.ToString(), true, out kind)
                   ? kind
                   : CompletionItemKind.Reference;

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
