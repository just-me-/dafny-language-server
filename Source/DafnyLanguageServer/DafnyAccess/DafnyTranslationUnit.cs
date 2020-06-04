using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using DafnyLanguageServer.Commons;
using DafnyLanguageServer.Resources;
using DafnyLanguageServer.Tools;
using Bpl = Microsoft.Boogie;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This is the translation unit for Dafny files. The constructor accepts a Physical File.
    /// The class can then verify this file using the method "Verify()".
    /// It will parse, resolve, translate and verify the source code and return all information gathered in the form of TranslationResults.
    /// TranslationResults include the compilats 'dafnyProgram' and 'boogiePrograms', the success state of the verification, and also
    /// any errors that occured during the process.
    /// </summary>
    public class DafnyTranslationUnit : IDafnyTranslationUnit
    {
        public DafnyTranslationUnit(PhysicalFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file), ExceptionMessages.DTU_no_physical_file_given);
        }

        private TranslationStatus _status = TranslationStatus.Virgin;
        private bool _dirtyInstance; // can only verify once per dafnyProgram
        private readonly PhysicalFile _file;
        private Microsoft.Dafny.Program _dafnyProgram;
        private IEnumerable<Tuple<string, Bpl.Program>> _boogiePrograms;

        #region ErrorReporting
        private readonly ErrorReporter _reporter = new Microsoft.Dafny.ConsoleErrorReporter();
        private bool _hasErrors;
        private readonly List<DiagnosticElement> _diagnosticElements = new List<DiagnosticElement>();

        /// <summary>
        /// This method is passed to Boogie's Logical Correctness Verifier as a delegate.
        /// It converts errors, that Boogie reports to our unified "DiagnosticElement" Format
        /// and adds them to the DiagnosticElement Container.
        /// </summary>
        /// <param name="e">ErrorInformation from Boogie</param>
        private void AddBoogieErrorToList(ErrorInformation e)
        {
            _diagnosticElements.Add(e.ConvertToErrorInformation());
            _hasErrors = true;
        }

        /// <summary>
        /// This method iterates over the ErrorReporter "_reporter" of this class.
        /// This reporter is used by Dafny only (not Boogie) and any errors occuring during verification
        /// are collected within this reporter.
        /// This method is called after verification to add any errors and messages inside the reporter
        /// to the DiagnosticElement Collection of this class.
        /// Dafny-Errors (Type ErrorMessage) will also be converted to the unified DiagnosticElement Type
        /// during this process.
        /// </summary>
        private void CollectDiagnosticsFromReporter()
        {
            foreach (ErrorMessage e in _reporter.AllMessages[ErrorLevel.Error])
            {
                _diagnosticElements.Add(e.ConvertToErrorInformation(ErrorLevel.Error));
                _hasErrors = true;
            }

            foreach (ErrorMessage w in _reporter.AllMessages[ErrorLevel.Warning])
            {
                _diagnosticElements.Add(w.ConvertToErrorInformation(ErrorLevel.Warning));
            }

            foreach (ErrorMessage i in _reporter.AllMessages[ErrorLevel.Info])
            {
                _diagnosticElements.Add(i.ConvertToErrorInformation(ErrorLevel.Info));
            }
        }
        #endregion

        /// <summary>
        /// Ensures that this instance will only get used once.
        /// This is necessary since pre-existing 'dafnyPrograms' cannot be recompiled.
        /// </summary>
        private void CheckInstance()
        {
            if (_dirtyInstance)
            {
                throw new InvalidOperationException(Resources.ExceptionMessages.DTU_only_use_once);
            }
            _dirtyInstance = true;
        }

        /// <summary>
        /// This method verifies Dafny Code. First, it checks if this instance is fresh.
        /// Next, it sets up default options with the tweak to generate the model file, which is needed for counter examples.
        /// Then, it tries to parse, resolve, translate and boogie the code, aborting whenever it fails.
        /// Then, errors are collected and provided in the Property "DiagnosticElements".
        /// The results and compilats are then returned in the Wrapper Class "TranslationResult".
        /// </summary>
        public TranslationResult Verify()
        {
            ExecutionEngine.printer = new LanguageServerOutputWriterSink();

            CheckInstance();
            SetUpDafnyOptions();

            var succeeded = Parse() && Resolve() && Translate() && Boogie();

            CollectDiagnosticsFromReporter();

            if (succeeded && !_hasErrors)
            {
                _status = TranslationStatus.Verified;
            }

            return new TranslationResult
            {
                DiagnosticElements = _diagnosticElements,
                BoogiePrograms = _boogiePrograms,
                DafnyProgram = _dafnyProgram,
                TranslationStatus = _status

            };
        }

        /// <summary>
        /// Sets Up Default Dafny Options except for the ModelViewFile Option, which is custom.
        /// </summary>
        private void SetUpDafnyOptions()
        {
            DafnyOptions.Install(new DafnyOptions(_reporter));
            DafnyOptions.Clo.ApplyDefaultOptions();
            DafnyOptions.O.ModelViewFile = FileAndFolderLocations.modelBVD;

        }

        /// <summary>
        /// Invokes Dafny parser on the physical file provided to this instance.
        /// This will find lexer errors.
        /// </summary>
        private bool Parse()
        {
            ModuleDecl module = new LiteralModuleDecl(new Microsoft.Dafny.DefaultModuleDecl(), null);
            BuiltIns builtIns = new BuiltIns();
            var success = Microsoft.Dafny.Parser.Parse(_file.Sourcecode, _file.Filepath, _file.Filepath, null, module, builtIns, new Microsoft.Dafny.Errors(_reporter)) == 0 &&
                           Microsoft.Dafny.Main.ParseIncludes(module, builtIns, new List<string>(), new Microsoft.Dafny.Errors(_reporter)) == null;
            if (success)
            {
                _dafnyProgram = new Microsoft.Dafny.Program(_file.Filepath, module, builtIns, _reporter);
                _status = TranslationStatus.Parsed;
            }
            return success;
        }

        /// <summary>
        /// Invokes Dany resolver.
        /// This will find semantic errors.
        /// </summary>
        private bool Resolve()
        {
            var resolver = new Microsoft.Dafny.Resolver(_dafnyProgram);
            resolver.ResolveProgram(_dafnyProgram);

            bool success = _reporter.Count(ErrorLevel.Error) == 0;
            if (success)
            {
                _status = TranslationStatus.Resolved;
            }
            return success;
        }

        /// <summary>
        /// This method translates a Dafny program into Boogie programs.
        /// </summary>
        private bool Translate()
        {
            _boogiePrograms = Translator.Translate(_dafnyProgram, _reporter,
                new Translator.TranslatorFlags { InsertChecksums = true, UniqueIdPrefix = _file.Filepath });
            _status = TranslationStatus.Translated;
            return true;
        }

        /// <summary>
        /// Iterates over all Boogie programs and calls BoogieOnce for each of them.
        /// </summary>
        private bool Boogie()
        {
            foreach (var (moduleName, boogieProgram) in _boogiePrograms)
            {
                if (!BoogieOnce(moduleName, boogieProgram))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Prove correctness with Boogie for a single Boogie program.
        /// This will report logical (asserts, ensures, decreases, ...) errors.
        /// </summary>
        private bool BoogieOnce(string moduleName, Bpl.Program boogieProgram)
        {
            ClearModelFile();

            if (boogieProgram.Resolve() != 0 || boogieProgram.Typecheck() != 0)
            {
                return false;
            }

            ExecutionEngine.EliminateDeadVariables(boogieProgram);
            ExecutionEngine.CollectModSets(boogieProgram);
            ExecutionEngine.CoalesceBlocks(boogieProgram);
            ExecutionEngine.Inline(boogieProgram);

            var ps = new PipelineStatistics();
            var programId = "BoogieProgram_" + moduleName;
            var time = DateTime.UtcNow.Ticks.ToString();
            var boogieOutcome = ExecutionEngine.InferAndVerify(boogieProgram, ps, programId, AddBoogieErrorToList, time);

            bool success = boogieOutcome == PipelineOutcome.Done ||
                           boogieOutcome == PipelineOutcome.VerificationCompleted;

            if (success)
            {
                _status = TranslationStatus.Boogied;
            }
            return success;

        }
        /// <summary>
        /// Clears the model file. This is necassary, since no model means no counter examples.
        /// If the file exists from a previous run, it needs to be deleted.
        /// </summary>
        private void ClearModelFile()
        {
            if (File.Exists(FileAndFolderLocations.modelBVD))
            {
                File.Delete(FileAndFolderLocations.modelBVD);
            }
        }
    }
}
