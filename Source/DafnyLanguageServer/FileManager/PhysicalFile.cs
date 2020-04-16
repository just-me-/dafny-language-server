using DafnyLanguageServer.DafnyAccess;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This class represents a physical Dafny file. The content of this file is not buffered.
    /// It is de current representation of the file in the users workspace in VSCode.
    /// Gets updated whenever a change event over LSP is triggered.
    /// Only contains basic attributes like source code and name. 
    /// </summary>
    public class PhysicalFile
    {
        public Uri Uri { get; set; }
        private string _filepath;
        public string Filepath
        {
            //for some reason, Uri.LocalPath sometimes puts a "/" in front of windows paths.
            //we need to correct this here.
            get
            {
                var windowsCorrectionPattern = @"/\w:[/\\].*";  //finds /C:/blabla  and /C:\blabla
                var match = Regex.Match(_filepath, windowsCorrectionPattern);
                if (match.Success)
                {
                    return _filepath.Substring(1);
                }
                return _filepath;
            }
            set => _filepath = value;
        }

        public string FileName => Path.GetFileName(Filepath);

        public string Sourcecode { get; set; }
    }
}
