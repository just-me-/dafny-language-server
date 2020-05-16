using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
using DafnyLanguageServer.WorkspaceManager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace DafnyLanguageServer.Handler.LspStandard
{
    public class RenameTaskHandler : LspBasicHandler<RenameCapability>, IRenameHandler
    {
        private static HashSet<string> _reservedWords = new ReservedWordsProvider().GetReservedWords();

        private readonly MessageSenderService _mss;
        public RenameTaskHandler(ILanguageServer router, Workspace workspaceManager,
            ILoggerFactory loggingFactory = null)
            : base(router, workspaceManager, loggingFactory)
        {
            _mss = new MessageSenderService(_router);

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
                    var manager = _workspaceManager.SymbolTableManager;
                    var line = (int)request.Position.Line + 1;
                    var col = (int)request.Position.Character + 1;
                    var symbolAtCursor = manager.GetSymbolByPosition(request.TextDocument.Uri, line, col);

                    if (RenameIsInvalid(request, symbolAtCursor, line, col))
                    {
                        return null;
                    }

                    IEnumerable<ISymbol> symbolsToRename = symbolAtCursor.GetAllOccurrences();

                    Dictionary<Uri, List<TextEdit>> changes = new Dictionary<Uri, List<TextEdit>>();

                    foreach (var symbol in symbolAtCursor.GetAllOccurrences())
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
                _mss.SendError("Error Handling Rename Request.");
                return Task.FromResult(new WorkspaceEdit());
            }
        }

        private bool RenameIsInvalid(RenameParams request, ISymbol symbolAtCursor, int line, int col)
        {
            if (symbolAtCursor == null)
            {
                _mss.SendInformation($"There is no renameable symbol at L{line}:C{col}");
                return true;
            }

            if (_reservedWords.Contains(request.NewName))
            {
                _mss.SendInformation($"{request.NewName} is a reserved word.");
                return true;
            }

            if (request.NewName.StartsWith("_"))
            {
                _mss.SendInformation($"Identifier names must not start with an underscore.");
                return true;
            }

            return false;
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

        private List<TextEdit> GetOrCreate(Dictionary<Uri, List<TextEdit>> Changes, ISymbol symbol)
        {
            if (!Changes.TryGetValue(symbol.File, out var textEditsPerFile))
            {
                textEditsPerFile = new List<TextEdit>();
                Changes.Add(symbol.File, textEditsPerFile);
            }

            return textEditsPerFile;
        }


    }
}

