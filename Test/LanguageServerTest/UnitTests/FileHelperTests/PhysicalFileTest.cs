using DafnyLanguageServer.Commons;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

namespace ContentManagerTest
{
    [TestFixture]
    [Category("Unit")]
    public class PhysicalFileTest
    {
        [Test]
        public void SimpleTest1()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            int result = f.GetLengthOfLine(1);

            Assert.AreEqual(3, result);
        }

        [Test]
        public void SimpleTest2()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            int result = f.GetLengthOfLine(0);

            Assert.AreEqual(1, result);
        }

        [Test]
        public void LineTooLarge()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            Assert.Throws<ArgumentException>(() => f.GetLengthOfLine(3));
        }

        [Test]
        public void LineNegativeThrows()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            Assert.Throws<ArgumentException>(() => f.GetLengthOfLine(-1));
        }

        [Test]
        public void GetIndexFirstLine()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "X\nabc\na"
            };
            Position p = new Position(0, 0);
            int actual = f.GetIndex(p);
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void GetIndexFirstLine2()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "aX\nabc\na"
            };
            Position p = new Position(0, 1);
            int actual = f.GetIndex(p);
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void GetNewLinesAsTarget_n_hitn()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };
            Position p = new Position(0, 1);
            int actual = f.GetIndex(p);
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void GetNewLinesAsTarget_rn_hitr()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\r\nabc\na"
            };
            Position p = new Position(0, 1);
            int actual = f.GetIndex(p);
            Assert.AreEqual(1, actual);
        }

        [Test]
        public void GetNewLinesAsTarget_rn_hitn()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\r\nabc\na"
            };
            Position p = new Position(0, 2);
            int actual = f.GetIndex(p);
            Assert.AreEqual(2, actual);
        }

        [Test]
        public void GetIndexNormal()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\naXc\na"
            };
            Position p = new Position(1, 1);
            int actual = f.GetIndex(p);
            Assert.AreEqual(3, actual);
        }

        [Test]
        public void GetIndexNormalSlashR()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\r\naXc\na"
            };
            Position p = new Position(1, 1);
            int actual = f.GetIndex(p);
            Assert.AreEqual(4, actual);
        }

        [Test]
        public void GetIndexOutOfBounds()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };
            Position p = new Position(2, 2);
            Assert.Throws<IndexOutOfRangeException>(() => f.GetIndex(p));
        }

        [Test]
        public void GetLineContentNormal()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };
            var actual = f.GetSourceLine(1);
            Assert.AreEqual("abc", actual);
        }

        [Test]
        public void GetLineContentNeg()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };
            Assert.Throws<IndexOutOfRangeException>(() => f.GetSourceLine(-1));
        }

        [Test]
        public void GetLineContentOutOfBounds()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };
            Assert.Throws<IndexOutOfRangeException>(() => f.GetSourceLine(3));
        }

        [Test]
        public void GetLineContentWithSlashR()
        {
            var f = new PhysicalFile()
            {
                Sourcecode = "a\r\nabc\r\na"
            };
            var actual = f.GetSourceLine(1);
            Assert.AreEqual("abc", actual);
        }
    }
}