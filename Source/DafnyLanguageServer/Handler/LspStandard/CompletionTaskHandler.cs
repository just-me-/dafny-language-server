using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.HandlerServices;
using DafnyLanguageServer.ProgramServices;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides auto completion as defined in the LSP.
    /// There is additional information (like symbol type and parent class) added to each symbol if the DEBUG mode is used.
    /// </summary>
    public class CompletionTaskHandler : LspBasicHandler<CompletionCapability>, ICompletionHandler
    {
        public CompletionTaskHandler(ILanguageServer router, WorkspaceManager workspaceManager,
            ILoggerFactory loggingFactory)
            : base(router, workspaceManager, loggingFactory)
        {
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
            _log.LogInformation("Completions..."); // todo lang file #102
            try
            {
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var codeLine = _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.GetSourceLine(line - 1);
                return await Task.Run(() => FindCompletionItems(request.TextDocument.Uri, line, col, codeLine));
            }
            catch (Exception e)
            {
                _log.LogError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                new MessageSenderService(_router).SendError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                return null;//todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
            }
        }

        private List<CompletionItem> FindCompletionItems(Uri file, int line, int col, string codeLine)
        {
            var service = new CompletionService(_workspaceManager.SymbolTableManager);
            var desire = service.GetSupposedDesire(codeLine, col);
            var entryPoint = service.GetWrappingEntrypointSymbol(file, line, col);

            var completionItems = new List<CompletionItem>();
            foreach (var symbol in service.GetSymbols(desire, entryPoint))
            {
                if (!symbol.Name.StartsWith("_"))
                {
                    completionItems.Add(CreateCompletionItem(symbol, desire, line, col));
                }
            }
            return completionItems;
        }

        private CompletionItem CreateCompletionItem(ISymbol symbol, CompletionType desire, int line, int col)
        {
            CompletionItemKind kind = Enum.TryParse(symbol.Kind.ToString(), true, out kind)
                ? kind
                : CompletionItemKind.Reference;

            //Range range = new Range
            //{
            //    Start = new Position(line-1, col-3),
            //    End = new Position(line-1, col-1)
            //};
            //TextEdit textEdit = new TextEdit
            //{
            //    NewText = symbol.Name,
            //    Range = range
            //};

            var insertCode = GetCompletionCodeFragment(symbol, desire);

            return
                new CompletionItem
                {
#if DEBUG
                    Label =
                        $"{symbol.Name} (Kind: {symbol.Kind}) (Parent: {symbol.Parent.Name})", // todo lang file #102
#else
                        Label = $"{symbol.Name}",
#endif
                    Kind = kind,
                    InsertText = insertCode
                };
        }

        private string GetCompletionCodeFragment(ISymbol symbol, CompletionType desire)
        {
            // todo evt in completion service auslagern damit man das dann auch noch testen kann, ist ja logik.. 
            switch (desire)
            {
                case CompletionType.AfterNew:
                    return symbol.Name + "()";
                case CompletionType.AfterDot:
                    return symbol.Kind == Kind.Method || symbol.Kind == Kind.Function ? symbol.Name + "()" : symbol.Name;
                default:
                    return symbol.Name;
            }
        }
    }
}
