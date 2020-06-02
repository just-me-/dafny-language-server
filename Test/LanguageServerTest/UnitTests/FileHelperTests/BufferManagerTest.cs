using System;
using DafnyLanguageServer.WorkspaceManager;
using NUnit.Framework;

namespace ContentManagerTests
{
    [TestFixture]
    [Category("Unit")]
    internal class BufferManagerTests
    {
        private IWorkspace b;
        readonly Uri uri1 = new Uri(@"C:\file1.txt");
        readonly Uri uri2 = new Uri(@"C:\file2.txt");
        readonly Uri unregisteredUri = new Uri(@"C:\file3.txt");

        readonly string source1 = @"
            method MultipleReturns(x: int, y: int) returns (more: int, less: int)
            {
                more:= x + y;
                less:= x - y;
                // comments: are not strictly necessary.
            }
            ";
        readonly string source2 = "";

        [SetUp]
        public void createBuffer()
        {
            b = new Workspace();
            b.UpdateFile(uri1, source1);
            b.UpdateFile(uri2, source2);
        }

        [Test]
        public void GetAllFilesRegular()
        {
            Assert.AreEqual(2, b.GetAllFiles().Count);
        }

        [Test]
        public void GetAllFilesEmpty()
        {
            IWorkspace t = new Workspace();
            Assert.IsTrue(t.GetAllFiles().IsEmpty);
        }

        [Test]
        public void GetFileRegular()
        {
            IFileRepository f = b.GetFileRepository(uri1);
            Assert.AreEqual(uri1, f.PhysicalFile.Uri);
            Assert.AreEqual(source1, f.PhysicalFile.Sourcecode);
        }

        [Test]
        public void GetFileInexistant()
        {
            IFileRepository f = b.GetFileRepository(unregisteredUri);
            Assert.AreEqual(unregisteredUri, f.PhysicalFile.Uri);
            Assert.AreEqual(string.Empty, f.PhysicalFile.Sourcecode);
        }

        [Test]
        public void GetFileByString()
        {
            IFileRepository f = b.GetFileRepository(uri1.ToString());
            Assert.AreEqual(uri1, f.PhysicalFile.Uri);
            Assert.AreEqual(source1, f.PhysicalFile.Sourcecode);
        }

        [Test]
        public void FilePathConversion()
        {
            IFileRepository f = b.GetFileRepository(uri1);
            Assert.AreEqual("C:\\file1.txt", f.PhysicalFile.Filepath);
        }


        [Test]
        public void GetSourceCode()
        {
            string s = b.GetFileRepository(uri1).PhysicalFile.Sourcecode;
            Assert.AreEqual(source1, s);
        }

        [Test]
        public void GetSourceCodeUnknownUri()
        {
            string s = b.GetFileRepository(unregisteredUri).PhysicalFile.Sourcecode;
            Assert.AreEqual(string.Empty, s);
        }
    }
}
