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
                "L4 C19: in1 = 14; out1 = false; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void Real()
        {
            GetCounterExamples(Files.ce_float);
            List<string> expecation = new List<string>()
            {
                "L4 C21: inp1 = -24; out1 = -48; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void Set()
        {
            GetCounterExamples(Files.ce_set);
            List<string> expecation = new List<string>()
            {
                "L4 C20: in1 = (T@U!val!55); in1#0.[2] = false; out1 = (T@U!val!62); out1#0.[2] = true; "
            };
            VerifyResults(expecation);
        }

        [Test]
        public void Sequence()
        {
            GetCounterExamples(Files.ce_sequence);
            List<string> expecation = new List<string>()
            {
                "L4 C19: in1 = (T@U!val!55); out1 = false; "
        };
            VerifyResults(expecation);
        }

        [Test]
        public void String()
        {
            GetCounterExamples(Files.ce_string);
            List<string> expecation = new List<string>()
            {
                "L4 C22: in1 = [Length 2](T@U!val!55); out1 = true; "
            };
            VerifyResults(expecation);
        }

    }
}