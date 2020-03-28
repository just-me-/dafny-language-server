using System;
using System.Collections.Concurrent;

namespace DafnyLanguageServer.ContentManager
{
    /// <summary>
    /// This interface is used for dependency injection. Therefore <c>BufferManagers</c> are more isolated for easy test writing. 
    /// </summary>
    public interface IBufferManager
    {
        DafnyFile UpdateBuffer(Uri documentPath, string sourceCodeOfFile);
        DafnyFile GetFile(Uri documentPath);
        DafnyFile GetFile(string documentPath);
        string GetSourceCodeAsText(Uri documentPath);
        FileSymboltable GetSymboltable(Uri documentPath);
        ConcurrentDictionary<Uri, DafnyFile> GetAllFiles();
    }
}
