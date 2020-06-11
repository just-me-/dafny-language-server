using System;

namespace DafnyLanguageServer.SymbolTable
{
    /// <summary>
    /// Isolated wrapping logic for ISymbolInformation.
    /// </summary>
    public static class SymbolUtil
    {
        /// <summary>
        /// Checks whether a position is within the identifier of a symbol.
        /// If the position is within the symbol's body, this method will return false.
        /// </summary>
        public static bool PositionIsWithinSymbolIdentifier(ISymbolInformation s, Uri file, int line, int character)
        {
            if (!IsSameFile(s, file))
            {
                return false;
            }

            int startLine = s.Line;
            int startCol = s.Column;

            int endLine = s.Line;
            int endCol = s.Column + s.Name.Length;

            int targetLine = line;
            int targetCol = character;

            return Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
        }

        /// <summary>
        /// Checks whether a position is within the full range of a symbol.
        /// The full range starts at the identifier and stops at the body end.
        /// If there is no body, the identifier end counts as the full range.
        /// </summary>
        public static bool PositionIsWithinSymbolTotalRange(ISymbolInformation s, Uri file, int line, int character)
        {
            if (!IsSameFile(s, file))
            {
                return false;
            }

            int startLine = s.Line;
            int startCol = s.Column;

            int endLine;
            int endCol;

            if (s.HasBody)
            {
                endLine = s.Position.BodyEndToken.line;
                endCol = s.Position.BodyEndToken.col;
            }
            else
            {
                endLine = s.Line;
                endCol = s.Column + s.Name.Length;
            }

            int targetLine = line;
            int targetCol = character;

            return Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
        }

        /// <summary>
        /// Checks whether a position is within the body of a symbol.
        /// If the symbol has no body, false is returned.
        /// </summary>
        public static bool PositionIsWithinSymbolBody(ISymbolInformation s, Uri file, int line, int character)
        {
            if (!IsSameFile(s, file))
            {
                return false;
            }

            if (!s.HasBody)
            {
                return false;
            }

            int startLine = s.Position.BodyStartToken.line;
            int startCol = s.Position.BodyStartToken.col;

            int endLine = s.Position.BodyEndToken.line;
            int endCol = s.Position.BodyEndToken.col;

            int targetLine = line;
            int targetCol = character;

            return Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
        }

        /// <summary>
        /// Checks if a targetPosition is within a start- and end-position
        /// Each Position contains of a line and a character.
        /// ints are used, although one may upgrade to a Positon Class later.
        /// </summary>
        /// <remarks>
        /// This method is written for clarity and correctness.
        /// It could be shortened, but to avoid bugs this is not done.
        /// The compiler will optimzie it internally.
        /// </remarks>
        public static bool Wraps(int startLine, int startCol,
            int endLine, int endCol,
            int targetLine, int targetCol)
        {
            if ((startLine > endLine) ||
                (startLine == endLine && startCol > endCol) ||
                (startLine < 0 || endLine < 0 || startCol < 0 || endCol < 0))
            {
                return false;
            }

            if (LineIsWrapped(startLine, endLine, targetLine))
            {
                return true;
            }

            if (OnlySameStartLine(startLine, endLine, targetLine))
            {
                return (startCol <= targetCol);
            }

            if (OnlySameEndLine(startLine, endLine, targetLine))
            {
                return (targetCol <= endCol);
            }

            if (OnSameLine(startLine, endLine, targetLine))
            {
                return (startCol <= targetCol && targetCol <= endCol);
            }

            return false;
        }

        private static bool LineIsWrapped(int startLine, int endLine, int targetLine)
        {
            return startLine < targetLine && targetLine < endLine;
        }

        private static bool OnlySameStartLine(int startLine, int endLine, int targetLine)
        {
            return startLine == targetLine && targetLine < endLine;
        }

        private static bool OnlySameEndLine(int startLine, int endLine, int targetLine)
        {
            return startLine < targetLine && targetLine == endLine;
        }

        private static bool OnSameLine(int startLine, int endLine, int targetLine)
        {
            return startLine == targetLine && targetLine == endLine;
        }

        /// <summary>
        /// Checks if a is in the same file as the provided URI.
        /// If a is the root note, this returns always true, since the root not surfaces every file.
        /// </summary>
        private static bool IsSameFile(ISymbolInformation a, Uri file)
        {
            return a.Kind == Kind.RootNode || a.FileUri == file;
        }
    }
}