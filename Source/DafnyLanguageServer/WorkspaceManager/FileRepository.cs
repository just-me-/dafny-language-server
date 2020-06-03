using System;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.SymbolTable;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.WorkspaceManager
{
    /// <summary>
    /// This class represents a buffered version of a Dafny file.
    /// Attributes like Uri, Dafny source code and not buffered informations are stored in the <c>PhysicalFile</c>.
    ///
    /// A <c>FileRepository</c> has also its own <c>TranslationResult</c> with all information provided by the verifier.
    /// The <c>SymbolTableManager</c> contains a buffered symbol table and allows navigation over it.
    /// </summary>
    public class FileRepository : IFileRepository
    {
        public FileRepository(PhysicalFile physicalFile)
        {
            PhysicalFile = physicalFile;
        }

        /// <summary>
        /// This CTOR is for testing only.
        /// </summary>
        public FileRepository(PhysicalFile physFile, TranslationResult results)
        {
            PhysicalFile = physFile;
            Result = results;
        }


        public PhysicalFile PhysicalFile { get; }
        public TranslationResult Result { get; private set; }
        public ISymbolTableManager SymbolTableManager { get; private set; } = new SymbolTableManager(SymbolTableGenerator.GetEmptySymbolTable());

        /// <summary>
        /// Updates the physical file with the provided sourcecode.
        /// </summary>
        /// <param name="sourceCodeOfFile">The new source code to be set.</param>
        public void UpdateFile(string sourceCodeOfFile)
        {
            PhysicalFile.Sourcecode = sourceCodeOfFile;
            GenerateTranslationResult();
            AddSymbolTable();
        }

        /// <summary>
        /// Updates the physical file with the provided ChangeEvents.
        /// Afterwards, the updated content gets verified.
        /// </summary>
        /// <param name="changes">The change events to be handled.</param>
        public void UpdateFile(Container<TextDocumentContentChangeEvent> changes)
        {
            foreach (var change in changes)
            {
                PhysicalFile.Apply(change);
            }

            GenerateTranslationResult();
            AddSymbolTable();
        }


        /// <summary>
        /// Creates a <c>DafnyTranslationUnit</c> and calls the verification process for the physical file of this instance.
        /// Afterwards, the updated content gets verified.
        /// </summary>
        private void GenerateTranslationResult()
        {
            if (PhysicalFile == null)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.file_repo_must_have_physical);
            }

            DafnyTranslationUnit translationUnit = new DafnyTranslationUnit(PhysicalFile);
            Result = translationUnit.Verify();
        }


        /// <summary>
        /// Sets the SymbolTableManager Property.
        /// Makes use of the dafnyProgram inside the Result property.
        /// </summary>
        private void AddSymbolTable()
        {

            if (Result.TranslationStatus >= TranslationStatus.Resolved &&
                Result.DafnyProgram != null)
            {
                SymbolTableGenerator s = new SymbolTableGenerator(Result.DafnyProgram);
                ISymbol rootnode = s.GenerateSymbolTable();
                SymbolTableManager = new SymbolTableManager(rootnode);
            }
        }
    }
}
