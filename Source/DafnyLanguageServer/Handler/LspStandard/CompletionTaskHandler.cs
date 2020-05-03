using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DafnyLanguageServer.ProgramServices;
using DafnyLanguageServer.SymbolTable;
using DafnyServer;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;
using SymbolInformation = DafnyLanguageServer.SymbolTable.SymbolInformation;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides auto completion as defined in the LSP.
    /// There is additional information (like symbol type and parent class) added to each symbol if the DEBUG mode is used.
    /// </summary>
    public class CompletionTaskHandler : LspBasicHandler<CompletionCapability>, ICompletionHandler
    {
        private enum CompletionType
        {
            afterDot,
            afterNew,
            allInScope
        }

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
            _log.LogInformation("Completions..." + Resources.ExceptionMessages.negative_position); // todo lang file #102 
            try
            {
                var line = (int)request.Position.Line + 1;
                var col = (int)request.Position.Character + 1;
                var codeLine = _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.GetSourceLine(line - 1);
                return await Task.Run(() => FindCompletionItems(line, col, codeLine));
            }
            catch (Exception e)
            {
                _log.LogError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                new MessageSenderService(_router).SendError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                return null; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
            }
        }

        private List<CompletionItem> FindCompletionItems(int line, int col, string codeLine)
        {
            var desire = GetSupposedDesire(col, codeLine, out var extractedSymbolName);

            var manager = _workspaceManager.SymbolTableManager;
            var wrappingEntrypointSymbol = manager.GetSymbolWrapperForCurrentScope(line, col);
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
            var completionItems = new List<CompletionItem>();
            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol))
            {
                AddCompletionItem(completionItems, suggestionElement);
            }
            return completionItems;
        }

        private void AddCompletionItem(List<CompletionItem> items, ISymbol symbol)
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

            items.Add(
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
                });
        }

        private CompletionType GetSupposedDesire(int colPos, string line, out string symbolName)
        {
            // diese ganze funktion ist hässlich und buggy. 
            var characters = line.ToCharArray();
            int position = colPos - 2;
            if (position > characters.Length || position < 0)
            {
                throw new ArgumentException("Curser position ist ausserhalb der Zeilenreichweite "); // todo translation 
            }
            if (characters[position] == '.')
            {
                position--;
                var symbolString = "";
                while (position >= 0)
                {
                    if (char.IsLetter(characters[position])
                        || char.IsNumber(characters[position])
                        || characters[position] == '_'
                        || characters[position] == '-'
                    ) // hmm ned mit regex weil chars... testen ob das a-zA-Z0-9-_ gleichwertig ist... 
                    {
                        symbolString += characters[position];
                        position--;
                    }
                    else // das else iwie streichen mit != 
                    {
                        break;
                    }
                }
                char[] symbolCharArray = symbolString.ToCharArray();
                Array.Reverse(symbolCharArray);
                symbolName = new string(symbolCharArray);
                return CompletionType.afterDot;
            }
            symbolName = "";
            if (characters.Length >= 3 && position >= 3
                && characters[position] == ' '
                && characters[position - 1] == 'w'
                && characters[position - 2] == 'e'
                && characters[position - 3] == 'n')
            {
                return CompletionType.afterNew;
            }
            return CompletionType.allInScope;
        }
    }
}
