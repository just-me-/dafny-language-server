﻿using DafnyLanguageServer.FileManager;
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
    /// <summary>
    ///  This service is used by the <c>CompileHandler</c>. 
    /// </summary>
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
                if (!File.Exists(DfyFile))
                {
                    return new CompilerResults
                    {
                        Error = true,
                        Message = "Compilation failed: Dafny Source File does not exist",
                        Executable = false
                    };
                }

                if (Path.GetExtension(DfyFile) != ".dfy")
                {
                    return new CompilerResults
                    {
                        Error = true,
                        Message = "Compilation failed: Can only compile .dfy files",
                        Executable = false
                    };
                }
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

                Console.WriteLine(threadOutput);
                if (threadOutput == " in line 0.")
                {
                    threadOutput = "Could not parse document. Please check the input and try again.";
                }

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

                    if (m.Success)
                    {
                        return new CompilerResults
                        {
                            Error = true,
                            Message = $"Compilation failed: {error} in line {line}.",
                            Executable = false
                        };
                    }
                    else
                    {
                        return new CompilerResults
                        {
                            Error = true,
                            Message = $"Compilation failed: {threadOutput}",
                            Executable = false
                        };
                    }
                }
            });
        }
    }
}
