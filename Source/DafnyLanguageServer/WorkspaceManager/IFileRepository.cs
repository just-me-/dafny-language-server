using DafnyLanguageServer.Commons;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.WorkspaceManager
{
    public interface IFileRepository
    {
        PhysicalFile PhysicalFile { get; }
        TranslationResult Result { get; }
        ISymbolTableManager SymbolTableManager { get; }
        void UpdateFile(string sourceCodeOfFile);
        void UpdateFile(Container<TextDocumentContentChangeEvent> changes); 
    }
}
