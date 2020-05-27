using DafnyLanguageServer.Commons;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

// actually todo use the interface
namespace DafnyLanguageServer.WorkspaceManager
{
    public interface IFileRepository
    {
        PhysicalFile PhysicalFile { get; }
        TranslationResult Result { get; }
        ISymbolTableManager SymbolTableManager { get; }
        void UpdateFile(string sourceCodeOfFile); //todo not used - gegen interface progamieren 
        void UpdateFile(Container<TextDocumentContentChangeEvent> changes); //todo not used - gegen interface progamieren 
    }
}
