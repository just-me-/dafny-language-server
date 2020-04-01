using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public DafnyTranslationUnit(string fname, string source) : this(fname, source, new string[] { }) { }
        public DafnyTranslationUnit(string fname, string source, string[] args)
        {
            this.fname = fname;
            this.source = source;
            this.args = args;
        }

        private readonly string fname;
        private readonly string source;
        private readonly string[] args;

        private Microsoft.Dafny.Program dafnyProgram;
        private IEnumerable<Tuple<string, Bpl.Program>> boogiePrograms;

        #region ErrorReporting
        private ErrorReporter reporter = new Microsoft.Dafny.ConsoleErrorReporter();
        public List<DiagnosticError> Errors { get; } = new List<DiagnosticError>();

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

        public bool Verify()
        {
            Errors.Clear();
            reporter = new ConsoleErrorReporter();
            ServerUtils.ApplyArgs(args, reporter);
            bool success = Parse() && Resolve() && Translate() && Boogie();
            CollectErrorsFromReporter();
            return success;
        }

        private bool Parse()
        {
            ModuleDecl module = new LiteralModuleDecl(new Microsoft.Dafny.DefaultModuleDecl(), null);
            BuiltIns builtIns = new BuiltIns();
            var success = (Microsoft.Dafny.Parser.Parse(source, fname, fname, null, module, builtIns, new Microsoft.Dafny.Errors(reporter)) == 0 &&
                           Microsoft.Dafny.Main.ParseIncludes(module, builtIns, new List<string>(), new Microsoft.Dafny.Errors(reporter)) == null);
            if (success)
            {
                dafnyProgram = new Microsoft.Dafny.Program(fname, module, builtIns, reporter);
            }
            return success;

        }

        private bool Resolve()
        {
            var resolver = new Microsoft.Dafny.Resolver(dafnyProgram);
            resolver.ResolveProgram(dafnyProgram);
            return reporter.Count(ErrorLevel.Error) == 0;
        }

        private bool Translate()
        {
            boogiePrograms = Translator.Translate(dafnyProgram, reporter,
                new Translator.TranslatorFlags() { InsertChecksums = true, UniqueIdPrefix = fname });
            return true;
        }

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

        private bool BoogieOnce(string moduleName, Bpl.Program boogieProgram)
        {
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

        public List<SymbolTable.SymbolInformation> Symbols()
        {
            ServerUtils.ApplyArgs(args, reporter);
            if (Parse() && Resolve())
            {
                var symbolTable = new SymbolTable(dafnyProgram);
                return symbolTable.CalculateSymbols();
            }
            else
            {
                return new List<SymbolTable.SymbolInformation>();
            }
        }

        public CounterExampleResults CounterExample()
        {
            if (!File.Exists(fname))
            {
                throw new FileNotFoundException("CounterExample requires a valid filename. Invalid Path: " + fname);
            }
            var listArgs = args.ToList();
            listArgs.Add("/mv:" + CounterExampleProvider.ModelBvd);
            ServerUtils.ApplyArgs(listArgs.ToArray(), reporter);
            try
            {
                if (Parse() && Resolve() && Translate()) //2
                {
                    var boogieProgram = boogiePrograms.First(); //One CE is sufficient.
                    RemoveExistingModel();
                    BoogieOnce(boogieProgram.Item1, boogieProgram.Item2);
                    return new CounterExampleProvider(source).LoadCounterModel();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Error while collecting models: " + e.Message); //1
            }

            return new CounterExampleResults();
        }

        private void RemoveExistingModel()
        {
            if (File.Exists(CounterExampleProvider.ModelBvd))
            {
                File.Delete(CounterExampleProvider.ModelBvd);
            }
        }

        public void DotGraph()
        {
            ServerUtils.ApplyArgs(args, reporter);

            if (Parse() && Resolve() && Translate())
            {
                foreach (var boogieProgram in boogiePrograms)
                {
                    BoogieOnce(boogieProgram.Item1, boogieProgram.Item2);

                    foreach (var impl in boogieProgram.Item2.Implementations)
                    {
                        using (StreamWriter sw = new StreamWriter(fname + impl.Name + ".dot"))
                        {
                            sw.Write(boogieProgram.Item2.ProcessLoops(impl).ToDot());
                        }
                    }
                }
            }
        }
    }
}
