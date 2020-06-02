using System;
using System.Collections.Concurrent;

namespace DafnyLanguageServer.WorkspaceManager
{
    /// <summary>
    /// This interface is used for dependency injection. Therefore <c>WorkspaceManagers</c> are more isolated for easy test writing. 
    /// </summary>
    public interface IWorkspace
    {
        IFileRepository UpdateFile<T>(Uri documentPath, T changes);
        IFileRepository GetFileRepository(Uri documentPath);
        IFileRepository GetFileRepository(string documentPath);
        ConcurrentDictionary<Uri, IFileRepository> GetAllFiles();
    }
}
