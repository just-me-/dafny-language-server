using NUnit.Framework;
using System;
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

        [Test]
        public void NoParam()
        {
            Client.TextDocument.DidOpen(Files.hv_params, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_params, 3, 14).Result;
            var actual = result.Contents.MarkupContent.Value;

            string textExpectation = " **`noParam()`** *(line 4, parameters.dfy)*\n";
            Assert.IsTrue(actual.Contains(textExpectation));
        }

        [Test]
        public void OneParam()
        {
            Client.TextDocument.DidOpen(Files.hv_params, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_params, 5, 16).Result;
            var actual = result.Contents.MarkupContent.Value;

            string textExpectation = " **`oneParam(a: int)`** *(line 6, parameters.dfy)*\n";
            Assert.IsTrue(actual.Contains(textExpectation));
        }

        [Test]
        public void TwoParam()
        {
            Client.TextDocument.DidOpen(Files.hv_params, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_params, 7, 14).Result;
            var actual = result.Contents.MarkupContent.Value;

            string textExpectation = " **`twoParam(a: int, b: int)`** *(line 8, parameters.dfy)*\n";
            Assert.IsTrue(actual.Contains(textExpectation));
        }

        [Test]
        public void ThreeParam()
        {
            Client.TextDocument.DidOpen(Files.hv_params, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_params, 12, 11).Result;
            var actual = result.Contents.MarkupContent.Value;

            string textExpectation = " **`MyMethod(a: int, b: int, c: int)`** *(line 13, parameters.dfy)*\n";
            Assert.IsTrue(actual.Contains(textExpectation));
        }
    }
}