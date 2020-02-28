using Microsoft.Boogie;
using Microsoft.Dafny;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bpl = Microsoft.Boogie;

namespace DafnyLanguageServer.DafnyAccess
{
    // needed cuz boogie needs an output stream - must not be null 
    public class DafnyConsolePrinter : ConsolePrinter
    {
        public override void ReportBplError(IToken tok, string message, bool error, TextWriter tw, string category = null)
        {
            // Dafny has 0-indexed columns, but Boogie counts from 1
            var realigned_tok = new Token(tok.line, tok.col - 1);
            realigned_tok.kind = tok.kind;
            realigned_tok.pos = tok.pos;
            realigned_tok.val = tok.val;
            realigned_tok.filename = tok.filename;
            base.ReportBplError(realigned_tok, message, error, tw, category);

            if (tok is Microsoft.Dafny.NestedToken)
            {
                var nt = (Microsoft.Dafny.NestedToken)tok;
                ReportBplError(nt.Inner, "Related location", false, tw);
            }
        }
    }

    public class DafnyTranslationUnit : IDafnyTranslationUnit
    {
        private string fname;
        private string source;
        private string[] args;

        private readonly ErrorReporter reporter = new Microsoft.Dafny.ConsoleErrorReporter();
        private Microsoft.Dafny.Program dafnyProgram;
        private IEnumerable<Tuple<string, Bpl.Program>> boogiePrograms;

        private List<ErrorInformation> _errors = new List<ErrorInformation>();

        private void AddErrorToList(ErrorInformation e)
        {
            _errors.Add(e);
        }

        public DafnyTranslationUnit(string fname, string source) : this(fname, source, new string[] { }) { }
        public DafnyTranslationUnit(string fname, string source, string[] args)
        {
            this.fname = fname;
            this.source = source;
            this.args = args;
        }

        public bool Verify()
        {
            ServerUtils.ApplyArgs(args, reporter);
            return Parse() && Resolve() && Translate() && Boogie();
        }

        public List<ErrorInformation> GetErrors()
        {
            _errors = new List<ErrorInformation>();
            Verify();
            return _errors;
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

        public List<CounterExampleProvider.CounterExample> CounterExample()
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
                if (Parse() && Resolve() && Translate())
                {
                    var counterExampleProvider = new CounterExampleProvider();
                    List<CounterExampleProvider.CounterExample> counterExamples = new List<CounterExampleProvider.CounterExample>();
                    foreach (var boogieProgram in boogiePrograms)
                    {
                        RemoveExistingModel();
                        BoogieOnce(boogieProgram.Item1, boogieProgram.Item2);
                        var model = counterExampleProvider.LoadCounterModel();
                        counterExamples.Add(model);
                    }
                    return counterExamples;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Error while collecting models: " + e.Message);
            }

            return new List<CounterExampleProvider.CounterExample>();
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
