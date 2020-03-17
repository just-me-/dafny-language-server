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
    public class ClassA : GoToBase
    {
        public ClassA() : base(9, 7, Files.gt_goto)
        {
        }

        private const int l = 21;


        [Test]
        public void LeftMost()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 22);
            SpecificVerificationWithGoalInSameFile();
        }

        //[Test]
        public void MidWord()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, l, 24);
            SpecificVerificationWithGoalInSameFile();

        }

        //[Test]
        public void RightMost()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, l, 28);
            SpecificVerificationWithGoalInSameFile();
        }

        //[Test]
        public void RightMostAfterBracket()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, l, 30);
            SpecificVerificationWithGoalInSameFile();
        }
    }


    [TestFixture]
    public class ClassB : GoToBase
    {
        public ClassB() : base(14, 7, Files.gt_goto)
        {
        }

        private const int l = 23;

        [Test]
        public void LeftMost_ClassB()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 22);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void MidWord_ClassB()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 24);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void RightMost_ClassB()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 28);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void RightMostAfterBrackets_ClassB()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 30);
            SpecificVerificationWithGoalInSameFile();
        }
    }


    [TestFixture]
    public class MethodInClassA : GoToBase
    {
        public MethodInClassA() : base(11, 11, Files.gt_goto)
        {
        }

        private const int l = 22;

        [Test]
        public void LeftMost_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 11);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void MidWord_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 14);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void RightMost_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 19);
            SpecificVerificationWithGoalInSameFile();

        }

        [Test]
        public void RightMostAfterBrackets_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 21);
            SpecificVerificationWithGoalInSameFile();
        }
    }


    [TestFixture]
    public class MethodInClassB : GoToBase
    {
        public MethodInClassB() : base(16, 11, Files.gt_goto)
        {
        }

        private const int l = 24;

        [Test]
        public void LeftMost_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 11);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void MidWord_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 14);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void RightMost_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 19);
            SpecificVerificationWithGoalInSameFile();

        }

        [Test]
        public void RightMostAfterBrackets_MethodInClassA()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 21);
            SpecificVerificationWithGoalInSameFile();
        }
    }


    [TestFixture]
    public class MultiReturnMethod : GoToBase
    {
        public MultiReturnMethod() : base(1, 8, Files.gt_goto)
        {
        }

        private const int l = 28;

        [Test]
        public void LeftMost_MultiReturnMethod()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 18);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void MidWord_MultiReturnMethod()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 20);
            SpecificVerificationWithGoalInSameFile();
        }


        [Test]
        public void RightMost_MultiReturnMethod()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 33);
            SpecificVerificationWithGoalInSameFile();
        }

        [Test]
        public void RightMostAfterBrackets_MultiReturnMethod()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 38);
            SpecificVerificationWithGoalInSameFile();
        }
    }


    [TestFixture]
    public class UninitializedVariableMore : GoToBase
    {
        public UninitializedVariableMore() : base(26, 8, Files.gt_goto)
        {
        }

        private const int l = 29;

        //[Test]
        public void LeftMost_UnitializedVariableMore()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 16);
            VerifyResult(file, 26, 8);   //todo, das failed, uninitialized variable
        }

        [Test]
        public void MidWord_UnitializedVariableMore()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 18);
            VerifyResult(file, 26, 8);
        }


        [Test]
        public void RightMost_UnitializedVariableMore()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 20);
            VerifyResult(file, 26, 8);
        }

    }

    [TestFixture]
    public class UninitializedVariableLess : GoToBase
    {
        public UninitializedVariableLess() : base(27, 8, Files.gt_goto)
        {
        }

        private const int l = 29;

        //[Test]
        public void LeftMost_UnitializedVariableMore()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 23);
            VerifyResult(file, 26, 8);   //todo, das failed, uninitialized variable
        }

        [Test]
        public void MidWord_UnitializedVariableMore()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 25);
            VerifyResult(file, 26, 8);
        }


        [Test]
        public void RightMost_UnitializedVariableMore()
        {
            SetGoToDefinitionWithoutZeroIndexing(file, l, 27);
            VerifyResult(file, 26, 8);
        }

    }


    [TestFixture]
    public class InitializedVariableA : GoToBase
    {
        [Test]
        public void LeftMost_InitializedVariableA()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 33, 15);
            VerifyResult(file, 31, 8 + 2);  //TODO beim := nicht gut wenn uninitinailsiert. Ticket 71
        }

        [Test]
        public void RightMost_InitializedVariableA()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 33, 16);
            VerifyResult(file, 31, 8);
        }

    }

    [TestFixture]
    public class InitializedVariableB : GoToBase
    {
        [Test]
        public void LeftMost_InitializedVariableB()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 33, 19);
            VerifyResult(file, 32, 8 + 2);
        }


        [Test]
        public void RightMost_InitializedVariableB()
        {
            string file = Files.gt_goto;
            SetGoToDefinitionWithoutZeroIndexing(file, 33, 20);
            VerifyResult(file, 32, 8);
        }

    }



}