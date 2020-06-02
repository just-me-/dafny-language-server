using System;
using Microsoft.Dafny;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbol
    {

        TokenPosition Position { get; set; }
        Uri FileUri { get; }
        string FileName { get; }
        int Line { get; }
        int? BodyLineStart { get; }
        int? BodyLineEnd { get; }
        bool HasBody { get; }
        int Column { get; }
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
        bool HasInheritedMembers { get; }
        List<ISymbol> Descendants { get; set; }
        bool IsDeclaration { get; }
        ISymbol Module { get; set; }
        ISymbol AssociatedDefaultClass { get; }
        bool HasChildren { get; }

        string ToNiceString();
        string ToDebugString();

        bool Wraps(ISymbol child);
        bool Wraps(Uri file, int line, int character);

        IEnumerable<ISymbol> GetAllOccurrences();

        ISymbol this[string index] { get; set; }
    }
}
