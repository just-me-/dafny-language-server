using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.Handler;
using Microsoft.Boogie;
using Microsoft.Dafny;
using ErrorProc = Microsoft.Boogie.ErrorProc;
using Type = Microsoft.Dafny.Type;

namespace DafnyLanguageServer.HandlerServices
{
    /// <summary>
    ///  This service is used by the <c>CompileHandler</c>. 
    /// </summary>
    public class CompilationService
    {
        public const string failurePrefix = "Compilation failed: ";
        private string[] CompilationArgs { get; }

        private FileRepository FileRepo { get; }
        private string Path => FileRepo.PhysicalFile.Filepath;
        private Microsoft.Dafny.Program Dp => FileRepo.Result.DafnyProgram;

        public CompilationService(FileRepository fileRepo, string[] args)
        {
            FileRepo = fileRepo;

            var argsAsList = args.ToList();
            argsAsList.Add(Path);
            CompilationArgs = argsAsList.ToArray();
        }

        public CompilerResults Compile()
        {

            var exitval = DafnyDriver.ProcessCommandLineArguments(CompilationArgs, out var dafnyFiles, out var otherFiles);

            if (!PerformPreliminaryChecks(exitval, out var errorneousCompileResults))
            {
                return errorneousCompileResults;
            }

            TextWriter tw = new StringWriter();
            Type.ResetScopes();
            int oldErrorCount = Dp.reporter.Count(ErrorLevel.Error);
            DafnyDriver.CompileDafnyProgram(Dp, Path, otherFiles.AsReadOnly(), true, tw);
            int newErrorCount = Dp.reporter.Count(ErrorLevel.Error);

            if (oldErrorCount != newErrorCount)
            {
                return ExtractError(oldErrorCount);
            }

            bool hasMain = new Microsoft.Dafny.CsharpCompiler(Dp.reporter).HasMain(Dp, out _);

            return new CompilerResults()
            {
                Error = false,
                Executable = hasMain,
                Message = tw.ToString().TrimEnd('\r', '\n')
            };

        }

        private CompilerResults ExtractError(int oldErrorCount)
        {
            var allErrors = Dp.reporter.AllMessages[ErrorLevel.Error];
            var newError = allErrors.GetRange(oldErrorCount, 1).First();

            var msg = newError.message;
            var line = newError.token.line;
            var col = newError.token.col;

            return CreateErrorneousResults($"[L{line}:C{col}] {msg}");
        }

        private bool PerformPreliminaryChecks(DafnyDriver.ExitValue exitval, out CompilerResults compilerResults)
        {
            if (System.IO.Path.GetExtension(Path) != ".dfy")
            {
                {
                    compilerResults = CreateErrorneousResults("Can only compile .dfy files");
                    return false;
                }
            }

            if (!File.Exists(FileRepo.PhysicalFile.Filepath))
            {
                {
                    compilerResults = CreateErrorneousResults("Could not locate file: " + Path);
                    return false;
                }
            }

            if (FileRepo.Result.TranslationStatus != TranslationStatus.Verified)
            {
                var firstError = FileRepo.Result.DiagnosticElements.First();
                var msg = firstError.Msg;
                var line = firstError.Tok.line;
                var col = firstError.Tok.col;

                {
                    compilerResults = CreateErrorneousResults($"[L{line}:C{col}] {msg}");
                    return false;
                }
            }

            if (!DafnyOptions.O.Compile)
            {
                {
                    compilerResults = CreateErrorneousResults("Compile CLO is explicitly set to 0.");
                    return false;
                }
            }

            if (exitval == DafnyDriver.ExitValue.PREPROCESSING_ERROR)
            {
                {
                    compilerResults = CreateErrorneousResults($"Error while preprocessing your custom command line arguments.");
                    return false;
                }
            }

            compilerResults = null;
            return true;
        }

        private CompilerResults CreateErrorneousResults(string msg)
        {
            return new CompilerResults
            {
                Error = true,
                Message = failurePrefix + msg,
                Executable = false
            };
        }



    }

}
