using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;

namespace SymbolTableTest
{

    [TestFixture]
    [Category("Unit")]
    public class Wraps
    {
        [Test]

        public void BasicTrueTrivial()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 5;
            int targetCol = 5;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);


            Assert.IsTrue(actual);
        }

        [Test]
        public void LineBefore()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 2;
            int targetCol = 2;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }


        [Test]
        public void LineAfter()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 9;
            int targetCol = 9;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }


        [Test]
        public void SameStartLineTrue()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 9;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void SameStartLineFalse()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 1;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void SameEndLineTrue()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 7;
            int targetCol = 1;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void SameEndLineFalse()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 7;
            int targetCol = 9;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void AllLinesEqualBefore()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 1;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void AllLinesEqualInside()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 5;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void AllLinesEqualAfter()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 9;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }


        [Test]
        public void targetAtSymbolStart()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 3;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void targetAtSymbolEnd()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 7;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void targetAtSymbolStartMultiLine()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 3;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void targetAtSymbolEndMultiLine()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 7;
            int targetCol = 7;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }
    
    [Test]
        public void targetAtSymbolStart_1off()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 2;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void targetAtSymbolEnd_1off()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 8;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void targetAtSymbolStartMultiLine_1off()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 3;
            int targetCol = 2;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void targetAtSymbolEndMultiLine_1off()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 7;
            int targetCol = 8;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void zeroCharTheoreticalEdgeCase()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 3;  

            int targetLine = 3;
            int targetCol = 3;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void singleCharEdgeCase1off()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 4;

            int targetLine = 3;
            int targetCol = 2;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }

        [Test]
        public void singleCharEdgeCaseAtStart()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 4;

            int targetLine = 3;
            int targetCol = 3;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void singleCharEdgeCaseAtEnd()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 4;

            int targetLine = 3;
            int targetCol = 4;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsTrue(actual);
        }

        [Test]
        public void singleCharEdgeCase1out()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 3;
            int endCol = 4;

            int targetLine = 3;
            int targetCol = 5;

            bool actual = SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol);
            Assert.IsFalse(actual);
        }


        [Test]
        public void StartAfterEndLine()
        {
            int startLine = 9;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 5;
            int targetCol = 5;

            Assert.Throws<ArgumentException>(() =>
                SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol));
        }

        [Test]
        public void StartAfterEndCol()
        {
            int startLine = 7;
            int startCol = 9;
            int endLine = 7;
            int endCol = 7;

            int targetLine = 5;
            int targetCol = 5;

            Assert.Throws<ArgumentException>(() =>
                SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol));
        }

        [Test]
        public void Negatives() //negative target is not tested... -2 is just not in range of something positive.
        {
            int startLine = -1;
            int startCol = -1;
            int endLine = -1;
            int endCol = -1;

            int targetLine = 5;
            int targetCol = 5;

            Assert.Throws<ArgumentException>(() =>
                SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol));
        }

        [Test]
        public void Overflow()
        {
            int startLine = 3;
            int startCol = 3;
            int endLine = 7;
            int endCol = 7;
            
            int bignumber = int.MaxValue;

            endCol += bignumber;

            int targetLine = 5;
            int targetCol = 5;

            Assert.Throws<ArgumentException>(() =>
                SymbolUtil.Wraps(startLine, startCol, endLine, endCol, targetLine, targetCol));
        }
    }

    public class ConreteWrapperTests
    {
        ISymbolInformation symbolWithBody = new SymbolInformationFake(2,0,4,0,6,0);
        ISymbolInformation symbolWithoutBody = new SymbolInformationFake(2,0);
        ISymbolInformation symbolInAnotherFile = new SymbolInformationFake(2,0,-1,-1,-1,-1,"C:/another.dfy") {Kind = Kind.Class};
        Uri defaultUri = new Uri("C:/file.dfy");



        [Test]
        public void Body_Inside()
        {
            int line = 5;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolBody(symbolWithBody, defaultUri, line, character);
            Assert.IsTrue(actual);
        }

        [Test]
        public void Body_Outside1()
        {
            int line = 1;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolBody(symbolWithBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }


        [Test]
        public void Body_Outside2()
        {
            int line = 7;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolBody(symbolWithBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }




        [Test]
        public void TotalRange_Inside1()
        {
            int line = 3;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolWithBody, defaultUri, line, character);
            Assert.IsTrue(actual);
        }

        [Test]
        public void TotalRange_Inside2()
        {
            int line = 5;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolWithBody, defaultUri, line, character);
            Assert.IsTrue(actual);
        }

        [Test]
        public void TotalRange_Outside1()
        {
            int line = 1;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolWithBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }

        [Test]
        public void TotalRange_Outside2()
        {
            int line = 7;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolWithBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }


        [Test]
        public void Identifier_Inside()
        {
            int line = 2;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolIdentifier(symbolWithBody, defaultUri, line, character);
            Assert.IsTrue(actual);
        }

        [Test]
        public void Identifier_Outside1()
        {
            int line = 2;
            int character = 99;
            bool actual = SymbolUtil.PositionIsWithinSymbolIdentifier(symbolWithBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }

        [Test]
        public void Identifier_Outside2()
        {
            int line = 2;
            int character = -1;
            bool actual = SymbolUtil.PositionIsWithinSymbolIdentifier(symbolWithBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }


        [Test]
        public void Wrongfile_Identifier()
        {
            int line = 2;
            int character = 2;
            bool actual = SymbolUtil.PositionIsWithinSymbolIdentifier(symbolInAnotherFile, defaultUri, line, character);
            Assert.IsFalse(actual);
        }

        [Test]
        public void Wrongfile_Body()
        {
            int line = 2;
            int character = 2;
            bool actual = SymbolUtil.PositionIsWithinSymbolBody(symbolInAnotherFile, defaultUri, line, character);
            Assert.IsFalse(actual);
        }

        [Test]
        public void Wrongfile_totalRange()
        {
            int line = 2;
            int character = 2;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolInAnotherFile, defaultUri, line, character);
            Assert.IsFalse(actual);
        }




        [Test]
        public void InBodyOfSymbolWOBody()
        {
            int line = 2;
            int character = 2;
            bool actual = SymbolUtil.PositionIsWithinSymbolBody(symbolWithoutBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }


        [Test]
        public void InBodyOfSymbolWOBody2()
        {
            int line = 3;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolBody(symbolWithoutBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }

        [Test]
        public void SymbolWOBody_TotalRange_In()
        {
            int line = 3;
            int character = 0;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolWithoutBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }

        [Test]
        public void SymbolWOBody_TotalRange_Out()
        {
            int line = 2;
            int character = 222;
            bool actual = SymbolUtil.PositionIsWithinSymbolTotalRange(symbolWithoutBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }


        [Test]
        public void SymbolWOBody_IdentifierRange_In()
        {
            int line = 2;
            int character = 2;
            bool actual = SymbolUtil.PositionIsWithinSymbolIdentifier(symbolWithoutBody, defaultUri, line, character);
            Assert.IsTrue(actual);
        }

        [Test]
        public void SymbolWOBody_IdentifierRange_Out()
        {
            int line = 2;
            int character = 222;
            bool actual = SymbolUtil.PositionIsWithinSymbolIdentifier(symbolWithoutBody, defaultUri, line, character);
            Assert.IsFalse(actual);
        }


    }
}
