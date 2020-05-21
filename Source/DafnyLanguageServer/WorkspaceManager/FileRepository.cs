using System;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Core;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Handler;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using CounterExampleProvider = DafnyLanguageServer.Core.CounterExampleProvider;

namespace DafnyLanguageServer.WorkspaceManager
{
    /// <summary>
    /// This class represents a buffered version of a Dafny file.
    /// Attributes like Uri, Dafny source code and not buffered informations are stored in the <c>PhysicalFile</c>.
    ///
    /// A <c>FileRepository</c> has also its own <c>TranslationResult</c> with all information provided by the verifier.
    /// </summary>
    public class FileRepository
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

        /// <summary>
        /// Updates the physical file with the provided sourcecode.
        /// </summary>
        /// <param name="sourceCodeOfFile">The new source code to be set.</param>
        public void UpdateFile(string sourceCodeOfFile)
        {
            PhysicalFile.Sourcecode = sourceCodeOfFile;
            GenerateTranslationResult();
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




        //todo das wäre besser aufgehoben wieter oben, aka new Provider(FileRepo) wenn mand en dependency graph ankuckt.
        /// <summary>
        /// Invokes the CounterExampleProvider to extract counter examples.
        /// </summary>
        /// <returns>CounterExample result wrapper to be handled by the client. If none were found, the result wrapper will just be empty.</returns>
        public CounterExampleResults CounterExample()
        {
            try
            {
                if (Result.TranslationStatus >= TranslationStatus.Translated)
                {
                    ICounterExampleProvider provider = new CounterExampleProvider(PhysicalFile);
                    return provider.LoadCounterModel();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.could_not_collect_models + " " + e.Message);
            }
            return new CounterExampleResults();
        }

        /// <summary>
        /// Invokes <c>CompilationService</c> to create an executable.
        /// </summary>
        /// <param name="requestCompilationArguments">Custom Arguments for compilation as far as applicable.</param>
        /// <returns>CompilationResults Wrapper to be sent to the LSP client.</returns>
        public CompilerResults Compile(string[] requestCompilationArguments)
        {
            try
            {
                return new CompileProvider(this, requestCompilationArguments).Compile();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.could_not_execute_compilation + " " + e.Message);
            }
        }
    }
}
