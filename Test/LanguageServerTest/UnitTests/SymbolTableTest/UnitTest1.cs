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
    public class Tests
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

        //todo work in progress
        //[Test]
        public void Test4_useSymbolsbeforeDeclaration()
        {
            var fa = Files.st_04;
            var fe = Files.st_04e;

            var actual = GetSymbolsAsList(fa);
            var excpected = GetExpectation(fe);

            CollectionAssert.AreEquivalent(excpected, actual);
        }

        //todo work in progress
        //[Test]
        public void Test5_useSymbolsFromGlobalScope()
        {
            var fa = Files.st_05;
            var fe = Files.st_05e;

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
            var actual = sm.GetEntriesAsStringList();

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