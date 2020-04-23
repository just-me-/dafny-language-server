using DafnyLanguageServer.DafnyAccess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DafnyLanguageServer.SymbolTable;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This <c>WorkspaceManager</c> buffers for every Dafny file a valid intermediate version.
    /// The structure is key-value-based <c>URI, FileRepository</c>.
    /// This buffer is needed to have always a valid intermediate file version to provide features like <c>AutoCompletion</c> 
    /// even if the current file state would not be valid Dafny code (eg the user is typing a new line in his Dafny source file.)
    /// </summary>
    public class WorkspaceManager : IWorkspaceManager
    {
        private readonly ConcurrentDictionary<Uri, FileRepository> _files = new ConcurrentDictionary<Uri, FileRepository>();
        public SymbolTableManager SymbolTableManager { get; set; }

        public FileRepository UpdateFile(Uri documentPath, string sourceCodeOfFile)
        {
            FileRepository fileRepository = GetOrCreateFileRepositoryInWorkspace(documentPath);
            fileRepository.UpdateFile(sourceCodeOfFile);
            _files.AddOrUpdate(documentPath, fileRepository, (k, v) => fileRepository);
            
            //Generate new fancy Symbol Table for Testing:
            if (fileRepository.Result.TranslationStatus >= TranslationStatus.Resolved)
            {
                SymbolTableManager = new SymbolTableManager(fileRepository.Result.DafnyProgram);
                //das ändert sich noch, is ja pro klasse eine table im moment.
                //würde hie rdann aber so schreiben filerepo.symboltable = TableGenerator.GetTable oder sowas.
                // ==> pro Klasse... "pro file" kann man aber nicht sagen. Ein File kann ein Array von Klassen haben. Wenns pro "Modul/Package" ist ists Mapping via Workspace, nicht? 
            }

            return fileRepository;
        }

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
