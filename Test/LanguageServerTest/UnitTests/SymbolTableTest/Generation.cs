using DafnyLanguageServer.Commons;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Files = TestCommons.Paths;

namespace SymbolTableTest
{
    /// <summary>
    /// Tests the creation of Symbol Tables
    /// </summary>
    public class Generation
    {
        [Test]
        public void Test01_BasicSymbols()
        {
            var fa = Files.st_01;
            var fe = Files.st_01e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test02_ClassAccessors()
        {
            var fa = Files.st_02;
            var fe = Files.st_02e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test03_NestedBlockScopes()
        {
            var fa = Files.st_03;
            var fe = Files.st_03e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test04_useSymbolsbeforeDeclaration()
        {
            var fa = Files.st_04;
            var fe = Files.st_04e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test05_useSymbolsFromGlobalScope()
        {
            var fa = Files.st_05;
            var fe = Files.st_05e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test06_ClassesMethodsMultiReturns_VariousBasicCodeFeatures()
        {
            var fa = Files.st_06;
            var fe = Files.st_06e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test07_IfWhile()
        {
            var fa = Files.st_07;
            var fe = Files.st_07e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test08_NegationTernary()
        {
            var fa = Files.st_08;
            var fe = Files.st_08e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test09_Traits()
        {
            var fa = Files.st_09;
            var fe = Files.st_09e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test10_ModifiesAndCo()
        {
            var fa = Files.st_10;
            var fe = Files.st_10e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test11_ModulesWithinASingleFile()
        {
            var fa = Files.st_11;
            var fe = Files.st_11e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test12_ModulesWithinASingleFile()
        {
            var fa = Files.st_12;
            var fe = Files.st_12e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test13_AccessModuleFromGlobalScope()
        {
            var fa = Files.st_13;
            var fe = Files.st_13e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test14_IncludedModule()
        {
            var fa = Files.st_14;
            var fe = Files.st_14e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test15_Sequences_Arrays()
        {
            var fa = Files.st_15;
            var fe = Files.st_15e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test16_Comprehension_Lambda_ForAll_BoundVars()
        {
            var fa = Files.st_16;
            var fe = Files.st_16e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test17_Sets()
        {
            var fa = Files.st_17;
            var fe = Files.st_17e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test18_ctor()
        {
            var fa = Files.st_18;
            var fe = Files.st_18e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        private IEnumerable<string> GetSymbolsAsList(string f)
        {
            var physFile = new PhysicalFile
            {
                Filepath = f,
                Sourcecode = File.ReadAllText(f)
            };

            var dtu = new DafnyTranslationUnit(physFile);
            var dafnyProg = dtu.Verify().DafnyProgram;
            ISymbolTableGenerator st = new SymbolTableGenerator(dafnyProg);
            ISymbolInformation root = st.GenerateSymbolTable();
            ISymbolTableManager sm = new SymbolTableManager(root);

            ISymbolNavigator navigator = new SymbolNavigator();
            List<ISymbolInformation> symbols = new List<ISymbolInformation>();

            foreach (var modul in root.Children)
            {
                symbols.AddRange(navigator.TopDownAll(modul));
            }

            var actual = symbols.Select(x => x.ToDebugString()).ToList();

            Console.WriteLine("SymboleTable for " + f);
            Console.Write(((SymbolTableManager)sm).CreateDebugReadOut());

            return actual;
        }

        private List<string> GetExpectation(string f)
        {
            return File.ReadAllLines(f).ToList();
        }
    }
}