﻿using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace RenameTest
{
    public class CustomTexts : RenameBase
    {
        protected string f = Files.rn_scopes;

        [Test]
        public void Regular()
        {
            string text = "baba";
            Run(f, 16, 17, text);
            List<string> expected = new List<string>()
            {
                text + " at L15:C16 - L15:C19",
                text + " at L16:C18 - L16:C21"
            };
            VerifyForSingleFile(expected);
        }

        [Test]
        public void SingleCharacter()
        {
            string text = "a";
            Run(f, 16, 17, text);
            List<string> expected = new List<string>()
            {
                text + " at L15:C16 - L15:C19",
                text + " at L16:C18 - L16:C21"
            };
            VerifyForSingleFile(expected);
        }

        [Test]
        public void ReservedWord()
        {
            result = new WorkspaceEdit();
            string text = "method";
            Run(f, 16, 17, text);
            Assert.IsNull(result);
        }

        [Test]
        public void NoWord()
        {
            string text = "";
            Run(f, 16, 17, text);
            Assert.IsNull(result);
        }

        [Test]
        public void UnicodeChallenge()
        {
            string text = "♥";
            Run(f, 16, 17, text);
            Assert.IsNull(result);
        }

        [Test]
        public void WeirdStuff()
        {
            string text = "{}[]]@##@#";
            Run(f, 16, 17, text);
            Assert.IsNull(result);
        }
    }
}