using DafnyLanguageServer.DafnyAccess;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using DafnyLanguageServer.Handler;
using TestCommons;
using Files = TestCommons.Paths;

namespace CounterExampleProviderUnitTest
{
    public class BVDChecks
    {
        [Test]
        public void PostconditionViolation1()
        {
            var model = Files.ce_fail1_bvd;
            var file = Files.ce_fail1;

            var src = File.ReadAllText(file);

            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C20: inp1 = -160; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

        [Test]
        public void PostconditionViolation2()
        {
            var model = Files.ce_fail2_bvd;
            var file = Files.ce_fail2;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C23: inp1 = -24; inp2 = 0; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

        [Test]
        public void StringReference()
        {
            var model = Files.ce_string_bvd;
            var file = Files.ce_string;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C23: in1 = [Object Reference]; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

        [Test]
        public void LoopInvariantFailure()
        {
            var model = Files.ce_li_bvd;
            var file = Files.ce_li;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L12 C24: b = 0; n = 5881; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

        [Test]
        public void BooleanCE()
        {
            var model = Files.ce_bool_bvd;
            var file = Files.ce_bool;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C15: in1 = false; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

        [Test]
        public void FloatyCE()
        {
            var model = Files.ce_float_bvd;
            var file = Files.ce_float;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C22: inp1 = -160.0; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

        [Test]
        public void CEWithTwoMethods()
        {
            var model = Files.ce_2m_bvd;
            var file = Files.ce_2m;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C19: in1 = 2446; ",
                "L9 C19: in2 = -160; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }


        [Test]
        public void CEWithTwoConnectedMethods()
        {
            var model = Files.ce_2mc_bvd;
            var file = Files.ce_2mc;

            var src = File.ReadAllText(file);
            var provider = new CounterExampleModelFileTranslator(src, model);

            CounterExampleResults result = provider.LoadCounterModel();
            List<string> resultAsString = result.CounterExamples.ToStringList();

            List<string> expectation = new List<string>()
            {
                "L3 C19: in1 = 2446; ", 
                "L9 C19: in2 = 891; "
            };
            CollectionAssert.AreEquivalent(expectation, resultAsString);
        }

    }
}