using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Handler;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Services
{
    public class CompilationService
    {
        private string PathToDafnyDotExe { get; }
        private string[] CompilationArgs { get; }
        private string _dfyFile;
        private string DfyFile
        {
            get => FileHelper.EscapeFilePath(_dfyFile);
            set => _dfyFile = value;
        }

        public CompilationService(string file, string[] args)
        {
            DfyFile = file;
            CompilationArgs = args;

            string assemblyPath = Path.GetDirectoryName(typeof(DafnyLanguageServer.Program).Assembly.Location);
            PathToDafnyDotExe = Path.GetFullPath(Path.Combine(assemblyPath, "Dafny.exe"));

        }

        public async Task<CompilerResults> Compile()
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(PathToDafnyDotExe))
                {
                    return new CompilerResults
                    {
                        Error = true,
                        Message = "Couldn't Locate Dafny.exe in " + PathToDafnyDotExe,
                        Executable = false
                    };
                }

                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = PathToDafnyDotExe,
                        Arguments = string.Join(" ", CompilationArgs) + " " + DfyFile,   //note: hier war compile:1 und /nolog. compile:1 ist aber eh ultradefault. no logo ka, sollte keinen ärger machen.
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    },
                    EnableRaisingEvents = true
                };

                string processOut;

                try
                {
                    processOut = new ProcessRunner(process).Run();
                }
                catch (Exception e)
                {
                    return new CompilerResults
                    {
                        Error = true,
                        Message = "Internal Server Error: " + e.Message,
                        Executable = false
                    };
                }

                if (processOut.Contains("Compiled assembly into") && processOut.Contains(".exe"))
                {
                    return new CompilerResults
                    {
                        Error = false,
                        Message = "Compilation successful",
                        Executable = true
                    };
                }
                else if (processOut.Contains("Compiled assembly into"))
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
                    Match m = Regex.Match(processOut, pattern);

                    int.TryParse(m.Groups[1].Value, out int line);
                    int.TryParse(m.Groups[2].Value, out int col);
                    string error = m.Groups[3].ToString();

                    return new CompilerResults
                    {
                        Error = true,
                        Message = $"Compilation failed: \"{error}\" in line {line}.",
                        Executable = false
                    };
                }
            });
        }

    }


    public class ProcessRunner
    {
        public Process Process { get; }
        public bool IsRunning { get; private set; } = false;
        public bool IsFinished { get; private set; } = false;

        public ProcessRunner(Process p)
        {
            Process = p;
        }

        public string Run()
        {
            string processOut = "";
            Process.OutputDataReceived += (sender, args) => processOut += args.Data + "\n";

            Process.Start();
            IsRunning = true;
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
            Process.WaitForExit();
            IsFinished = true;
            IsRunning = false;
            return processOut;

        }
    }

}
