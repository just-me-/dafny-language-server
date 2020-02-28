using System;
using System.Collections.Concurrent;

namespace DafnyLanguageServer.ContentManager
{
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
