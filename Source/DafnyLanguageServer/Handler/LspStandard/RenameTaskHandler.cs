using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.FileManager;
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
                    var nav = new SymbolTableNavigator();
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

                    IEnumerable<ISymbol> symbolsToRename = nav.GetAllOccurences(symbolAtCursor);
                    List<TextEdit> editsForOneFile = new List<TextEdit>(); //todo multifile, import, blabla

                    foreach (var symbol in symbolsToRename)
                    {
                        var textEdit = new TextEdit
                        {
                            NewText = request.NewName,
                            Range = new Range()
                            {
                                Start = new Position(symbol.Line-1 ?? 0, symbol.Column-1 ?? 0),
                                End = new Position(symbol.Line-1 ?? 0, symbol.ColumnEnd-1 ?? 0)
                            }
                        };
                        editsForOneFile.Add(textEdit);
                    }

                    var result = new WorkspaceEdit
                    {
                        Changes = new Dictionary<Uri, IEnumerable<TextEdit>>
                        {
                            {request.TextDocument.Uri, editsForOneFile}
                        }
                    };
                    return result;
                });
            }
            catch (Exception e)
            {
                _log.LogError("Error Handling Rename Execution: " + e.Message);
                return null;
            }
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