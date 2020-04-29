using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Text.RegularExpressions;

namespace DafnyLanguageServer.FileManager
{
    /// <summary>
    /// This is a static class for general helper functions. This class will be refactored soon. 
    /// </summary>
    public static class FileHelper // 2do - was wird mit neuer symbol table überhaupt noch verwendet, was nur bei den tests... splitten #132
    {
        public static string GetCurrentWord(string code, int line, int character)
        {
            var selectedLine = SaveLineGetter(code, line, character);
            var match = Regex.Match(selectedLine, @"(\S+)\.$");
            return (match.Success) ? (match.Groups[1].Value) : null;
        }

        public static string GetFollowingWord(string code, int line, int character)
        {
            var selectedLine = SaveLineGetter(code, line, character, false);
            var match = Regex.Match(selectedLine, @"^([a-zA-Z0-9-_]+).*");
            return (match.Success) ? (match.Groups[1].Value) : null;
        }

        private static string SaveLineGetter(string code, int line, int character, bool front = true)
        {
            var codeLines = Regex.Split(code, "\r\n|\r|\n");
            // avoid out of bounds exceptions 
            return (codeLines.Length >= line && codeLines[line].Length >= character)
                ?
                    (front
                    ? codeLines[line].Substring(0, character)
                    : codeLines[line].Substring(character))
                : "";
        }

        // moved to smboltablemaneger ... can be deleted if autocompletion is done. move tests
        public static bool ChildIsContainedByParent(Range childRange, Range parentRange)
        {
            return (
                (childRange != null && parentRange != null)
                && (
                    (childRange.Start.Line >= parentRange.Start.Line
                     && childRange.End.Line <= parentRange.End.Line
                     && parentRange.Start.Line != parentRange.End.Line)
                    ||
                    // if it is an one liner - check position 
                    (parentRange.Start.Line == parentRange.End.Line
                     && childRange.Start.Line == childRange.End.Line
                     && parentRange.Start.Line == childRange.Start.Line
                     && childRange.Start.Character >= parentRange.Start.Character
                     && childRange.End.Character <= parentRange.End.Character)
                )
            );
        }


        public static Position CreatePosition(long line, long chr)
        {
            if (line < 0 || chr < 0)
            {
                throw new ArgumentException(Resources.ExceptionMessages.negative_position);
            }

            return new Position
            {
                Line = line,
                Character = chr
            };
        }

        public static Range CreateRange(long lineStart, long lineEnd, long chrStart, long chrEnd)
        {
            Position start = CreatePosition(lineStart, chrStart);
            Position end = CreatePosition(lineEnd, chrEnd);
            return CreateRange(start, end);
        }

        public static Range CreateRange(long line, long chr, long length)
        {
            if (length < 0)
            {
                length = Math.Abs(length);
                chr -= length;
            }

            Position start = CreatePosition(line, chr);
            Position end = CreatePosition(line, chr + length);
            return CreateRange(start, end);
        }

        public static Range CreateRange(Position start, Position end)
        {
            return new Range(start, end);
        }
    }
}
