using System;
using System.Collections.Generic;
using System.Linq;
using DafnyLanguageServer.SymbolTable;
using DafnyLanguageServer.Tools;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.Core
{
    /// <summary>
    /// This service is used by the related handler and contains the core logic.
    /// </summary>
    public class RenameProvider : IRenameProvider
    {
        public RenameOutcome Outcome { get; } = new RenameOutcome();

        private readonly ISymbolTableManager _manager;
        private static readonly HashSet<string> _reservedWords = new ReservedWordsProvider().GetReservedWords();


        public RenameProvider(ISymbolTableManager manager)
        {
            _manager = manager;
        }
        public WorkspaceEdit GetRenameChanges(string newName, Uri uri, int line, int col)
        {
            var symbolAtCursor = _manager.GetSymbolByPosition(uri, line, col);

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

            foreach (var symbol in SymbolUtil.GetAllOccurrences(symbolAtCursor))
            {
                var textEdit = new TextEdit
                {
                    NewText = newName,
                    Range = new Range
                    {
                        Start = new Position(symbol.Line - 1, symbol.Column - 1),
                        End = new Position(symbol.Line - 1, symbol.IdentifierEndColumn - 1)
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

            if (char.IsDigit(newName.First()))
            {
                SetError(Resources.LoggingMessages.rename_start_with_number);
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

        private List<TextEdit> GetOrCreate(IDictionary<Uri, List<TextEdit>> Changes, ISymbolInformation symbol)
        {
            if (Changes.TryGetValue(symbol.FileUri, out var textEditsPerFile))
            {
                return textEditsPerFile;
            }
            textEditsPerFile = new List<TextEdit>();
            Changes.Add(symbol.FileUri, textEditsPerFile);

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
