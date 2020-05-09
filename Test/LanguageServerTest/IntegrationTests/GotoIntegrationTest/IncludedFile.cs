using System;
using System.Linq;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace GotoIntegrationTest

{
    [TestFixture]
    public class IncludedFile : GoToBase
    {

        [Test]
        public void ExternalClassDefinition()
        {
            Client.TextDocument.DidOpen(Files.gt_include_main, "dfy");
            goneTo = Client.TextDocument.Definition(Files.gt_include_main, 3 - 1, 18 - 1).Result;

            VerifyResult(Files.gt_includee, 1, 7);
        }


        [Test]
        public void MemberOfExternalClass()
        {
            Client.TextDocument.DidOpen(Files.gt_include_main, "dfy");
            goneTo = Client.TextDocument.Definition(Files.gt_include_main,  4 - 1, 17 - 1).Result;

            VerifyResult(Files.gt_includee, 3, 21);
        }

    }
}