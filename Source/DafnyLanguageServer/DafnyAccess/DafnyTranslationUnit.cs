using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using DafnyServer;
using Bpl = Microsoft.Boogie;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This is the translation unit for Dafny files. It calls parse, resolve, translate and Boogie's verification.
    /// The results are provided within the Property "Errors"
    /// </summary>
    public class DafnyTranslationUnit : IDafnyTranslationUnit
    {
        public DafnyTranslationUnit(PhysicalFile file) : this(file, new string[] { }) { }
        public DafnyTranslationUnit(PhysicalFile file, string[] args)
        {
            if (file == null) 
                throw new ArgumentNullException("_file must be set");

            this._file = file;
            this._args = args;
        }

        // Keep track of the process state  todo #148
        private TranslationStatus _status = TranslationStatus.Virgin;
        private bool _dirtyInstance = false; // only use once! 

        private Microsoft.Dafny.Program _dafnyProgram;
        private IEnumerable<Tuple<string, Bpl.Program>> _boogiePrograms;

        private readonly PhysicalFile _file;
        private readonly string[] _args;

        #region ErrorReporting
        private ErrorReporter reporter = new Microsoft.Dafny.ConsoleErrorReporter();
        public List<DiagnosticError> Errors { get; } = new List<DiagnosticError>(); // behaltem

        private void AddErrorToList(ErrorInformation e)
        {
            Errors.Add(e.ConvertToErrorInformation());
        }

        private void AddErrorToList(ErrorMessage e)
        {
            Errors.Add(e.ConvertToErrorInformation());
        }

        private void CollectErrorsFromReporter()
        {
            foreach (ErrorMessage error in reporter.AllMessages[ErrorLevel.Error])
            {
                AddErrorToList(error);
            }
        }
        #endregion

        private void checkInstance()
        {
            if (_dirtyInstance)
            {
                throw new Exception("You can this instance only use once!");
            }
            _dirtyInstance = true;
        }

        /// <summary>
        /// Calls Pars, Resolve, Translate and Boogie.
        /// Makes sure, that it gets only called once per class instance. 
        /// </summary>
        public TranslationResult Verify()
        {
            checkInstance();

            // Apply _args for counter example 
            var listArgs = _args.ToList();
            listArgs.Add("/mv:" + CounterExampleProvider.ModelBvd);
            ServerUtils.ApplyArgs(listArgs.ToArray(), reporter);

            if (Parse() && Resolve() && Translate() && Boogie())
                _status = TranslationStatus.Boogied;

            CollectErrorsFromReporter();

            var result = new TranslationResult
            {
                Errors = Errors,
                BoogiePrograms = _boogiePrograms,
                DafnyProgram = _dafnyProgram,
                // Keep track of the process state todo #148
                TranslationStatus = _status
            };
            return result;
        }

        /// <summary>
        /// Calls Dafny parser
        /// </summary>
        private bool Parse()
        {
            ModuleDecl module = new LiteralModuleDecl(new Microsoft.Dafny.DefaultModuleDecl(), null);
            BuiltIns builtIns = new BuiltIns();
            var success = (Microsoft.Dafny.Parser.Parse(_file.Sourcecode, _file.Filepath, _file.Filepath, null, module, builtIns, new Microsoft.Dafny.Errors(reporter)) == 0 &&
                           Microsoft.Dafny.Main.ParseIncludes(module, builtIns, new List<string>(), new Microsoft.Dafny.Errors(reporter)) == null);
            if (success)
            {
                _dafnyProgram = new Microsoft.Dafny.Program(_file.Filepath, module, builtIns, reporter);
                _status = TranslationStatus.Parsed;
            }
            return success;
        }

        /// <summary>
        ///  Calls Dany resolver 
        /// </summary>
        private bool Resolve()
        {
            var resolver = new Microsoft.Dafny.Resolver(_dafnyProgram);
            resolver.ResolveProgram(_dafnyProgram);

            bool success = (reporter.Count(ErrorLevel.Error) == 0); 
            if (success) _status = TranslationStatus.Resolved;
            return success;
        }

        /// <summary>
        ///  Translates Dafny programs into Boogie programs
        /// </summary>
        private bool Translate()
        {
            _boogiePrograms = Translator.Translate(_dafnyProgram, reporter,
                new Translator.TranslatorFlags() { InsertChecksums = true, UniqueIdPrefix = _file.Filepath });
            _status = TranslationStatus.Translated;
            return true;
        }

        /// <summary>
        /// In case there are multiple Boogie programs,
        /// verify them all. 
        /// </summary>
        private bool Boogie()
        {
            foreach (var boogieProgram in _boogiePrograms)
            {
                if (!BoogieOnce(boogieProgram.Item1, boogieProgram.Item2))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Prove correctness with Boogie for a single Boogie program.  
        /// </summary>
        private bool BoogieOnce(string moduleName, Bpl.Program boogieProgram)
        {
            CounterExampleProvider.RemoveExistingFileModel();
            if (boogieProgram.Resolve() == 0 && boogieProgram.Typecheck() == 0)
            {
                ExecutionEngine.EliminateDeadVariables(boogieProgram);
                ExecutionEngine.CollectModSets(boogieProgram);
                ExecutionEngine.CoalesceBlocks(boogieProgram);
                ExecutionEngine.Inline(boogieProgram);

                var ps = new PipelineStatistics();
                var stringteil = "ServerProgram_" + moduleName;
                var time = DateTime.UtcNow.Ticks.ToString();
                var a = ExecutionEngine.InferAndVerify(boogieProgram, ps, stringteil, AddErrorToList, time);
                switch (a)
                {
                    case PipelineOutcome.Done:
                    case PipelineOutcome.VerificationCompleted:
                        return true;
                }
            }
            return false;
        }
    }
}
