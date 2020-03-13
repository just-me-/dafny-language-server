using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Handler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dafny;

namespace DafnyLanguageServer.Services
{
    public class CompilationService
    {

        private string[] CompilationArgs { get; }

        private string DfyFile { get; }

        public CompilationService(string file, string[] args)
        {
            DfyFile = file;
            CompilationArgs = args;
        }

        public async Task<CompilerResults> Compile()
        {
            return await Task.Run(() =>
            {
                var argsAsList = CompilationArgs.ToList();
                argsAsList.Add(DfyFile);
                var finalArgs = argsAsList.ToArray();

                
                Thread thread = new Thread(() =>
                {
                    DafnyDriver.Main(finalArgs);
                });

                var sw = new StringWriter();
                var oldOut = Console.Out;
                var oldErr = Console.Error;

                Console.SetOut(sw);
                Console.SetError(sw);

                thread.Start();
                thread.Join();

                Console.SetOut(oldOut);
                Console.SetError(oldErr);

                string threadOutput = sw.ToString();
                threadOutput = threadOutput.Replace("\r", "");

                if (threadOutput.Contains("Compiled assembly into") && threadOutput.Contains(".exe"))
                {
                    return new CompilerResults
                    {
                        Error = false,
                        Message = "Compilation successful",
                        Executable = true
                    };
                }
                else if (threadOutput.Contains("Compiled assembly into"))
                {
                    return new CompilerResults
                    {
                        Error = false,
                        Message = "Compilation successful",
                        Executable = false
                    };
                }
                else
                {
                    const string pattern = "\\((\\d+),(\\d+)\\): Error:? (.*)\n";
                    Match m = Regex.Match(threadOutput, pattern);

                    int.TryParse(m.Groups[1].Value, out int line);
                    int.TryParse(m.Groups[2].Value, out int col);
                    string error = m.Groups[3].ToString();

                    return new CompilerResults
                    {
                        Error = true,
                        Message = $"Compilation failed: {error} in line {line}.",
                        Executable = false
                    };
                }
            });
        }

    }

}
