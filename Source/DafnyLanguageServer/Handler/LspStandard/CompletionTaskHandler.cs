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

                /*
                var list = new List<CompletionItem>();
                list.Add(new CompletionItem
                {
                    Label = "hello" + e.Message
                });
                return list; ; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
                */
                return null;
            }
        }

        private List<CompletionItem> FindCompletionItems(Uri file, int line, int col, string codeLine)
        {
            /*
            var list = new List<CompletionItem>();
            list.Add(new CompletionItem
            {
                Label = "hello2"
            });
            return list;
            */
            var service = new CompletionService(_workspaceManager.SymbolTableManager);
            var desire = service.GetSupposedDesire(codeLine, col);
            var entryPoint = service.GetWrappingEntrypointSymbol(file, line, col);

            var completionItems = new List<CompletionItem>();
            foreach (var symbol in service.GetSymbols(desire, entryPoint))
            {
                if (!symbol.Name.StartsWith("_"))
                {
                    completionItems.Add(CreateCompletionItem(symbol, line, col));
                }
            }
            return completionItems;
        }

        private CompletionItem CreateCompletionItem(ISymbol symbol, int line, int col)
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


            //Es gibt auch di eMöglichkeit, statt dem Textedit einrfach das "Inserttext" zu nehmen - da hast du nur text. Siehe unten.

            //Ein TextEdit enthält neben dem neuen Text auch noch einen Range. Das TextEdit löscht dann alles in dem Range, udn ersetzt es durch den neuen Text.
            //das ist z.b. beim rename der fall: alter text löschen, neuen einfügen.

            //Damit hätte mand ie möglichkeit, wenn der User z.B. schon () geschrieben hat, diese klammern wie zu überschreiben durch die autocompletion.
            //nicht sicher, ob wir das brauchen.

            //der range muss die cursor postion enthalten, muss gültig sein, also nicht über die zeilenlänge rausgehen etc. scheint merh insgesamt dann doch etwas heikel.
            //habe es mal auf insertText geändret.


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
                    InsertText = symbol.Name
                };
        }
    }
}
