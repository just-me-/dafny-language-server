using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.ProgramServices;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler.LspStandard
{
    public class RenameTaskHandler : LspBasicHandler<RenameCapability>, IRenameHandler
    {
        public RenameTaskHandler(ILanguageServer router, WorkspaceManager workspaceManager,
            ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
        }

        public RenameRegistrationOptions GetRegistrationOptions()
        {
            return new RenameRegistrationOptions
            {
                DocumentSelector = _documentSelector,
                PrepareProvider = true
            };
        }

        public Task<WorkspaceEdit> Handle(RenameParams request, CancellationToken cancellationToken)
        {
            try
            {
                return Task.Run(() =>
                {
                    var file = _workspaceManager.GetFileRepository(request.TextDocument.Uri);
                    var stMan = _workspaceManager.SymbolTableManager;
                    var nav = new SymbolTableNavigator(); // navigator should not be used here.. use manager.. todo
                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;

                    var symbolAtCursor = stMan.GetSymbolByPosition(line, col);

                    if (symbolAtCursor == null)
                    {
                        return null;
                    }

                    if (reservedWords.Contains(request.NewName))
                    {
                        return null;
                        //todo #341 message sender hier iwie möglich?
                    }

                    IEnumerable<ISymbol> symbolsToRename = nav.GetAllOccurrences(symbolAtCursor);

                    Dictionary<Uri, List<TextEdit>> changes = new Dictionary<Uri, List<TextEdit>>();


                    foreach (var symbol in symbolsToRename)
                    {
                        var textEdit = new TextEdit
                        {
                            NewText = request.NewName,
                            Range = new Range()
                            {
                                Start = new Position(symbol.Line - 1 ?? 0, symbol.Column - 1 ?? 0),
                                End = new Position(symbol.Line - 1 ?? 0, symbol.ColumnEnd - 1 ?? 0)
                            }
                        };
                        var editsForAffectedFile = GetOrCreate(changes, symbol);
                        editsForAffectedFile.Add(textEdit);
                    }

                    var changesAsEnumerable = ConvertDict(changes);

                    var result = new WorkspaceEdit
                    {
                        Changes = changesAsEnumerable
                    };
                    return result;
                });
            }
            catch (Exception e)
            {
                _log.LogError("Error Handling Rename Execution: " + e.Message);
                new MessageSenderService(_router).SendError("Error Handling Rename Request.");
                return Task.FromResult(new WorkspaceEdit());
            }
        }

        private Dictionary<Uri, IEnumerable<TextEdit>> ConvertDict(Dictionary<Uri, List<TextEdit>> input)
        {
            Dictionary<Uri, IEnumerable<TextEdit>> output = new Dictionary<Uri, IEnumerable<TextEdit>>();
            foreach (var kvp in input)
            {
                output.Add(kvp.Key, kvp.Value);
            }
            return output;
        }

        //private Dictionary<Uri, IEnumerable<TextEdit> ConvertDict(Dictionary<Uri, IList<TextEdit>> changes)
        //{
        //    throw new NotImplementedException();
        //}

        private List<TextEdit> GetOrCreate(Dictionary<Uri, List<TextEdit>> Changes, ISymbol symbol)
        {
            if (!Changes.TryGetValue(symbol.File, out var textEditsPerFile))
            {
                textEditsPerFile = new List<TextEdit>();
                Changes.Add(symbol.File, textEditsPerFile);
            }
            return textEditsPerFile;
        }

        // todo das in ein json oder so für konfigurierbarkeit 
        private static readonly HashSet<string> reservedWords = new HashSet<string> //Hashset for turbospeed.
        {
            "abstract", "array", "as", "assert", "assume", "bool", "break",
            "calc", "case", "char", "class", "codatatype", "colemma",
            "constructor", "copredicate", "datatype", "decreases",
            "default", "else", "ensures", "exists", "extends", "false",
            "forall", "free", "fresh", "function", "ghost", "if", "imap", "import",
            "in", "include", "inductive", "int", "invariant", "iset", "iterator", "label",
            "lemma", "map", "match", "method", "modifies", "modify",
            "module", "multiset", "nat", "new", "newtype", "null", "object",
            "old", "opened", "predicate", "print", "protected",
            "reads", "real", "refines", "requires", "return", "returns", "seq",
            "set", "static", "string", "then", "this", "trait", "true", "type",
            "var", "where", "while", "yield", "yields"
        };
    }
}