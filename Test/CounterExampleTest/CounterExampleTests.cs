using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.Handler;
using DafnyLanguageServer.Services;
using Microsoft.Boogie;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CounterExample = DafnyLanguageServer.DafnyAccess.CounterExampleProvider.CounterExample;
using CounterExamples = System.Collections.Generic.List<DafnyLanguageServer.DafnyAccess.CounterExampleProvider.CounterExample>;
using CounterExampleState = DafnyLanguageServer.DafnyAccess.CounterExampleProvider.CounterExampleState;
using CounterExampleVariable = DafnyLanguageServer.DafnyAccess.CounterExampleProvider.CounterExampleVariable;
using DafnyConsolePrinter = DafnyLanguageServer.DafnyAccess.DafnyConsolePrinter;

namespace Tests
{

    class FakeDafnyTranslationUnitForCounterExamples : IDafnyTranslationUnit
    {
        private readonly CounterExamples counterExamples = new CounterExamples();
        public CounterExamples CounterExample() => counterExamples;

        public FakeDafnyTranslationUnitForCounterExamples()
        {
            counterExamples.Add(new CounterExample());
            var states = counterExamples[0].States;
            //first two states are ignored as-is by Dafny Provider
            states.Add(new CounterExampleState());
            states.Add(new CounterExampleState());
        }

        public void AddCounterExampleToFake(int col, int row, string[] vars, string[] vals)
        {
            var states = counterExamples[0].States;

            states.Add(new CounterExampleState());
            var state = counterExamples[0].States.Last();
            state.Column = col;
            state.Line = row;
            state.Variables = new List<CounterExampleVariable>();

            for (int i = 0; i < vars.Length; i++)
            {
                state.Variables.Add(new CounterExampleVariable
                {
                    //CanonicalName = "a",  //Field not used, add here if used in future tests
                    Name = vars[i],
                    //RealName = "c",       //Field not used, add here if used in future tests
                    Value = vals[i]
                });
            }

        }

        public bool Verify() { throw new NotImplementedException(); }
        public List<ErrorInformation> GetErrors() { throw new NotImplementedException(); }
        public List<SymbolTable.SymbolInformation> Symbols() { throw new NotImplementedException(); }
        public void DotGraph() { throw new NotImplementedException(); }
    }

    public class UnitTests
    {
        [Test]
        public void NoCounterExample()
        {

            FakeDafnyTranslationUnitForCounterExamples fake = new FakeDafnyTranslationUnitForCounterExamples();

            var service = new CounterExampleService(fake);
            CounterExampleResults results = service.ProvideCounterExamples().Result;

            Assert.AreEqual(0, results.CounterExamples.Count,
                $"Counter Example should only contain 0 counter examples.");

        }

        [Test]
        public void SingleCounterExample()
        {
            const int col = 2;
            const int row = 2;
            string[] vars = { "myVar" };
            string[] vals = { "myVal" };

            FakeDafnyTranslationUnitForCounterExamples fake = new FakeDafnyTranslationUnitForCounterExamples();
            fake.AddCounterExampleToFake(col, row, vars, vals);

            var service = new CounterExampleService(fake);
            CounterExampleResults results = service.ProvideCounterExamples().Result;

            Assert.AreEqual(1, results.CounterExamples.Count,
                $"Counter Example should only contain 1 counter examples.");

            CompareCounterExampleWithExpectation(results.CounterExamples[0], col, row, vars, vals);
        }


        [Test]
        public void SingleCounterExampleWithTwoVars()
        {
            const int col = 2;
            const int row = 2;
            string[] vars = { "myVar", "otherVar" };
            string[] vals = { "myVal", "otherVal" };

            FakeDafnyTranslationUnitForCounterExamples fake = new FakeDafnyTranslationUnitForCounterExamples();
            fake.AddCounterExampleToFake(col, row, vars, vals);

            var service = new CounterExampleService(fake);
            CounterExampleResults results = service.ProvideCounterExamples().Result;

            Assert.AreEqual(1, results.CounterExamples.Count,
                $"Counter Example should only contain 1 counter examples.");

            CompareCounterExampleWithExpectation(results.CounterExamples[0], col, row, vars, vals);
        }

        [Test]
        public void TwoCounterExamples()
        {
            const int col1 = 2;
            const int row1 = 2;
            string[] vars1 = { "myVar" };
            string[] vals1 = { "myVal" };


            const int col2 = 3;
            const int row2 = 3;
            string[] vars2 = { "myVar2" };
            string[] vals2 = { "myVal2" };

            FakeDafnyTranslationUnitForCounterExamples fake = new FakeDafnyTranslationUnitForCounterExamples();
            fake.AddCounterExampleToFake(col1, row1, vars1, vals1);
            fake.AddCounterExampleToFake(col2, row2, vars2, vals2);

            var service = new CounterExampleService(fake);
            CounterExampleResults results = service.ProvideCounterExamples().Result;

            Assert.AreEqual(2, results.CounterExamples.Count,
                $"Counter Example should only contain 2 counter examples.");

            CompareCounterExampleWithExpectation(results.CounterExamples[0], col1, row1, vars1, vals1);
            CompareCounterExampleWithExpectation(results.CounterExamples[1], col2, row2, vars2, vals2);
        }


        [Test]
        public void ManyExamplesWithManyVars()
        {
            const int col1 = 1;
            const int row1 = 31;
            string[] vars1 = { "myVar", "a", "b" };
            string[] vals1 = { "myVal", "-2", "((3))" };


            const int col2 = 211;
            const int row2 = 2123;
            string[] vars2 = { "abc", "def", "ghi" };
            string[] vals2 = { "0", "((-23))", "#less'1" };


            const int col3 = 22;
            const int row3 = 222;
            string[] vars3 = { "abc" };
            string[] vals3 = { "#less'1" };

            FakeDafnyTranslationUnitForCounterExamples fake = new FakeDafnyTranslationUnitForCounterExamples();
            fake.AddCounterExampleToFake(col1, row1, vars1, vals1);
            fake.AddCounterExampleToFake(col2, row2, vars2, vals2);
            fake.AddCounterExampleToFake(col3, row3, vars3, vals3);

            var service = new CounterExampleService(fake);
            CounterExampleResults results = service.ProvideCounterExamples().Result;

            Assert.AreEqual(3, results.CounterExamples.Count,
                $"Counter Example should only contain 3 counter examples.");

            CompareCounterExampleWithExpectation(results.CounterExamples[0], col1, row1, vars1, vals1);
            CompareCounterExampleWithExpectation(results.CounterExamples[1], col2, row2, vars2, vals2);
            CompareCounterExampleWithExpectation(results.CounterExamples[2], col3, row3, vars3, vals3);
        }


        private static void CompareCounterExampleWithExpectation(CounterExampleResult r, int col, int row, string[] vars, string[] vals)
        {
            Assert.AreEqual(col, r.Col,
                "A column index is wrong in the provided counter example");
            Assert.AreEqual(row, r.Line,
                "A line (row) index is wrong in the provided counter example");

            foreach (string var in vars)
            {
                Assert.Contains(var, r.Variables.Keys,
                    $"The key {var} is not provided in the counter examples.");
            }

            Dictionary<string, string> expected = new Dictionary<string, string>();
            for (int i = 0; i < vars.Length; i++)
            {
                expected.Add(vars[i], vals[i]);
            }

            CollectionAssert.AreEqual(
                expected.OrderBy(kv => kv.Key).ToList(),
                r.Variables.OrderBy(kv => kv.Key).ToList(),
                "The Key-Value-Pairs 'variable: value' are not equal"
            );

            var a = r.Variables.OrderBy(kv => kv.Key).ToList();
        }

    }
    public class IntegrationTests
    {
        private static readonly string assemblyPath = Path.GetDirectoryName(typeof(IntegrationTests).Assembly.Location);
        internal static readonly string testPath = Path.GetFullPath(Path.Combine(assemblyPath, "../Test/CounterExampleFiles"));

        private CounterExampleResults ProvideCounterExamples(string filename)
        {
            ExecutionEngine.printer = new DafnyConsolePrinter();
            string fullFilePath = Path.Combine(testPath, filename);
            string source = File.ReadAllText(fullFilePath);
            DafnyTranslationUnit h = new DafnyTranslationUnit(fullFilePath, source);
            var service = new CounterExampleService(h);
            return service.ProvideCounterExamples().Result;
        }

        [Test]
        public void Fail1()
        {
            var results = ProvideCounterExamples("fail1.dfy");
            Assert.AreEqual(1, results.CounterExamples.Count);
        }

        [Test]
        public void Fail2()
        {
            var results = ProvideCounterExamples("fail2.dfy");
            Assert.AreEqual(2, results.CounterExamples.Count);
        }

        [Test]
        public void TwoMethods()
        {
            var results = ProvideCounterExamples("twoMethods.dfy");
            Assert.AreEqual(1, results.CounterExamples.Count);
        }

        [Test]
        public void Ok()
        {
            var results = ProvideCounterExamples("ok.dfy");
            Assert.AreEqual(0, results.CounterExamples.Count);
        }

        [Test]
        public void InvalidFileName()
        {
            var ex = Assert.Throws<AggregateException>(() =>
            {
                ExecutionEngine.printer = new DafnyConsolePrinter();
                string fullFilePath = Path.Combine(testPath, "idonotexist.dfy");
                string source = "method a(){}";
                DafnyTranslationUnit h = new DafnyTranslationUnit(fullFilePath, source);
                var service = new CounterExampleService(h);
                var _ = service.ProvideCounterExamples().Result;
            });
            Assert.That(ex.InnerException, Is.TypeOf(typeof(FileNotFoundException)));
        }
    }
}