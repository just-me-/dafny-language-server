using DafnyLanguageServer.FileManager;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DafnyLanguageServer.ProgramServices;
using DafnyServer;
using Microsoft.Dafny;
using Microsoft.Extensions.Logging;

namespace DafnyLanguageServer.Handler
{
    /// <summary>
    /// This handler provides auto completion as defined in the LSP.
    /// There is additional information (like symbol type and parent class) added to each symbol if the DEBUG mode is used.
    /// </summary>
    public class CompletionHandler : LspBasicHandler<CompletionCapability>, ICompletionHandler
    {
        private enum CompletionType
        {
            afterDot,
            afterNew,
            allInScope
        }

        public CompletionHandler(ILanguageServer router, WorkspaceManager workspaceManager, ILoggerFactory loggingFactory)
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
                return await Task.Run(() =>
                {
                    /* old 
                    var symbols = _workspaceManager.GetFileRepository(request.TextDocument.Uri).SymboleProcessor();
                    var word = FileHelper.GetCurrentWord(
                        _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.Sourcecode,
                        (int)request.Position.Line,
                        (int)request.Position.Character
                    );
                    var parentClass = symbols.GetParentForWord(word);

                    return (symbols is null)
                        ? new CompletionList()
                        : ConvertListToCompletionresponse(symbols.GetList(parentClass), request);
                        */

                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;

                    var codeLine = _workspaceManager.GetFileRepository(request.TextDocument.Uri).PhysicalFile.GetSourceLine(line - 1);
                    string extractedSymbolName;
                    var desire = GetSupposedDesire(col, codeLine, out extractedSymbolName);

                    var manager = _workspaceManager.SymbolTableManager;
                    var wrappingEntrypointSymbol = manager.GetSymbolWrapperForCurrentScope(line, col);
                    var complitionItems = new List<CompletionItem>();
                    switch (desire)
                    {
                        case CompletionType.afterDot: // so its a class.... not a dmodule(?) - for v1... 
                            // modul1.modul2.class ist auch möglich
                            // und
                            // object.variable gibts auch... not supported yet 

                            var selectedSymbol = manager.GetClosestSymbolByName(wrappingEntrypointSymbol, extractedSymbolName);
                            var classSymbol = manager.GetClassOriginFromSymbol(selectedSymbol);
                            foreach (var suggestionElement in classSymbol.Children)
                            {
                                // strip constructor 2do 
                                /*
                                var ignoredSymbols = new[] { "_ctor", "_default" };
                                list?.RemoveAll(x => ignoredSymbols.Any(x.Name.Contains));
                                return list;
                                */
                                AddCompletionItem(complitionItems, suggestionElement);
                            }
                            break;
                        case CompletionType.afterNew:
                            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol))
                            {
                                // strip all "non class declarations" 
                                AddCompletionItem(complitionItems, suggestionElement);
                            }
                            break;
                        case CompletionType.allInScope:
                            foreach (var suggestionElement in manager.GetAllDeclarationForSymbolInScope(wrappingEntrypointSymbol))
                            {
                                AddCompletionItem(complitionItems, suggestionElement);
                            }
                            break;
                    }

                    return complitionItems;



                });
            }
            catch (Exception e)
            {
                _log.LogError("Internal server error handling Completions: " + e.Message); // todo lang file #102
                new MessageSenderService(_router).SendError("Internal server error handling Completions: " + e.Message); // todo lang file #102

                return null; //todo warum return null... ght dat ned eleganter? sendError oder so via new throw ? #107
            }
        }

        private void AddCompletionItem(List<CompletionItem> items, SymbolTable.SymbolInformation symbol)
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
                    Label = $"{symbol.Name} (Kind: {symbol.Kind}) (Parent: {symbol.Parent.Name})", // todo lang file #102
#else
                        Label = $"{symbol.Name}",
#endif
                    Kind = kind,
                    TextEdit = textEdit
                });
        }

        private CompletionType GetSupposedDesire(int colPos, string line, out string symbolName)
        {
            // return parameter und dann gleich das "symbol" auslesen falls es ein "." ist? wär gail 
            symbolName = "teest";

            // zurückloopen 

            // if forher ".", dann symbol pos -2
            // verfeinerungen... vorher ein "new"? 
            // von deer position zurück loopen.."geetmeaningofcompltion.. punkt?"

            return CompletionType.afterDot;
        }

        private CompletionList ConvertListToCompletionresponse(List<DafnyServer.OldSymbolTable.OldSymbolInformation> symbols, CompletionParams request)
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
                        Label = $"{symbol.Name} (Type: {symbol.SymbolType}) (Parent: {symbol.ParentClass})", // todo lang file #102
#else
                        Label = $"{symbol.Name}",
#endif
                        Kind = kind,
                        TextEdit = textEdit
                    });
            }
            return new CompletionList(complitionItems);
        }
    }
}
