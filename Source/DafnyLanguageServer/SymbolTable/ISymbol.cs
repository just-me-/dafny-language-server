using System;
using Microsoft.Dafny;
using System.Collections.Generic;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbol
    {

        TokenPosition Position { get; set; }
        Uri FileUri { get; } //optionales todo: kann man das vereinfachen? Die Uri ist ja im Token.
        string FileName { get; }
        int Line { get; }
        int Column { get; }
        int ColumnStart { get; } //optionales todo: Warum ist col-Start im Interface, aber Line-start nicht?
        int ColumnEnd { get; }
        string Name { get; set; }
        Kind Kind { get; set; }
        Type Type { get; set; }
        UserDefinedType UserTypeDefinition { get; }

        ISymbol Parent { get; set; }
        ISymbol DeclarationOrigin { get; set; }
        Dictionary<string, ISymbol> ChildrenHash { get; set; }
        List<ISymbol> Children { get; }
        List<ISymbol> Usages { get; set; }
        List<ISymbol> BaseClasses { get; set; }
        List<ISymbol> Descendants { get; set; }
        bool IsDeclaration { get; }
        ISymbol Module { get; set; }
        ISymbol AssociatedDefaultClass { get; }

        string ToNiceString();
        string ToDebugString();

        bool Wraps(ISymbol child);
        bool Wraps(Uri file, int line, int character);

        IEnumerable<ISymbol> GetAllOccurrences();

        ISymbol this[string index] { get; set; }
    }
}
