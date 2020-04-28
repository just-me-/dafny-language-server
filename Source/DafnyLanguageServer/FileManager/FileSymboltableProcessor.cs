using DafnyLanguageServer.DafnyAccess;
using System.Collections.Generic;
using System.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DafnyServer;
using Microsoft.Boogie;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This <c>FileSymboltableProcessor</c> provides all symbols that were found in a (valid) Dafny file.
    /// This newOldSymbol list can be used for features like <c>AutoCompletion</c>.
    ///
    /// Gets replaced... 2do
    /// </summary>
    public class FileSymboltableProcessor
    {
        private readonly List<DafnyServer.OldSymbolTable.OldSymbolInformation> _symbolTable;
        public bool HasEntries => (_symbolTable.Count > 0); //del

        public FileSymboltableProcessor(List<DafnyServer.OldSymbolTable.OldSymbolInformation> symbolTable)
        {
            _symbolTable = symbolTable;
        }

        public List<DafnyServer.OldSymbolTable.OldSymbolInformation> GetFullList()
        {
            return RemoveConstructorSymbols(_symbolTable);
        }

        public List<DafnyServer.OldSymbolTable.OldSymbolInformation> GetList()
        {
            return RemoveDuplicates(_symbolTable);
        }

        public List<DafnyServer.OldSymbolTable.OldSymbolInformation> GetList(string identifier)
        {
            if (identifier is null)
            {
                return GetList();
            }
            var parentSymbol = GetSymbolByName(identifier);
            return RemoveDuplicates(_symbolTable.Where(x => (x.ParentClass == identifier && SymbolIsInRangeOf(x, parentSymbol))).ToList());
        }

        private DafnyServer.OldSymbolTable.OldSymbolInformation GetSymbolByName(string name)
        {
            return _symbolTable.FirstOrDefault(x => (x.Name == name));
        }

        // is this needed in new symbol table? 
        private Range SymbolInformationToRange(DafnyServer.OldSymbolTable.OldSymbolInformation newOldSymbol)
        {
            Range range = null;
            if (newOldSymbol.Line != null && newOldSymbol.EndLine != null && newOldSymbol.Position != null && newOldSymbol.EndPosition != null)
            {
                range = FileHelper.CreateRange(
                    (long) newOldSymbol.Line, (long) newOldSymbol.EndLine,
                    (long) newOldSymbol.Position, (long) newOldSymbol.EndPosition);
            }
            return range;
        }

        // isParent wäre das glaubs neu... 
        private bool SymbolIsInRangeOf(DafnyServer.OldSymbolTable.OldSymbolInformation child, DafnyServer.OldSymbolTable.OldSymbolInformation parent)
        {
            Range childRange = SymbolInformationToRange(child);
            Range parentRange = SymbolInformationToRange(parent);
            return  FileHelper.ChildIsContainedByParent(childRange, parentRange);
        }

        // einstieg über string können wir glaubs neu umgehen 
        public string GetParentForWord(string word)
        {
            return word is null ? null : _symbolTable.FirstOrDefault(x => x.Name == word)?.ParentClass;
        }

        private List<DafnyServer.OldSymbolTable.OldSymbolInformation> RemoveDuplicates(List<DafnyServer.OldSymbolTable.OldSymbolInformation> list)
        {
            return RemoveConstructorSymbols(list).GroupBy(x => x.Name).Select(x => x.First()).ToList();
        }

        private List<DafnyServer.OldSymbolTable.OldSymbolInformation> RemoveConstructorSymbols(List<DafnyServer.OldSymbolTable.OldSymbolInformation> list)
        {
            var ignoredSymbols = new[] { "_ctor", "_default" };
            list?.RemoveAll(x => ignoredSymbols.Any(x.Name.Contains));
            return list;
        }
    }
}
