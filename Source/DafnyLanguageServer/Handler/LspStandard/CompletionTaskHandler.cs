﻿using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
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
        public CompletionTaskHandler(ILanguageServer router, Workspace workspaceManager,
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
            ICompletionProvider provider = new CompletionProvider(_workspaceManager.SymbolTableManager);
            var desire = provider.GetSupposedDesire(codeLine, col);
            var entryPoint = provider.GetWrappingEntrypointSymbol(file, line, col);

            var completionItems = new List<CompletionItem>();
            foreach (var symbol in provider.GetSymbols(desire, entryPoint))
            {
                completionItems.Add(CreateCompletionItem(symbol, desire, line, col));
            }
            return completionItems;
        }

        private CompletionItem CreateCompletionItem(ISymbol symbol, CompletionType desire, int line, int col)
        {
            CompletionItemKind kind = Enum.TryParse(symbol.Kind.ToString(), true, out kind)
                ? kind
                : CompletionItemKind.Reference;

            var insertCode = GetCompletionCodeFragment(symbol, desire);

            return
                new CompletionItem
                {
                    Label = symbol.Name,
                    Kind = kind,
                    InsertText = insertCode,
#if DEBUG
                    Detail = $"Kind: { symbol.Kind } \n Parent: { symbol.Parent.Name }"
#endif
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
