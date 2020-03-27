using System.Collections.Generic;
using DafnyLanguageServer.Handler;
using NUnit.Framework;
using TestCommons;
using Files = TestCommons.Paths;

namespace CounterExampleIntegrationTest
{
    [TestFixture]
    public class VariousTypes : CounterExampleBase
    {


        [Test]
        public void Bool()
        {
            GetCounterExamples(Files.ce_bool);
            List<string> expecation = new List<string>()
            {
                "L3 C15: in1 = false; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void Real()
        {
            GetCounterExamples(Files.ce_float);
            List<string> expecation = new List<string>()
            {
                "L3 C22: inp1 = - 160.0; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void Set()
        {
            GetCounterExamples(Files.ce_set);
            List<string> expecation = new List<string>()
            {
                "L3 C21: in1 = [Object Reference]; out1 = [Object Reference]; "
        };
            VerifyResults(expecation);
        }

        [Test]
        public void Sequence()
        {
            GetCounterExamples(Files.ce_sequence);
            List<string> expecation = new List<string>()
            {
                "L3 C20: in1 = [Object Reference]; "
        };
            VerifyResults(expecation);
        }

        [Test]
        public void String()
        {
            GetCounterExamples(Files.ce_string);
            List<string> expecation = new List<string>()
            {
                "L3 C23: in1 = [Object Reference]; "
            };
            VerifyResults(expecation);
        }

    }
}