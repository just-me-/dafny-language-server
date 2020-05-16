using System;
using System.Collections.Concurrent;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

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
        private readonly ConcurrentDictionary<Uri, FileRepository> _files = new ConcurrentDictionary<Uri, FileRepository>();
        public IManager SymbolTableManager { get; set; }

        /// <summary>
        /// Requests the fileRepository to apply updates and store it in the buffer.
        /// </summary>
        /// <typeparam name="T">string or Container with TextDocumentContentChangeEvent</typeparam>
        /// <param name="documentPath">URI to the document</param>
        /// <param name="sourceCodeOfFileOrChangeEvents">Update Information, either just the new complete sourcecode or TextDocumentChangeEvent-Container</param>
        /// <returns></returns>
        public FileRepository UpdateFile<T>(Uri documentPath, T sourceCodeOfFileOrChangeEvents)
        {
            FileRepository fileRepository = GetOrCreateFileRepositoryInWorkspace(documentPath);

            if (typeof(T) == typeof(string)) //full
            {
                var text = sourceCodeOfFileOrChangeEvents as string;
                fileRepository.UpdateFile(text);
            }
            else if (typeof(T) == typeof(Container<TextDocumentContentChangeEvent>)) //incremental; line(s)
            {
                var changes = sourceCodeOfFileOrChangeEvents as Container<TextDocumentContentChangeEvent>;
                fileRepository.UpdateFile(changes);
            }
            else
            {
                throw new TypeAccessException(Resources.ExceptionMessages.unexpected_file_type);
            }

            _files.AddOrUpdate(documentPath, fileRepository, (k, v) => fileRepository);

            //Generate new fancy Symbol Table for Testing:
            //kommt dann glaub eher in das filerepo rein
            // dokuuu "table ist auf workspace (vscode) basis, ned auf file basis oder so
            // wir müssen das noch diskutierne. das wäre natürlich die 'geile' lösung
            // aber das kreige wir ja niemals hin :O.
            // ich glaube, es wäre robuster,w enn wir wirklich pro file ein table machen.
            // das 'dafnyProgram' und der ganze bums ist ja auch einfach 'einmal pro file'^in den tranlsation results drin.
            // dies obwohl das dafnyProgram ja eignetlich alle includes und so enthält....
            // das wäre denk ich was aka "es wäre gut, aber wir haben es jetzt so gemacht, weil blabla".
            // aka dann etwas für die nächsten hust hstu xd.
            if (fileRepository.Result.TranslationStatus >= TranslationStatus.Resolved)
            {
                SymbolTableManager = new SymbolTableManager(fileRepository.Result.DafnyProgram);
                // if not null... update changes 
                // not create all symbols every time... todo 
                // get wrapping symbol of cursor position... update "this childs in symbols". 
                // commpile "just this file again" 
            }

            return fileRepository;
        }

        /// <summary>
        /// Retreives a fileRepository from the buffer. If it doesn't exist, it will be created.
        /// </summary>
        /// <param name="documentPath">URI to the file.</param>
        /// <returns>FileRepository</returns>
        private FileRepository GetOrCreateFileRepositoryInWorkspace(Uri documentPath)
        {
            return _files.TryGetValue(documentPath, out var bufferedFile)
                ? bufferedFile
                : new FileRepository
                {
                    PhysicalFile = new PhysicalFile
                    {
                        Uri = documentPath,
                        Filepath = documentPath.LocalPath
                    }
                };
        }

        public FileRepository GetFileRepository(Uri documentPath)
        {
            return GetOrCreateFileRepositoryInWorkspace(documentPath);
        }

        public FileRepository GetFileRepository(string documentPath)
        {
            return GetFileRepository(new Uri(documentPath));
        }

        public ConcurrentDictionary<Uri, FileRepository> GetAllFiles()
        {
            return _files;
        }
    }
}
