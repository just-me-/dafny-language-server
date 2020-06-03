using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
            var actual = result.Contents.MarkupContent.Value;

            string textExpectation = "## Symbol Information\n" +
                                     "*r at Line 7 in hover.dfy*\n" +
                                     "* **Kind:** Variable\n" +
                                     "* **Type:** int\n" +
                                     "* **Scope:** `addOne`\n" +
                                     "* **Declaration:** r at Line 5 in hover.dfy";
            Console.WriteLine(actual);
            Assert.AreEqual(textExpectation, actual);
        }

        [Test]
        public void ClassDeclaration()
        {
            Client.TextDocument.DidOpen(Files.hv_hover, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_hover, 0, 6).Result;
            var actual = result.Contents.MarkupContent.Value;


            string textExpectation = "## Symbol Information\n" +
                                     "*MyClass at Line 1 in hover.dfy*\n" +
                                     "* **Kind:** Class\n" +
                                     "* **Type:** N/A\n" +
                                     "* **Scope:** `_module`\n" +
                                     "* **Declaration:** This symbol is a declaration.";

            Console.WriteLine(actual);

            Assert.AreEqual(textExpectation, actual);
        }
    }
}