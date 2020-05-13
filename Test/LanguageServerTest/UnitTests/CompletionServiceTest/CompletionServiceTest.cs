using System;
using DafnyLanguageServer.HandlerServices;
using NUnit.Framework;

namespace CompletionServiceTest
{
    [TestFixture]
    [Category("Unit")]
    public class CompletionServiceTest
    {
        private readonly CompletionService _service = new CompletionService(null);

        [Test]
        public void Desire_SimpleAfterDot()
        {
            var line = "myObject.";
            var desire = _service.GetSupposedDesire(line, 10);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("myObject", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_ComplexAfterDot()
        {
            var line = "no myObject. ohw";
            var desire = _service.GetSupposedDesire(line, 13);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("myObject", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_MultipleDotsAfterDot()
        {
            var line = "no. myObject. ohw.";
            var desire = _service.GetSupposedDesire(line, 14);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("myObject", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_AfterDot_ComplexWord()
        {
            var line = "wrap mySub3Var. wrap";
            var desire = _service.GetSupposedDesire(line, 16);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("mySub3Var", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_AfterDot_ComplexSubWord()
        {
            var line = "wrap myObject.mySub3Var. wrap";
            var desire = _service.GetSupposedDesire(line, 25);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("mySub3Var", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_AfterDot_ComplexSpecialCharWord()
        {
            var line = "wrap my_Sub3Var. wrap";
            var desire = _service.GetSupposedDesire(line, 17);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("my_Sub3Var", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_AfterDot_MinusWord()
        {
            var line = "wrap my_Sub3-Var. wrap";
            var desire = _service.GetSupposedDesire(line, 18);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("Var", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_AfterDot_PlusWord()
        {
            var line = "wrap my_Sub3+Var. wrap";
            var desire = _service.GetSupposedDesire(line, 18);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("Var", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_AfterDot_ComplexUmlauts()
        {
            var line = "wrap heÖöl. wrap";
            var desire = _service.GetSupposedDesire(line, 12);
            Assert.AreEqual(CompletionType.AfterDot, desire);
            Assert.AreEqual("heÖöl", _service.ExtractedSymbol);
        }

        [Test]
        public void Desire_UnderflowSpot()
        {
            var line = "ohno";
            Assert.Throws<ArgumentException>(() => _service.GetSupposedDesire(line, -1));
        }

        [Test]
        public void Desire_OverflowSpot()
        {
            var line = "ohno";
            Assert.Throws<ArgumentException>(() => _service.GetSupposedDesire(line, 10));
        }

        [Test]
        public void Desire_ZeroSpot()
        {
            var line = "ohno";
            Assert.Throws<ArgumentException>(() => _service.GetSupposedDesire(line, 0));
        }

        [Test]
        public void Desire_StartAfterNew()
        {
            var line = "new  ";
            var desire = _service.GetSupposedDesire(line, 4);
            Assert.AreEqual(CompletionType.AllInScope, desire);

        }

        [Test]
        public void Desire_StartAfterNewSpace()
        {
            var line = "new  ";
            var desire = _service.GetSupposedDesire(line, 5);
            Assert.AreEqual(CompletionType.AfterNew, desire);
        }

        [Test]
        public void Desire_StartAfterNewJustSpace()
        {
            var line = "new   ";
            var desire = _service.GetSupposedDesire(line, 6);
            Assert.AreEqual(CompletionType.AfterNew, desire);
        }

        [Test]
        public void Desire_IntoWordAfterNew()
        {
            var line = "new";
            var desire = _service.GetSupposedDesire(line, 2);
            Assert.AreEqual(CompletionType.AllInScope, desire);
        }

        [Test]
        public void Desire_ComplexAfterNew()
        {
            var line = "hi new ewn";
            var desire = _service.GetSupposedDesire(line, 8);
            Assert.AreEqual(CompletionType.AfterNew, desire);
        }

        [Test]
        public void Desire_SimpleStartAllInScope()
        {
            var line = "myWord";
            var desire = _service.GetSupposedDesire(line, 1);
            Assert.AreEqual(CompletionType.AllInScope, desire);
        }

        [Test]
        public void Desire_SimpleMiddleAllInScope()
        {
            var line = "myWord";
            var desire = _service.GetSupposedDesire(line, 3);
            Assert.AreEqual(CompletionType.AllInScope, desire);
        }

        [Test]
        public void Desire_SimpleEndAllInScope()
        {
            var line = "myWord";
            var desire = _service.GetSupposedDesire(line, 7);
            Assert.AreEqual(CompletionType.AllInScope, desire);
        }

        [Test]
        public void Desire_SimpleBlankAllInScope()
        {
            var line = " ";
            var desire = _service.GetSupposedDesire(line, 2);
            Assert.AreEqual(CompletionType.AllInScope, desire);
        }
    }
}