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
                throw new ArgumentNullException("file must be set");

            this.file = file;
            this.args = args;
        }

        // Keep track of the process state  todo #148
        private TranslationStatus status = TranslationStatus.Virgin;

        private readonly PhysicalFile file;
        private readonly string[] args;
        private bool dirtyInstance = false; // only use once! 

        private Microsoft.Dafny.Program dafnyProgram; // behalten 
        private IEnumerable<Tuple<string, Bpl.Program>> boogiePrograms;  // behalten 

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
            if (dirtyInstance)
            {
                throw new Exception("You can this instance only use once!");
            }
            dirtyInstance = true;
        }

        /// <summary>
        ///  Calls Pars, Resolve, Translate and Boogie.
        /// Makes sure, that it gets only called once per class instance. 
        /// </summary>
        /// <returns></returns>
        public TranslationResult Verify()
        {
            checkInstance();

            // Apply args for counter example 
            var listArgs = args.ToList();
            listArgs.Add("/mv:" + CounterExampleModelFileTranslator.ModelBvd);
            ServerUtils.ApplyArgs(listArgs.ToArray(), reporter);

            if (Parse() && Resolve() && Translate() && Boogie())
                status = TranslationStatus.Boogied;

            CollectErrorsFromReporter();

            var result = new TranslationResult
            {
                Errors = Errors,
                BoogiePrograms = boogiePrograms,
                DafnyProgram = dafnyProgram,
                // Keep track of the process state todo #148
                TranslationStatus = status
            };
            return result;
        }

        /// <summary>
        /// Calls Dafny parser.
        /// </summary>
        private bool Parse()
        {
            ModuleDecl module = new LiteralModuleDecl(new Microsoft.Dafny.DefaultModuleDecl(), null);
            BuiltIns builtIns = new BuiltIns();
            var success = (Microsoft.Dafny.Parser.Parse(file.Sourcecode, file.Filepath, file.Filepath, null, module, builtIns, new Microsoft.Dafny.Errors(reporter)) == 0 &&
                           Microsoft.Dafny.Main.ParseIncludes(module, builtIns, new List<string>(), new Microsoft.Dafny.Errors(reporter)) == null);
            if (success)
            {
                dafnyProgram = new Microsoft.Dafny.Program(file.Filepath, module, builtIns, reporter);
                status = TranslationStatus.Parsed;
            }
            return success;
        }

        /// <summary>
        ///  Calls Dany resolver 
        /// </summary>
        private bool Resolve()
        {
            var resolver = new Microsoft.Dafny.Resolver(dafnyProgram);
            resolver.ResolveProgram(dafnyProgram);

            bool success = (reporter.Count(ErrorLevel.Error) == 0); 
            if (success) status = TranslationStatus.Resolved;
            return success;
        }

        /// <summary>
        ///  Translates Dafny programs into Boogie programs
        /// </summary>
        private bool Translate()
        {
            boogiePrograms = Translator.Translate(dafnyProgram, reporter,
                new Translator.TranslatorFlags() { InsertChecksums = true, UniqueIdPrefix = file.Filepath });
            status = TranslationStatus.Translated;
            return true;
        }

        /// <summary>
        /// In case there are multiple Boogie programs,
        /// verify them all. 
        /// </summary>
        /// <returns></returns>
        private bool Boogie()
        {
            foreach (var boogieProgram in boogiePrograms)
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
            CounterExampleModelFileTranslator.ClearDefaultModelFile();

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
