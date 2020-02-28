using DafnyLanguageServer.DafnyAccess;
using System.Collections.Generic;
using System.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SymbolTable = DafnyLanguageServer.DafnyAccess.SymbolTable;

namespace DafnyLanguageServer.ContentManager
{
    public class FileSymboltable
    {
        private List<SymbolTable.SymbolInformation> _symbolTable;
        public bool HasEntries => (_symbolTable.Count > 0);
        private IDafnyTranslationUnit _translationUnit;

        public FileSymboltable(IDafnyTranslationUnit translationUnit)
        {
            _translationUnit = translationUnit;
            _symbolTable = GetSymbolList();
        }

        public List<SymbolTable.SymbolInformation> GetFullList()
        {
            return RemoveConstructorSymbols(_symbolTable);
        }

        public List<SymbolTable.SymbolInformation> GetList()
        {
            return RemoveDuplicates(_symbolTable);
        }

        public List<SymbolTable.SymbolInformation> GetList(string identifier)
        {
            if (identifier is null)
            {
                return GetList();
            }
            var parentSymbol = GetSymbolByName(identifier);
            return RemoveDuplicates(_symbolTable.Where(x => (x.ParentClass == identifier && SymbolIsInRangeOf(x, parentSymbol))).ToList());
        }

        private SymbolTable.SymbolInformation GetSymbolByName(string name)
        {
            return _symbolTable.FirstOrDefault(x => (x.Name == name));
        }

        private Range SymbolInformationToRange(SymbolTable.SymbolInformation symbol)
        {
            Range range = null;
            if (symbol.Line != null && symbol.EndLine != null && symbol.Position != null && symbol.EndPosition != null)
            {
                range = FileHelper.CreateRange(
                    (long) symbol.Line, (long) symbol.EndLine,
                    (long) symbol.Position, (long) symbol.EndPosition);
            }
            return range;
        }

        private bool SymbolIsInRangeOf(SymbolTable.SymbolInformation child, SymbolTable.SymbolInformation parent)
        {
            Range childRange = SymbolInformationToRange(child);
            Range parentRange = SymbolInformationToRange(parent);
            return  FileHelper.ChildIsContainedByParent(childRange, parentRange);
        }

        public string GetParentForWord(string word)
        {
            return word is null ? null : _symbolTable.FirstOrDefault(x => x.Name == word)?.ParentClass;
        }

        private List<SymbolTable.SymbolInformation> GetSymbolList()
        {
            return _translationUnit.Symbols();
        }

        private List<SymbolTable.SymbolInformation> RemoveDuplicates(List<SymbolTable.SymbolInformation> list)
        {
            return RemoveConstructorSymbols(list).GroupBy(x => x.Name).Select(x => x.First()).ToList();
        }

        private List<SymbolTable.SymbolInformation> RemoveConstructorSymbols(List<SymbolTable.SymbolInformation> list)
        {
            var ignoredSymbols = new[] { "_ctor", "_default" };
            list?.RemoveAll(x => ignoredSymbols.Any(x.Name.Contains));
            return list;
        }
    }
}
