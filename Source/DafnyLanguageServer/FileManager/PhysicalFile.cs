using DafnyLanguageServer.DafnyAccess;
using System;
using System.Data.OleDb;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

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
        
        public void Apply(TextDocumentContentChangeEvent change)
        {
            Position startPos = change.Range.Start;
            int startIndex = GetIndex(startPos);
            string newSourceCut = Sourcecode.Remove(startIndex, change.RangeLength);
            string newSource = newSourceCut.Insert(startIndex, change.Text);
            Sourcecode = newSource;
        }

        public int GetIndex(Position pos)
        {
            int result = 0;

            //using 1-based indexes, thus +1 everywhere and starting from 1.
            int targetLine = (int)pos.Line + 1;
            int targetCol = (int)pos.Character + 1;
            int currentLine = 1;
            int currentCol = 1;

            while (!ReachedTargetPosition(currentLine, targetLine, currentCol, targetCol))
            {
                if (Sourcecode[result] == '\n')
                {
                    currentLine++;
                    currentCol = 1;
                }
                else
                {
                    currentCol++;
                }
                result++;
            }

            return result;
        }

        private static bool ReachedTargetPosition(int currentLine, int targetLine, int currentCol, int targetCol)
        {
            return (currentLine == targetLine && currentCol == targetCol);
        }
    }
}
