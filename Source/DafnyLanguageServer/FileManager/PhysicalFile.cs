﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This class represents a physical Dafny file. The content of this file is not buffered.
    /// It is the current representation of the file in the users workspace in VSCode.
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
                var windowsCorrectionPattern = @"/\w:[/\\].*";  //finds /C:/xyz  and /C:\xyz
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

        /// <summary>
        /// Applies TextDocumentChanges to this instance's source code.
        /// </summary>
        /// <param name="change">The TextDocumentContentChangeEvent provided by the LSP</param>
        public void Apply(TextDocumentContentChangeEvent change)
        {
            Position startPos = change.Range.Start;
            int startIndex = GetIndex(startPos);
            string newSourceCut = Sourcecode.Remove(startIndex, change.RangeLength);
            string newSource = newSourceCut.Insert(startIndex, change.Text);
            Sourcecode = newSource;
        }

        /// <summary>
        /// Gets the index of a certain position (position includes line + character).
        /// </summary>
        /// <param name="pos">The position you want the index for.</param>
        /// <returns>The index. It can be used for common string methods like replace(index) or insert(index).</returns>
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

        private bool ReachedTargetPosition(int currentLine, int targetLine, int currentCol, int targetCol)
        {
            return currentLine == targetLine && currentCol == targetCol;
        }

        /// <summary>
        /// Returns the length of a line.
        /// </summary>
        /// <param name="line">The line you want the length for.</param>
        /// <returns>Length of the line.</returns>
        public int GetLengthOfLine(int line)
        {
            string[] lines = Regex.Split(Sourcecode, "\r\n|\r|\n");
            if (line < 0)
            {
                throw new ArgumentException("Line-Index must not be negative"); // todo lang file #102
            }
            if (line >= lines.Length)
            {
                throw new ArgumentException($"There are not enough lines ({line}) in the given source!"); // todo lang file #102
            }
            return lines[line].Length;
        }
    }
}
