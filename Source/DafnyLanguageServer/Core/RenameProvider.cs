using System;
using System.Collections.Generic;
using System.Linq;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{

    public class RenameProvider : IRenameProvider
    {
        public RenameOutcome Outcome { get;  set; } = new RenameOutcome();

        private readonly ISymbolTable _symbolTable;
        private static readonly HashSet<string> _reservedWords = new ReservedWordsProvider().GetReservedWords();


        public RenameProvider(ISymbolTable symbolTable)
        {
            _symbolTable = symbolTable;
        }
        public WorkspaceEdit GetRenameChanges(string newName, Uri uri, int line, int col)
        {
            var symbolAtCursor = _symbolTable.GetSymbolByPosition(uri, line, col);

            if (symbolAtCursor == null)
            {
                SetError(Resources.LoggingMessages.rename_no_symbol + $" L{line}:C{col}");
                return null;
            }

            VerifyNewName(newName);

            if (Outcome.Error)
            {
                return null;
            }
            
            Dictionary<Uri, List<TextEdit>> changes = new Dictionary<Uri, List<TextEdit>>();

            foreach (var symbol in symbolAtCursor.GetAllOccurrences())
            {
                var textEdit = new TextEdit
                {
                    NewText = newName,
                    Range = new Range
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
        }

        private void VerifyNewName(string newName)
        {
            if (_reservedWords.Contains(newName))
            {
                SetError(newName + Resources.LoggingMessages.rename_reserved_word);
            }

            if (!newName.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                SetError(newName + Resources.LoggingMessages.rename_only_digits);

            }

            if (newName.StartsWith("_"))
            {
                SetError(Resources.LoggingMessages.rename_start_with_underscore);
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

        private List<TextEdit> GetOrCreate(Dictionary<Uri, List<TextEdit>> Changes, ISymbol symbol)
        {
            if (Changes.TryGetValue(symbol.File, out var textEditsPerFile))
            {
                return textEditsPerFile;
            }
            textEditsPerFile = new List<TextEdit>();
            Changes.Add(symbol.File, textEditsPerFile);

            return textEditsPerFile;
        }


        private void SetError(string msg)
        {
            Outcome.Error = true;
            Outcome.Msg = msg;
        }

    }

    public class RenameOutcome
    {
        public bool Error { get; set; }
        public string Msg { get; set; }
    }
}
