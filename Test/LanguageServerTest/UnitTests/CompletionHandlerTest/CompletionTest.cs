using DafnyLanguageServer.FileManager;
using NUnit.Framework;

namespace CompletionHandlerTest
{
    [TestFixture]
    [Category("Unit")]
    public class CompletionTests
    {
        /*private FileSymboltableProcessor symbolTable = new FileSymboltableProcessor(new DafnyTranslationUnitFakeForCompletions().Symbols());

        [Test]
        public void FullList()
        {
            var list = symbolTable.GetFullList();
            Assert.AreEqual(7, list.Count);
        }

        [Test]
        public void ListWithoutDuplicates()
        {
            var list = symbolTable.GetList();
            Assert.AreEqual(6, list.Count);
        }

        [Test]
        public void ListForIdentifier()
        {
            var list = symbolTable.GetList("ClassA");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("aFunctionInClassA", list[0].Name);
        }

        [Test]
        public void GetParentForWord()
        {
            var parent = symbolTable.GetParentForWord("aFunctionInClassA");
            Assert.AreEqual("ClassA", parent);
        }

        [Test]
        public void GetParentForWordNull()
        {
            var parent = symbolTable.GetParentForWord(null);
            Assert.AreEqual(null, parent);
        }

        [Test]
        public void GetParentForWordNotExist()
        {
            var parent = symbolTable.GetParentForWord("ThisDoesNotExist95");
            Assert.AreEqual(null, parent);
        }
        */
    }
}