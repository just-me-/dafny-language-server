using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;
using System.Text.RegularExpressions;

namespace DafnyLanguageServer.ContentManager
{
    public static class FileHelper
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

        public static string EscapeFilePath(string path)
        {
            string escapeCharacter = "\"";

            if (path.StartsWith(escapeCharacter) && path.EndsWith(escapeCharacter))
            {
                path = path.Substring(1, path.Length - 2);
            }

            if (path.Contains(escapeCharacter))
            {
                throw new NotSupportedException("Filename with Quote is not supported.");
            }

            if (path.Contains(" "))
            {
                return escapeCharacter + path + escapeCharacter;
            }
            else
            {
                return path;
            }
        }

        public static Position CreatePosition(long line, long chr)
        {
            if (line < 0 || chr < 0)
            {
                throw new ArgumentException("Negative position values are not supported");
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

        public static int GetLineLength(string source, int line)
        {
            string[] lines = Regex.Split(source, "\r\n|\r|\n");
            if (line < 0)
            {
                throw new ArgumentException("Line-Index must not be negative");
            }
            if (line >= lines.Length)
            {
                throw new ArgumentException($"There are not enough lines ({line}) in the given source!");
            }
            return lines[line].Length;
        }
    }
}
