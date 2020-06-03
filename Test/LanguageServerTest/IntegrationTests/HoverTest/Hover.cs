using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TestCommons;
using Files = TestCommons.Paths;

namespace HoverTest
{
    public class Tests : IntegrationTestBase
    {

        public Tests() : base("Hover")
        {
        }

        [Test]
        public void LocalVariableUsage()
        {
            Client.TextDocument.DidOpen(Files.hv_hover, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_hover, 6, 14).Result;
            var actual = result.Contents.MarkedStrings.Select(x => x.Value).ToList();

            List<string> expected = new List<string>()
            {
                "Symbol: r at Line 7 in hover.dfy",
                "Kind: Variable",
                "Type: int",
                "Scope: addOne",
                "Declaration: r at Line 5 in hover.dfy"
            };
            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public void ClassDeclaration()
        {
            Client.TextDocument.DidOpen(Files.hv_hover, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_hover, 0, 6).Result;
            var actual = result.Contents.MarkedStrings.Select(x => x.Value).ToList();

            List<string> expected = new List<string>()
            {
                "Symbol: MyClass at Line 1 in hover.dfy",
                "Kind: Class",
                "Type: N/A",
                "Scope: _module",
                "Declaration: This symbol is a declaration."
            };
            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}