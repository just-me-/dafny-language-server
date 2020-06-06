using System;
using Microsoft.Dafny;
using System.Collections.Generic;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.SymbolTable
{
    public interface ISymbolInformation
    {
        TokenPosition Position { get; set; }
        Uri FileUri { get; }
        string FileName { get; }
        int Line { get; }
        bool HasBody { get; }
        int Column { get; }
        int IdentifierEndColumn { get; }

        string Name { get; set; }
        Kind Kind { get; set; }
        Type Type { get; set; }
        UserDefinedType UserTypeDefinition { get; }

        ISymbolInformation Parent { get; set; }
        ISymbolInformation DeclarationOrigin { get; set; }
        Dictionary<string, ISymbolInformation> ChildrenHash { get; set; }
        List<ISymbolInformation> Children { get; }
        bool HasChildren { get; }
        List<ISymbolInformation> Params { get; set; }
        List<ISymbolInformation> BaseClasses { get; set; }
        bool HasInheritedMembers { get; }
        List<ISymbolInformation> Descendants { get; set; }
        bool IsDeclaration { get; }
        ISymbolInformation Module { get; set; }
        ISymbolInformation AssociatedDefaultClass { get; }
        List<ISymbolInformation> Usages { get; set; }
        IEnumerable<ISymbolInformation> GetAllOccurrences();

        string PositionToFormattedString();
        string ToDebugString();
        ISymbolInformation this[string index] { get; set; }
    }
}
