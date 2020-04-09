using DafnyLanguageServer.DafnyAccess;
using System.Collections.Generic;
using System.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DafnyServer;
using Microsoft.Boogie;
using SymbolInformation = DafnyServer.SymbolTable.SymbolInformation;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This <c>FileSymboltableProcessor</c> provides all symbols that were found in a (valid) Dafny file.
    /// This newSymbol list can be used for features like <c>AutoCompletion</c>.
    /// </summary>
    public class FileSymboltableProcessor
    {
        private readonly List<SymbolInformation> _symbolTable;
        public bool HasEntries => (_symbolTable.Count > 0);

        public FileSymboltableProcessor(List<SymbolInformation> symbolTable)
        {
            _symbolTable = symbolTable;
        }

        public List<SymbolInformation> GetFullList()
        {
            return RemoveConstructorSymbols(_symbolTable);
        }

        public List<SymbolInformation> GetList()
        {
            return RemoveDuplicates(_symbolTable);
        }

        public List<SymbolInformation> GetList(string identifier)
        {
            if (identifier is null)
            {
                return GetList();
            }
            var parentSymbol = GetSymbolByName(identifier);
            return RemoveDuplicates(_symbolTable.Where(x => (x.ParentClass == identifier && SymbolIsInRangeOf(x, parentSymbol))).ToList());
        }

        private SymbolInformation GetSymbolByName(string name)
        {
            return _symbolTable.FirstOrDefault(x => (x.Name == name));
        }

        private Range SymbolInformationToRange(SymbolInformation newSymbol)
        {
            Range range = null;
            if (newSymbol.Line != null && newSymbol.EndLine != null && newSymbol.Position != null && newSymbol.EndPosition != null)
            {
                range = FileHelper.CreateRange(
                    (long) newSymbol.Line, (long) newSymbol.EndLine,
                    (long) newSymbol.Position, (long) newSymbol.EndPosition);
            }
            return range;
        }

        private bool SymbolIsInRangeOf(SymbolInformation child, SymbolInformation parent)
        {
            Range childRange = SymbolInformationToRange(child);
            Range parentRange = SymbolInformationToRange(parent);
            return  FileHelper.ChildIsContainedByParent(childRange, parentRange);
        }

        public string GetParentForWord(string word)
        {
            return word is null ? null : _symbolTable.FirstOrDefault(x => x.Name == word)?.ParentClass;
        }

        private List<SymbolInformation> RemoveDuplicates(List<SymbolInformation> list)
        {
            return RemoveConstructorSymbols(list).GroupBy(x => x.Name).Select(x => x.First()).ToList();
        }

        private List<SymbolInformation> RemoveConstructorSymbols(List<SymbolInformation> list)
        {
            var ignoredSymbols = new[] { "_ctor", "_default" };
            list?.RemoveAll(x => ignoredSymbols.Any(x.Name.Contains));
            return list;
        }
    }
}
