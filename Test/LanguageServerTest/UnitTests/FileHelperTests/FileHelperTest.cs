using System;
using DafnyLanguageServer.FileManager;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ContentManagerTests
{
    internal class ParentChildTests
    {
        [Test]
        public void IsInRange()
        {
            Range childRange = FileHelper.CreateRange( 1, 1, 0, 0);
            Range parentRange = FileHelper.CreateRange(0, 2, 0, 0);
            Assert.IsTrue(FileHelper.ChildIsContainedByParent(childRange, parentRange));
        }

        [Test]
        public void IsNotInRange()
        {
            Range childRange = FileHelper.CreateRange(0, 0, 0, 0);
            Range parentRange = FileHelper.CreateRange(1, 1, 0, 0);
            Assert.IsFalse(FileHelper.ChildIsContainedByParent(childRange, parentRange));
        }

        [Test]
        public void IsInRange_SameLine()
        {
            Range childRange = FileHelper.CreateRange(0, 0, 2, 3);
            Range parentRange = FileHelper.CreateRange(0, 0, 1, 4);
            Assert.IsTrue(FileHelper.ChildIsContainedByParent(childRange, parentRange));
        }

        [Test]
        public void IsNotInRange_SameLine()
        {
            Range childRange = FileHelper.CreateRange(0, 0, 2, 0);
            Range parentRange = FileHelper.CreateRange(0, 0, 3, 4);
            Assert.IsFalse(FileHelper.ChildIsContainedByParent(childRange, parentRange));
        }

        [Test]
        public void IsNotInRange_ChildNull()
        {
            Range range = FileHelper.CreateRange(0, 0, 0, 0);
            Assert.IsFalse(FileHelper.ChildIsContainedByParent(null, range));
        }

        [Test]
        public void IsNotInRange_ParentNull()
        {
            Range range = FileHelper.CreateRange(0, 0, 0, 0);
            Assert.IsFalse(FileHelper.ChildIsContainedByParent(range, null));
        }

        [Test]
        public void IsNotInRange_BothNull()
        {
            Assert.IsFalse(FileHelper.ChildIsContainedByParent(null, null));
        }
    }

    internal class WordGetterTests
    {
        [Test]
        public void GetWord()
        {
            var code = @"method Main() {
                        var a := 1 + 2;
                        var acc2 := new C();
                        var acc3.             
            }";
            var acc3 = FileHelper.GetCurrentWord(code, 4 - 1, 33);
            Assert.AreEqual("acc3", acc3);
        }

        [Test]
        public void GetWordOfSimpleLine()
        {
            var code = "myWord.";
            var myWord = FileHelper.GetCurrentWord(code, 0, 7);
            Assert.AreEqual("myWord", myWord);
        }

        [Test]
        public void GetWordOfOutOfBounce()
        {
            var code = "hi";
            var myWord = FileHelper.GetCurrentWord(code, 0, 50);
            Assert.AreEqual(null, myWord);
        }

        [Test]
        public void GetFollowingWord()
        {
            var code = "hello world";
            var myWord = FileHelper.GetFollowingWord(code, 0, 6);
            Assert.AreEqual("world", myWord);
        }

        [Test]
        public void GetFollowingWordOutOfBounce()
        {
            var code = "hello";
            var myWord = FileHelper.GetFollowingWord(code, 0, 50);
            Assert.AreEqual(null, myWord);
        }
    }

    internal class PositionalTests
    {
        [Test]
        public void GetAValidPosition1()
        {
            const int line = 5;
            const int chr1 = 10;

            var pos = FileHelper.CreatePosition(line, chr1);
            Assert.AreEqual(new Position(line, chr1), pos);
        }

        [Test]
        public void GetInvalidPosition1()
        {
            const int line = -5;
            const int chr = 10;

            Assert.Throws<ArgumentException>(() => FileHelper.CreatePosition(line, chr));
        }

        [Test]
        public void GetInvalidPosition2()
        {
            const int line = 5;
            const int chr = -10;

            Assert.Throws<ArgumentException>(() => FileHelper.CreatePosition(line, chr));
        }

        [Test]
        public void GetAValidRange()
        {
            const int line = 5;
            const int chr1 = 10;
            const int chr2 = 15;
            const int len = chr2 - chr1;

            var range = FileHelper.CreateRange(line, chr1, len);
            Assert.AreEqual(new Position(line, chr1), range.Start);
            Assert.AreEqual(new Position(line, chr2), range.End);
        }

        [Test]
        public void LowerPositionIsFirstAkaNegativeRange()
        {
            const int line = 5;
            const int chr1 = 10;
            const int chr2 = 5;
            const int len = chr2 - chr1;

            var range = FileHelper.CreateRange(line, chr1, len);
            Assert.AreEqual(new Position(line, chr2), range.Start);
            Assert.AreEqual(new Position(line, chr1), range.End);
        }

        [Test]
        public void GetAnInvalidRange1()
        {
            const int line = -2;
            const int chr1 = 10;
            const int chr2 = 15;
            const int len = chr2 - chr1;

            Assert.Throws<ArgumentException>(() => FileHelper.CreateRange(line, chr1, len));
        }

        [Test]
        public void GetAnInvalidRange2()
        {
            const int line = 5;
            const int chr1 = -10;
            const int chr2 = 15;
            const int len = chr2 - chr1;

            Assert.Throws<ArgumentException>(() => FileHelper.CreateRange(line, chr1, len));
        }
    }

    internal class LineLengthTests
    {
        [Test]
        public void SimpleTest1()
        {
            const string s = "a\nabc\na";
            const int l = 1;

            int result = PhysicalFile.GetLineLength(s, l);

            Assert.AreEqual(3, result);
        }

        [Test]
        public void SimpleTest2()
        {
            const string s = "a\nabc\na";
            const int l = 0;

            int result = PhysicalFile.GetLineLength(s, l);

            Assert.AreEqual(1, result);
        }

        [Test]
        public void LineTooLarge()
        {
            const string s = "a\nabc\na";
            const int l = 3;

            Assert.Throws<ArgumentException>(() => PhysicalFile.GetLineLength(s, l));
        }

        [Test]
        public void LineNegativeThrows()
        {
            const string s = "a\nabc\na";
            const int l = -1;

            Assert.Throws<ArgumentException>(() => PhysicalFile.GetLineLength(s, l));
        }
    }

    internal class PathEscapeTests
    {
        [Test]
        public void SimplePath()
        {
            const string input = @"C:\a.dfy";
            const string expected = input;

            var output = FileHelper.EscapeFilePath(input);

            Assert.AreEqual(expected, output);
        }


        [Test]
        public void SpacedPath()
        {
            const string input = @"C:\some folder\a.dfy";
            const string expected = "\"" + input + "\"";

            var output = FileHelper.EscapeFilePath(input);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void MultipleSpacedPath()
        {
            const string input = @"C:\some folder\here is my file.dfy";
            const string expected = "\"" + input + "\"";

            var output = FileHelper.EscapeFilePath(input);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void PreEscapedPath()
        {
            const string input = @"""C:\some folder\a.dfy""";
            const string expected = input;

            var output = FileHelper.EscapeFilePath(input);

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void PathWithQuoteWithSpaceThrows()
        {
            const string input = @"C:\some fol""der\a.dfy";
            Assert.Throws<NotSupportedException>(() => FileHelper.EscapeFilePath(input));
        }

        [Test]
        public void PathWithQuoteWithoutSpaceThrows()
        {
            const string input = @"C:\ba""ba\a.dfy";
            Assert.Throws<NotSupportedException>(() => FileHelper.EscapeFilePath(input));
        }
    }
}