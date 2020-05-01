using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DafnyLanguageServer.DafnyAccess;
using DafnyLanguageServer.FileManager;
using DafnyLanguageServer.SymbolTable;
using NUnit.Framework;
using Files = TestCommons.Paths;

namespace SymbolTableTest
{
    public class Basics
    {

        [Test]
        public void Test1_BasicSymbols()
        {
            var fa = Files.st_01;
            var fe = Files.st_01e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test2_ClassAccessors()
        {
            var fa = Files.st_02;
            var fe = Files.st_02e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test3_NestedBlockScopes()
        {
            var fa = Files.st_03;
            var fe = Files.st_03e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test4_useSymbolsbeforeDeclaration()
        {
            var fa = Files.st_04;
            var fe = Files.st_04e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test5_useSymbolsFromGlobalScope()
        {
            var fa = Files.st_05;
            var fe = Files.st_05e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }


        //Note on test:
        //assert, ensures, requires not yet supported, excluded from expecatation temporarily //todo ticket für symboltable weiss nummer grad nich.
        [Test]
        public void Test6_ClassesMethodsMultiReturns_VariousBasicCodeFeatures()
        {
            var fa = Files.st_06;
            var fe = Files.st_06e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }


        [Test]
        public void Test7_IfWhile()
        {
            var fa = Files.st_07;
            var fe = Files.st_07e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }


        [Test]
        public void Test8_NegationTernary()
        {
            var fa = Files.st_08;
            var fe = Files.st_08e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        [Test]
        public void Test9_Traits()
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

        private List<string> GetSymbolsAsList(string f)
        {
            var physFile = new PhysicalFile
            {
                Filepath = f,
                Sourcecode = File.ReadAllText(f)
            };

            var dtu = new DafnyTranslationUnit(physFile);
            var dafnyProg = dtu.Verify().DafnyProgram;
            var sm = new SymbolTableManager(dafnyProg);
            var navigator = new SymbolTableNavigator();
            Predicate<SymbolInformation> filter = x => x.Kind != Kind.BlockScope;
            var symbols = navigator.TopDownAll(sm.SymbolTables.First().Value, filter); // todo husthust
            var actual = symbols.Select(x => x.ToString()).ToList();

            Console.WriteLine("SymboleTable for " + f);
            Console.Write(sm.CreateDebugReadOut());

            return actual;
        }

        private List<string> GetExpectation(string f)
        {
            return File.ReadAllLines(f).ToList();
        }
    }
}