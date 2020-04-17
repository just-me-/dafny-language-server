using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.HandlerServices;
using DafnyServer;
using Bpl = Microsoft.Boogie;
using CounterExampleProvider = DafnyLanguageServer.HandlerServices.CounterExampleProvider;

namespace DafnyLanguageServer.DafnyAccess
{
    /// <summary>
    /// This is the translation unit for Dafny files. It calls parse, resolve, translate and Boogie's verification.
    /// The results are provided within the Property "DiagnosticElements"
    /// </summary>
    public class DafnyTranslationUnit : IDafnyTranslationUnit
    {
        public DafnyTranslationUnit(PhysicalFile file)
        {
            this.file = file ?? throw new ArgumentNullException(nameof(file), "Internal Error constructing DTU: File must be non-null.");
        }

        private TranslationStatus status = TranslationStatus.Virgin;
        private bool dirtyInstance = false; // can only verify once per dafnyProgram
        private readonly PhysicalFile file;
        private Microsoft.Dafny.Program dafnyProgram;
        private IEnumerable<Tuple<string, Bpl.Program>> boogiePrograms;

        #region ErrorReporting
        private readonly ErrorReporter reporter = new Microsoft.Dafny.ConsoleErrorReporter();
        private bool hasErrors;
        public List<DiagnosticElement> DiagnosticElements { get; } = new List<DiagnosticElement>();


        private void AddBoogieErrorToList(ErrorInformation e)
        {
            DiagnosticElements.Add(e.ConvertToErrorInformation());
            hasErrors = true;
        }

        private void CollectDiagnosticsFromReporter()
        {
            foreach (ErrorMessage e in reporter.AllMessages[ErrorLevel.Error])
            {
                DiagnosticElements.Add(e.ConvertToErrorInformation(ErrorLevel.Error));
                hasErrors = true;
            }

            foreach (ErrorMessage w in reporter.AllMessages[ErrorLevel.Warning])
            {
                DiagnosticElements.Add(w.ConvertToErrorInformation(ErrorLevel.Warning));
            }

            foreach (ErrorMessage i in reporter.AllMessages[ErrorLevel.Info])
            {
                DiagnosticElements.Add(i.ConvertToErrorInformation(ErrorLevel.Info));
            }
        }
        #endregion

        private void CheckInstance()
        {
            if (dirtyInstance)
            {
                throw new Exception("You can this instance only use once!");
            }
            dirtyInstance = true;
        }

        /// <summary>
        ///  This method verifies Dafny Code. First, it checks if this instance is fresh.
        /// Next, it sets up default options with the tweak to generate the model file, which is needed for counter examples.
        /// then, it tries to parse, resolve, translate and boogie the code, aborting whenever it fails.
        /// Then, errors are collected and provided in the Property "Error".
        /// The results are compilats are then returned in the Wrapper Class "TranslationResult".
        /// </summary>
        /// <returns></returns>
        public TranslationResult Verify()
        {
            CheckInstance();
            SetUpDafnyOptions();

            var succeeded = Parse() && Resolve() && Translate() && Boogie();

            CollectDiagnosticsFromReporter();

            if (succeeded && !hasErrors)
            {
                status = TranslationStatus.Verified;
            }
            var result = new TranslationResult
            {
                DiagnosticElements = DiagnosticElements,
                BoogiePrograms = boogiePrograms,
                DafnyProgram = dafnyProgram,
                TranslationStatus = status
            };
            return result;
        }

        private void SetUpDafnyOptions()
        {
            DafnyOptions.Install(new DafnyOptions(reporter));
            DafnyOptions.Clo.ApplyDefaultOptions();
            DafnyOptions.O.ModelViewFile = CounterExampleDefaultModelFile.FilePath;

        }

        /// <summary>
        /// Calls Dafny parser. Will find Lexer DiagnosticElements.
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
        ///  Calls Dany resolver. Will find semantic errors.
        /// </summary>
        private bool Resolve()
        {
            var resolver = new Microsoft.Dafny.Resolver(dafnyProgram);
            resolver.ResolveProgram(dafnyProgram);

            bool success = (reporter.Count(ErrorLevel.Error) == 0);
            if (success)
            {
                status = TranslationStatus.Resolved;
            }
        
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
        /// Just calls BoogieOnce for each of the Boogie Programs.
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
        /// Prove correctness with Boogie for a single Boogie program. Finds logical errors.
        /// </summary>
        private bool BoogieOnce(string moduleName, Bpl.Program boogieProgram)
        {
            CounterExampleDefaultModelFile.ClearDefaultModelFile();

            if (boogieProgram.Resolve() == 0 && boogieProgram.Typecheck() == 0)
            {
                ExecutionEngine.EliminateDeadVariables(boogieProgram);
                ExecutionEngine.CollectModSets(boogieProgram);
                ExecutionEngine.CoalesceBlocks(boogieProgram);
                ExecutionEngine.Inline(boogieProgram);

                var ps = new PipelineStatistics();
                var stringteil = "ServerProgram_" + moduleName;
                var time = DateTime.UtcNow.Ticks.ToString();
                var a = ExecutionEngine.InferAndVerify(boogieProgram, ps, stringteil, AddBoogieErrorToList, time);
                switch (a)
                {
                    case PipelineOutcome.Done:
                    case PipelineOutcome.VerificationCompleted:
                        status = TranslationStatus.Boogied;
                        return true;
                }
            }
            return false;
        }
    }
}
