using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;
using Microsoft.Dafny;
using Type = Microsoft.Dafny.Type;

namespace SymbolTableTest
{
    class SymbolInformationFake : SymbolInformation, ISymbol
    {
        public override int? Line => _lineStart;
        public override int? LineStart => _lineStart;
        public override int? LineEnd => _lineEnd;
        public override int? ColumnStart => _columnStart;
        public override int? ColumnEnd => _columnEnd;

        private int _lineStart;
        private int _lineEnd;
        private int _columnStart;
        private int _columnEnd;

        public SymbolInformationFake(int lineStart, int lineEnd, int columnStart, int columnEnd, string name = "no_name")
        {
            _lineStart = lineStart;
            _lineEnd = lineEnd;
            _columnStart = columnStart;
            _columnEnd = columnEnd;
            Name = name;
            ChildrenHash = new Dictionary<string, ISymbol>();
            Descendants = null;
            Parent = null;
        }

        public void AddChild(SymbolInformationFake child)
        {
            ChildrenHash.Add(child.Name, child);
        }

        public void AddSubsymbol(SymbolInformation child)
        {
            if (Descendants == null)
            {
                Descendants = new List<ISymbol>();
            }
            Descendants.Add(child);
        }

        public void SetParent(SymbolInformation parent)
        {
            Parent = parent;
        }
    }
}
