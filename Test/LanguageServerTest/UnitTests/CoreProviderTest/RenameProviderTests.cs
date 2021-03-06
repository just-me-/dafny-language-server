﻿using DafnyLanguageServer.Core;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;
using System;
using System.Linq;

namespace CoreProviderTest
{
    [TestFixture]
    [Category("Unit")]
    public class RenameProviderTests
    {
        [Test]
        public void TestAssembling()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new RenameProvider(manager);

            //note: params do not matter. the fake returns a fixed symbol and does not respect the position. those methods are tested within the symbol table tests.
            var result = provider.GetRenameChanges("oink", new Uri("file:///N:/u/l.l"), 2, 22);

            Assert.AreEqual("file:///N:/u/l.l", result.Changes.Keys.First().ToString(),
                "The fake instance is not assigning URIs. It should be the default-null uri.");

            var changes = result.Changes.Values.First();
            var aslist = changes.ToList();
            aslist.Sort(
                (te1, te2) => (int)te1.Range.Start.Line - (int)te2.Range.Start.Line
            );
            foreach (var c in aslist)
            {
                Assert.AreEqual("oink", c.NewText, "New Text was not as expected");
                Assert.AreEqual(8, c.Range.End.Character - c.Range.Start.Character, "The Range was not as expected");
                Assert.AreEqual(0, c.Range.End.Line - c.Range.Start.Line, "Edit is not on the same line");
            }

            Assert.AreEqual(0, aslist[0].Range.Start.Line, "Edit 0 starts at wrong line");
            Assert.AreEqual(10, aslist[0].Range.Start.Character, "Edit 0 starts at wrong char");

            Assert.AreEqual(1, aslist[1].Range.Start.Line, "Edit 1 starts at wrong line");
            Assert.AreEqual(21, aslist[1].Range.Start.Character, "Edit 1 starts at wrong char");

            Assert.AreEqual(2, aslist[2].Range.Start.Line, "Edit 2 starts at wrong line");
            Assert.AreEqual(32, aslist[2].Range.Start.Character, "Edit 2 starts at wrong char");
        }

        [Test]
        public void TestOutcome()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new RenameProvider(manager);
            var result = provider.GetRenameChanges("oink", new Uri("file:///N:/u/l.l"), 2, 22);

            Assert.IsFalse(provider.Outcome.Error, "no error expected in rename-outcome");
        }

        [Test]
        public void IrregularName()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new RenameProvider(manager);
            var result = provider.GetRenameChanges("{{}}}", new Uri("file:///N:/u/l.l"), 2, 22);

            Assert.IsTrue(provider.Outcome.Error, "error expected in rename-outcome");
            Assert.AreEqual("{{}}}" + DafnyLanguageServer.Resources.LoggingMessages.rename_only_digits, provider.Outcome.Msg);
        }

        [Test]
        public void StartWithNumber()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new RenameProvider(manager);
            var result = provider.GetRenameChanges("1abc", new Uri("file:///N:/u/l.l"), 2, 22);

            Assert.IsTrue(provider.Outcome.Error, "error expected in rename-outcome");
            Assert.AreEqual(DafnyLanguageServer.Resources.LoggingMessages.rename_start_with_number, provider.Outcome.Msg);
        }

        [Test]
        public void UnderScoreName()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new RenameProvider(manager);
            var result = provider.GetRenameChanges("_abc", new Uri("file:///N:/u/l.l"), 2, 22);

            Assert.IsTrue(provider.Outcome.Error, "error expected in rename-outcome");
            Assert.AreEqual(DafnyLanguageServer.Resources.LoggingMessages.rename_start_with_underscore, provider.Outcome.Msg);
        }

        [Test]
        public void ReservedWord()
        {
            ISymbolTableManager manager = new FakeSymbolManager(false, false);
            var provider = new RenameProvider(manager);
            var result = provider.GetRenameChanges("method", new Uri("file:///N:/u/l.l"), 2, 22);

            Assert.IsTrue(provider.Outcome.Error, "error expected in rename-outcome");
            Assert.AreEqual("method" + DafnyLanguageServer.Resources.LoggingMessages.rename_reserved_word, provider.Outcome.Msg);
        }
    }
}