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
    public class IrregularPositions : GoToBase
    {

        [Test]
        public void RandomSpot()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 18, 1);
            Assert.IsEmpty(goneTo);
        }

        [Test]
        public void UnrealSpot()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 20000, 5);
            Assert.IsEmpty(goneTo);
        }

        [Test]
        public void NegativeSpot()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, -5, -5);
            Assert.IsEmpty(goneTo);
        }

    }
}