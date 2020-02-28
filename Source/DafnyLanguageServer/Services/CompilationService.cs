using DafnyLanguageServer.ContentManager;
using DafnyLanguageServer.Handler;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DafnyLanguageServer.Services
{
    public class CompilationService
    {
        private string PathToDafnyDotExe { get; }
        private string _pathToDfy;
        private string PathToDfy
        {
            get
            {
                return FileHelper.EscapeFilePath(_pathToDfy);
            }
            set
            {
                _pathToDfy = value;
            }
        }

        public CompilationService(string exe, string file)
        {
            PathToDafnyDotExe = exe;
            PathToDfy = file;
        }

        public async Task<CompilerResults> Compile()
        {
            return await Task.Run(() =>
            {

                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = PathToDafnyDotExe,
                        Arguments = "/compile:1 /nologo " + PathToDfy,
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
        public Process Process { get; private set; }
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
