using System;
using DafnyLanguageServer.ContentManager;
using NUnit.Framework;

namespace ContentManagerTests
{
    [TestFixture]
    [Category("Unit")]
    internal class BufferManagerTests
    {
        private BufferManager b;
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
            b = new BufferManager();
            b.UpdateBuffer(uri1, source1);
            b.UpdateBuffer(uri2, source2);
        }

        [Test]
        public void GetAllFilesRegular()
        {
            Assert.AreEqual(2, b.GetAllFiles().Count);
        }

        [Test]
        public void GetAllFilesEmpty()
        {
            BufferManager t = new BufferManager();
            Assert.IsTrue(t.GetAllFiles().IsEmpty);
        }

        [Test]
        public void GetFileRegular()
        {
            DafnyFile f = b.GetFile(uri1);
            Assert.AreEqual(uri1, f.Uri);
            Assert.AreEqual(source1, f.Sourcecode);
        }

        [Test]
        public void GetFileInexistant()
        {
            DafnyFile f = b.GetFile(unregisteredUri);
            Assert.AreEqual(unregisteredUri, f.Uri);
            Assert.IsTrue(f.Sourcecode is null); //todo: sinnvoll?
        }

        [Test]
        public void GetFileByString()
        {
            DafnyFile f = b.GetFile(uri1.ToString());
            Assert.AreEqual(uri1, f.Uri);
            Assert.AreEqual(source1, f.Sourcecode);
        }

        [Test]
        public void FilePathConversion()
        {
            DafnyFile f = b.GetFile(uri1);
            Assert.AreEqual(uri1.LocalPath, f.Filepath);  //this seems not really right, in windows i get /G:/blabla.txt  (leading slash).
        }


        [Test]
        public void GetSourceCode()
        {
            string s = b.GetSourceCodeAsText(uri1);
            Assert.AreEqual(source1, s);
        }

        [Test]
        public void GetSourceCodeUnknownUri()
        {
            string s = b.GetSourceCodeAsText(unregisteredUri);
            Assert.AreEqual(null, s);
        }


        [Test]
        public void GetSymbolTableRegular()
        {
            FileSymboltable s = this.b.GetSymboltable(uri1);
            Assert.IsTrue(s.HasEntries);
            var a = s.GetList();
            Assert.AreEqual("MultipleReturns", a[0].Name);
            Assert.AreEqual("Method", a[0].SymbolType.ToString());
        }

        [Test]
        public void GetSymbolTableEmpty()
        {
            FileSymboltable s2 = this.b.GetSymboltable(uri2);
            Assert.IsTrue(s2.HasEntries);
            var a = s2.GetList();
            Assert.IsEmpty(a);
        }
    }
}
