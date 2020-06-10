using DafnyLanguageServer.Commons;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Collections.Concurrent;

namespace DafnyLanguageServer.WorkspaceManager
{
    /// <summary>
    /// This <c>WorkspaceManager</c> buffers for every Dafny file a valid intermediate version.
    /// The structure is key-value-based <c>URI, FileRepository</c>.
    /// This buffer is needed to have always a valid intermediate file version to provide features like <c>AutoCompletion</c>
    /// even if the current file state would not be valid Dafny code (eg the user is typing a new line in his Dafny source file.)
    /// </summary>
    public class Workspace : IWorkspace
    {
        private readonly ConcurrentDictionary<Uri, IFileRepository> _files = new ConcurrentDictionary<Uri, IFileRepository>();

        /// <summary>
        /// Requests the fileRepository to apply updates and store it in the buffer.
        /// </summary>
        /// <typeparam name="T">string or Container with TextDocumentContentChangeEvent</typeparam>
        /// <param name="documentPath">URI to the document</param>
        /// <param name="changes">Update Information, either just the new complete sourcecode or TextDocumentChangeEvent-Container</param>
        /// <returns></returns>
        public IFileRepository UpdateFile<T>(Uri documentPath, T changes)
        {
            IFileRepository fileRepository = GetOrCreateFileRepositoryInWorkspace(documentPath);

            if (typeof(T) == typeof(string)) //Sync Kind Full
            {
                var text = changes as string;
                fileRepository.UpdateFile(text);
            }
            else if (typeof(T) == typeof(Container<TextDocumentContentChangeEvent>)) //Sync Kind Incremental
            {
                var changes1 = changes as Container<TextDocumentContentChangeEvent>;
                fileRepository.UpdateFile(changes1);
            }
            else
            {
                throw new TypeAccessException(Resources.ExceptionMessages.unexpected_file_type);
            }

            _files.AddOrUpdate(documentPath, fileRepository, (k, v) => fileRepository);

            return fileRepository;
        }

        /// <summary>
        /// Retreives a fileRepository from the buffer. If it doesn't exist, it will be created.
        /// </summary>
        /// <param name="documentPath">URI to the file.</param>
        /// <returns>FileRepository</returns>
        private IFileRepository GetOrCreateFileRepositoryInWorkspace(Uri documentPath)
        {
            return _files.TryGetValue(documentPath, out var bufferedFile)
                ? bufferedFile
                : new FileRepository(new PhysicalFile
                {
                    Uri = documentPath,
                    Filepath = documentPath.LocalPath,
                    Sourcecode = string.Empty
                });
        }

        public IFileRepository GetFileRepository(Uri documentPath)
        {
            return GetOrCreateFileRepositoryInWorkspace(documentPath);
        }

        public IFileRepository GetFileRepository(string documentPath)
        {
            return GetFileRepository(new Uri(documentPath));
        }

        public ConcurrentDictionary<Uri, IFileRepository> GetAllFiles()
        {
            return _files;
        }
    }
}