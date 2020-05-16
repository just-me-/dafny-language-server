using System;
using DafnyLanguageServer.Commons;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace ContentManagerTest
{
    
    public class PhysicalFileTest
    {
        [Test]
        public void SimpleTest1()
        {
            const int l = 1;

            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            int result = f.GetLengthOfLine(l);

            Assert.AreEqual(3, result);
        }

        [Test]
        public void SimpleTest2()
        {
            const int l = 0;

            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            int result = f.GetLengthOfLine(l);

            Assert.AreEqual(1, result);
        }

        [Test]
        public void LineTooLarge()
        {
            const int l = 3;

            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            Assert.Throws<ArgumentException>(() => f.GetLengthOfLine(l));
        }

        [Test]
        public void LineNegativeThrows()
        {
            const int l = -1;

            var f = new PhysicalFile()
            {
                Sourcecode = "a\nabc\na"
            };

            Assert.Throws<ArgumentException>(() => f.GetLengthOfLine(l));
        }
    }
    
}