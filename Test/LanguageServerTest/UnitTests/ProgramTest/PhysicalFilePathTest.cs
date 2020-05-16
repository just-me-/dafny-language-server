using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.Commons;
using NUnit.Framework;

namespace ProgramTest
{
    class PhysicalFilePathTest
    {
        private const string expct = @"C:/bla.txt";
        [Test]
        public void NormalWindowsPath()
        {
            string path = @"C:/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"C:/bla.txt", pf.Filepath);
        }

        [Test]
        public void NormalWindowsPathBackslashed()
        {
            string path = @"C:\bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"C:\bla.txt", pf.Filepath);
        }

        [Test]
        public void NormalWindowsPathSmallLetter()
        {
            string path = @"c:/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"c:/bla.txt", pf.Filepath);
        }

        [Test]
        public void NormalWindowsPathBackslashedSmallLetter()
        {
            string path = @"c:\bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"c:\bla.txt", pf.Filepath);
        }

        [Test]
        public void BuggedWindowsPath()
        {
            string path = @"/C:/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"C:/bla.txt", pf.Filepath);
        }

        [Test]
        public void BuggedWindowsPathBackslashed()
        {
            string path = @"/C:\bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"C:\bla.txt", pf.Filepath);
        }

        [Test]
        public void BuggedWindowsPathSmall()
        {
            string path = @"/c:/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"c:/bla.txt", pf.Filepath);
        }

        [Test]
        public void BuggedWindowsPathBackslashedSmall()
        {
            string path = @"/c:\bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"c:\bla.txt", pf.Filepath);
        }

        [Test]
        public void LinuxAbsolute()
        {
            string path = @"/builds/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"/builds/bla.txt", pf.Filepath);
        }

        [Test]
        public void Relative()
        {
            string path = @"builds/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"builds/bla.txt", pf.Filepath);
        }
        [Test]
        public void FileName()
        {
            string path = @"C:/bla.txt";
            var pf = new PhysicalFile()
            {
                Filepath = path
            };

            Assert.AreEqual(@"bla.txt", pf.FileName);
        }
    }
}
