﻿using DafnyLanguageServer.DafnyAccess;
using System;
using System.Collections.Concurrent;

namespace DafnyLanguageServer.ContentManager
{
    /// <summary>
    /// This <c>BufferManager</c> buffers for every Dafny file a valid intermediate version.
    /// The structure is key-value-based <c>URI, DafnyFile</c>.
    /// This buffer is needed to have always a valid intermediate file version to provide features like <c>AutoCompletion</c> 
    /// even if the current file state would not be valid Dafny code (eg the user is typing a new line in his Dafny source file.)
    /// </summary>
    public class BufferManager : IBufferManager
    {
        private readonly ConcurrentDictionary<Uri, DafnyFile> _buffers = new ConcurrentDictionary<Uri, DafnyFile>();

        public DafnyFile UpdateBuffer(Uri documentPath, string sourceCodeOfFile)
        {
            DafnyFile file = GetOrCreateDafnyfileInstance(documentPath);
            file.Sourcecode = sourceCodeOfFile;
            file.DafnyTranslationUnit = new DafnyTranslationUnit(file.Filepath, file.Sourcecode);

            // do not update symboltable if current file state is invalid 
            var symboltable = new FileSymboltable(file.DafnyTranslationUnit);
            if (symboltable.HasEntries)
            {
                file.Symboltable = symboltable;
            }

            _buffers.AddOrUpdate(documentPath, file, (k, v) => file);
            return file;
        }

        private DafnyFile GetOrCreateDafnyfileInstance(Uri documentPath)
        {
            return _buffers.TryGetValue(documentPath, out var bufferedFile) ? bufferedFile : new DafnyFile { Uri = documentPath };
        }

        public DafnyFile GetFile(Uri documentPath)
        {
            return GetOrCreateDafnyfileInstance(documentPath);
        }

        public DafnyFile GetFile(string documentPath)
        {
            return GetFile(new Uri(documentPath));
        }

        public string GetSourceCodeAsText(Uri documentPath)
        {
            return _buffers.TryGetValue(documentPath, out var buffer) ? buffer.Sourcecode : null;
        }

        public FileSymboltable GetSymboltable(Uri documentPath)
        {
            return _buffers.TryGetValue(documentPath, out var buffer) ? buffer.Symboltable : null;
        }

        public ConcurrentDictionary<Uri, DafnyFile> GetAllFiles()
        {
            return _buffers;
        }
    }
}
