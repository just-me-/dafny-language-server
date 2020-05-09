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
                return await Task.Run(() => FindCompletionItems(request.TextDocument.Uri, line, col, codeLine));  //todo merge conflict: auf dem adneren branch heisyxst es "GetCompletion., msus es get heissen?"
            }
            catch (Exception e)
            {
                _log.LogError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                new MessageSenderService(_router).SendError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                return null; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
            }
        }

        private List<CompletionItem> FindCompletionItems(Uri file, int line, int col, string codeLine) //todo merge conflict: auf dem adneren branch heisyxst es "GetCompletion., msus es get heissen?
        {
            var service = new CompletionService(_workspaceManager.SymbolTableManager);
            var desire = service.GetSupposedDesire(codeLine, col);
            var entryPoint = service.GetWrappingEntrypointSymbol(line, col);

            //todo merge von hier bis, siehe unten, ka, auf dem master alles weg? muiss das weg?? ich denk es muss eher weg ich hab hier eig eh nix gemacht??
            var manager = _workspaceManager.SymbolTableManager;
            var wrappingEntrypointSymbol = manager.GetSymbolWrapperForCurrentScope(file, line, col);
            switch (desire)
            {
                case CompletionType.afterDot: // so its a class.... not a dmodule(?) - for v1...
                                              // modul1.modul2.class ist auch möglich
                                              // und
                                              // object.variable gibts auch... not supported yet
                    var selectedSymbol = manager.GetClosestSymbolByName(wrappingEntrypointSymbol, extractedSymbolName);
                    return GetSymbolsProperties(manager, selectedSymbol);
                case CompletionType.afterNew:
                    return GetClassSymbolsInScope(manager, wrappingEntrypointSymbol);
                case CompletionType.allInScope:
                    return GetAllSymbolsInScope(manager, wrappingEntrypointSymbol);
                default:
                    throw new ArgumentException("Users desire is not supported yet.");
            }
        }

        private List<CompletionItem> GetSymbolsProperties(IManager manager, ISymbol selectedSymbol)
        {
            var completionItems = new List<CompletionItem>();
            // if selectedSymbol is null... error iwas... not found mässig... todo
            var classSymbol = manager.GetClassOriginFromSymbol(selectedSymbol);
            foreach (var suggestionElement in classSymbol.Children)
            {
                // strip constructor 2do
                /*
                var ignoredSymbols = new[] { "_ctor", "_default" };
                list?.RemoveAll(x => ignoredSymbols.Any(x.Name.Contains));
                return list;
                */
                AddCompletionItem(completionItems, suggestionElement);
            }
            return completionItems;
        }

        private List<CompletionItem> GetClassSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol, new Predicate<ISymbol>(x => x.Kind == Kind.Class)))
            {
                AddCompletionItem(completionItems, suggestionElement);
            }
            return completionItems;
        }

        private List<CompletionItem> GetAllSymbolsInScope(IManager manager, ISymbol wrappingEntrypointSymbol)
        {

          //todo merge bis here
            var completionItems = new List<CompletionItem>();
            foreach (var symbol in service.GetSymbols(desire, entryPoint))
            {
                completionItems.Add(CreateCompletionItem(symbol));
            }
            return completionItems;
        }

        private CompletionItem CreateCompletionItem(ISymbol symbol)
        {
            CompletionItemKind kind = Enum.TryParse(symbol.Kind.ToString(), true, out kind)
                ? kind
                : CompletionItemKind.Reference;

            // is this range rly neeeded?
            ///Range range = FileHelper.CreateRange(request.Position.Line, request.Position.Character, symbol.Name.Length);
            TextEdit textEdit = new TextEdit
            {
                NewText = symbol.Name,
                //Range = range
            };

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
                    TextEdit = textEdit
                };
        }
    }
}
