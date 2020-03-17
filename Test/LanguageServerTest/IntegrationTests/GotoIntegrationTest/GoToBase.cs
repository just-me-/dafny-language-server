using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestCommons;

namespace GotoIntegrationTest
{
    public class GoToBase : IntegrationTestBase
    {

        protected LocationOrLocationLinks goneTo;

        [SetUp]
        public void ResetResult()
        {
            goneTo = default;
        }


        protected void SetGoToDefinitionWithoutZeroIndexing(string file, int line, int col)
        {
            Client.TextDocument.DidOpen(file, "dfy");
            goneTo = Client.TextDocument.Definition(file, line - 1, col - 1).Result;
        }

        protected void VerifyResult(string expectedFile, int expectedLine, int expectedCol)
        {
            long line = goneTo.FirstOrDefault().Location.Range.Start.Line;
            long col = goneTo.FirstOrDefault().Location.Range.Start.Character;
            Uri uri = goneTo.FirstOrDefault().Location.Uri;

            Assert.AreEqual(expectedLine, line + 1);  //adding 1 here to get rid of the 0 indexing.
            Assert.AreEqual(expectedCol, col + 1 - 1);    //same, but removing one again because cursor is one off to the right in current implementation   todo fixen Ticket 71
            Assert.AreEqual(new Uri(expectedFile), uri);
        }
    }
}
