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

            string textExpectation = " **`r`** *(line 7, hover.dfy)*\n" +
                                     "* **Type:** int\n" +
                                     "* **Kind:** Variable\n" +
                                     "* **Scope:** `addOne`\n" +
                                     "* **Declaration:** line 5, hover.dfy";
            Console.WriteLine(actual);
            Assert.AreEqual(textExpectation, actual);
        }

        [Test]
        public void ClassDeclaration()
        {
            Client.TextDocument.DidOpen(Files.hv_hover, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_hover, 0, 6).Result;
            var actual = result.Contents.MarkupContent.Value;


            string textExpectation = " **`MyClass`** *(line 1, hover.dfy)*\n" +
                                     "* **Type:** N/A\n" +
                                     "* **Kind:** Class\n" +
                                     "* **Scope:** `_module`\n" +
                                     "* **Declaration:** This symbol is the declaration.";

            Console.WriteLine(actual);

            Assert.AreEqual(textExpectation, actual);
        }
    }
}