using System;
using System.Collections.Concurrent;
using DafnyLanguageServer.SymbolTable;

namespace DafnyLanguageServer.WorkspaceManager
{
    /// <summary>
    /// This interface is used for dependency injection. Therefore <c>WorkspaceManagers</c> are more isolated for easy test writing. 
    /// </summary>
    public interface IWorkspace
    {
        ISymbolTable SymbolTableSymbolTable { get; set; }
        FileRepository UpdateFile<T>(Uri documentPath, T changes);
        FileRepository GetFileRepository(Uri documentPath);
        FileRepository GetFileRepository(string documentPath);
        ConcurrentDictionary<Uri, FileRepository> GetAllFiles();
    }
}
