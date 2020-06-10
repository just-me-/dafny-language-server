using NUnit.Framework;
using System.Collections.Generic;
using Files = TestCommons.Paths;

namespace CodeLensIntegrationTest
{
    [TestFixture]
    public class CodeLensTest : CodeLensBase
    {
        [Test]
        public void BasicFile()
        {
            RunCodeLens(Files.cl_basic);
            List<ExpectedCodeLensEntry> exp = new List<ExpectedCodeLensEntry>()
            {
                new ExpectedCodeLensEntry
                {
                    Name = "1 reference to ClassA",
                    References = new List<ExpectedReference>
                    {
                        new ExpectedReference{Col = 21, Line = 17}
                    }
                },
                new ExpectedCodeLensEntry
                {
                    Name = "1 reference to myMethod",
                    References = new List<ExpectedReference>
                    {
                        new ExpectedReference{Col = 10, Line = 18}
                    }
                },
                new ExpectedCodeLensEntry
                {
                    Name = "2 references to ClassB",
                    References = new List<ExpectedReference>
                    {
                        new ExpectedReference{Col = 22, Line = 19},
                        new ExpectedReference{Col = 22, Line = 20},
                    }
                },
                new ExpectedCodeLensEntry
                {
                    Name = "3 references to myMethod",
                    References = new List<ExpectedReference>
                    {
                        new ExpectedReference{Col = 11, Line = 21},
                        new ExpectedReference{Col = 11, Line = 22},
                        new ExpectedReference{Col = 11, Line = 23},
                    }
                },
                new ExpectedCodeLensEntry
                {
                    Name = "1 reference to ClassC",
                    References = new List<ExpectedReference>
                    {
                        new ExpectedReference{Col = 18, Line = 29}
                    }
                },
                new ExpectedCodeLensEntry
                {
                    Name = "Not used yet. Can you remove myMethod?"
                },
                new ExpectedCodeLensEntry
                {
                    Name = "1 reference to MultipleReturns",
                    References = new List<ExpectedReference>
                    {
                        new ExpectedReference{Col = 17, Line = 27}
                    }
                }
            };
            VerifyResults(exp);
        }

        [Test]
        public void IncludeeFile()
        {
            RunCodeLens(Files.cl_includee);
            List<ExpectedCodeLensEntry> exp = new List<ExpectedCodeLensEntry>()
            {
                new ExpectedCodeLensEntry
                {
                    Name = "Not used yet. Can you remove C?",
                },
                new ExpectedCodeLensEntry
                {
                    Name = "Not used yet. Can you remove increase?",
                }
            };
            VerifyResults(exp);
        }

        [Test]
        public void MainFile()
        {
            RunCodeLens(Files.cl_include_main);
            List<ExpectedCodeLensEntry> exp = new List<ExpectedCodeLensEntry>() { };
            VerifyResults(exp);
        }

        [Test]
        public void EmptyFile()
        {
            RunCodeLens(Files.cl_empty);
            List<ExpectedCodeLensEntry> exp = new List<ExpectedCodeLensEntry>() { };
            VerifyResults(exp);
        }
    }
}