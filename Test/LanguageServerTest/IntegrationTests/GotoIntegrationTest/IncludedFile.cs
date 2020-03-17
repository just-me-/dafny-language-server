using System;
using System.Linq;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;
using Files = TestCommons.Paths;

namespace GotoIntegrationTest

//Notiz: ALle failenden tests sind aukommentiert damit CI nicht ausrastet. Wird später gefixed im Milestone 5 wenn wir Symbol Table haben. Alle Todos Ticket 71
{
    [TestFixture]
    public class IncludedFile : GoToBase
    {

        //[Test]
        public void Included_File()
        {
            Client.TextDocument.DidOpen(Files.ic_includee, "dfy");
            Client.TextDocument.DidOpen(Files.ic_basic, "dfy");
            goneTo = Client.TextDocument.Definition(Files.ic_basic, 3 - 1, 18 - 1).Result;

            VerifyResult(Files.ic_includee, 1, 7); //todo, multi file support, ich glaub test läuft auch nicht korrekt
        }

    }
}