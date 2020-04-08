using System;
using System.Collections.Concurrent;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This interface is used for dependency injection. Therefore <c>WorkspaceManagers</c> are more isolated for easy test writing. 
    /// </summary>
    public interface IWorkspaceManager
    {
        FileRepository UpdateFile(Uri documentPath, string sourceCodeOfFile);
        FileRepository GetFileRepository(Uri documentPath);
        FileRepository GetFileRepository(string documentPath);
        ConcurrentDictionary<Uri, FileRepository> GetAllFiles();
    }
}
