using System;
using DafnyLanguageServer.ContentManager;
using NUnit.Framework;

namespace ContentManagerTests
{
    [TestFixture]
    [Category("Unit")]
    internal class BufferManagerTests
    {
        private WorkspaceManager b;
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
            b = new WorkspaceManager();
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
            WorkspaceManager t = new WorkspaceManager();
            Assert.IsTrue(t.GetAllFiles().IsEmpty);
        }

        [Test]
        public void GetFileRegular()
        {
            FileRepository f = b.GetFileRepository(uri1);
            Assert.AreEqual(uri1, f.PhysicalFile.Uri);
            Assert.AreEqual(source1, f.PhysicalFile.Sourcecode);
        }

        [Test]
        public void GetFileInexistant()
        {
            FileRepository f = b.GetFileRepository(unregisteredUri);
            Assert.AreEqual(unregisteredUri, f.PhysicalFile.Uri);
            Assert.IsTrue(f.PhysicalFile.Sourcecode is null); //todo: sinnvoll?
        }

        [Test]
        public void GetFileByString()
        {
            FileRepository f = b.GetFileRepository(uri1.ToString());
            Assert.AreEqual(uri1, f.PhysicalFile.Uri);
            Assert.AreEqual(source1, f.PhysicalFile.Sourcecode);
        }

        [Test]
        public void FilePathConversion()
        {
            FileRepository f = b.GetFileRepository(uri1);
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
            Assert.AreEqual(null, s);
        }


        [Test]
        public void GetSymbolTableRegular()
        {
            FileSymboltableProcessor s = this.b.GetFileRepository(uri1).SymboleProcessor();
            Assert.IsTrue(s.HasEntries);
            var a = s.GetList();
            Assert.AreEqual("MultipleReturns", a[0].Name);
            Assert.AreEqual("Method", a[0].SymbolType.ToString());
        }

        [Test]
        public void GetSymbolTableEmpty()
        {
            FileSymboltableProcessor s2 = this.b.GetFileRepository(uri2).SymboleProcessor();
            Assert.IsTrue(s2.HasEntries);
            var a = s2.GetList();
            Assert.IsEmpty(a);
        }
    }
}
