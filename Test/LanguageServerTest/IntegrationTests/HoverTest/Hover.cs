using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
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
        public void Test()
        {
            Client.TextDocument.DidOpen(Files.hv_hover, "dfy");
            var result = Client.TextDocument.Hover(Files.hv_hover, 6, 14).Result;
            var strings = result.Contents.MarkedStrings.ToList();
        }
    }
}