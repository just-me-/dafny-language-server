using System;
using System.Linq;
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

        protected void VerifyResult(string expectedFile, int expLine, int expCol)
        {
            long line = goneTo.FirstOrDefault().Location.Range.Start.Line;
            long col = goneTo.FirstOrDefault().Location.Range.Start.Character;
            Uri uri = goneTo.FirstOrDefault().Location.Uri;

            Assert.AreEqual(expLine, line + 1);  //adding 1 here to get rid of the 0 indexing.
            Assert.AreEqual(expCol, col + 1 - 1);    //same, but removing one again because cursor is one off to the right in current implementation   todo fixen Ticket 71
            Assert.AreEqual(new Uri(expectedFile), uri);
        }


        protected int expectedLine;
        protected int expectedChar;
        protected string file;

        protected GoToBase(int expectedLine, int expectedChar, string file) : base("GoTo")
        {
            this.expectedLine = expectedLine;
            this.expectedChar = expectedChar;
            this.file = file;
        }

        public GoToBase() : base("GoTo")
        {
        }

        protected void SpecificVerificationWithGoalInSameFile()
        {
            if (expectedChar == 0 || expectedLine == 0 || file is null)
            {
                throw new ArgumentException("Need to set expectation class variables to use specific verificatoin");
            }
            VerifyResult(file, expectedLine, expectedChar);
        }


    }
}
