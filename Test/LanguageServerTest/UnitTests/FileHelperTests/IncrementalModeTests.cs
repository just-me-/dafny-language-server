using DafnyLanguageServer.Commons;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System;

namespace ContentManagerTest
{
    public class GetIndexTest
    {
        private PhysicalFile f;

        [SetUp]
        public void InitializeFile()
        {
            f = new PhysicalFile()
            {
                Sourcecode = "\r\n\r\nclass MyClass {\r\n\r\n\r\n    var field: int; \r\n\t\r\n\t\r\n method  addOne(i: int) returns(r: int) {\r\n r := i + 1;\r\n       return r; \r\n    }\r\n method aMethod() modifies this { \r\n var aLocalVar:= 2;\r\n field := aLocalVar;  \r\n aLocalVar := addOne(field);\r\n    }\r\n\r\n\r\n\r\n constructor() { }\r\n\r\n}\r\n\r\n"
            };
        }

        [Test]
        public void Pos_0_0()
        {
            Position p = new Position(0, 0);
            var r = f.GetIndex(p);
            Assert.AreEqual(0, r);
        }

        [Test]
        public void Pos_2_0()
        {
            Position p = new Position(2, 0);
            var r = f.GetIndex(p);
            Assert.AreEqual(4, r);
        }

        [Test]
        public void Pos_2_1()
        {
            Position p = new Position(2, 1);
            var r = f.GetIndex(p);
            Assert.AreEqual(5, r);
        }

        [Test]
        public void Pos_2_2()
        {
            Position p = new Position(2, 2);
            var r = f.GetIndex(p);
            Assert.AreEqual(6, r);
        }

        [Test]
        public void Pos_2_3()
        {
            Position p = new Position(2, 3);
            var r = f.GetIndex(p);
            Assert.AreEqual(7, r);
        }

        [Test]
        public void Pos_2_15()
        {
            Position p = new Position(2, 15);
            var r = f.GetIndex(p);
            Assert.AreEqual(19, r);
        }

        [Test]
        public void Pos_2_16()
        {
            Position p = new Position(2, 16);
            var r = f.GetIndex(p);
            Assert.AreEqual(20, r);
        }

        [Test]
        public void Pos_2_17_OutofBounds()
        {
            Position p = new Position(2, 17);
            Assert.Throws<IndexOutOfRangeException>(() => f.GetIndex(p));
        }

        [Test]
        public void Pos_15_24()
        {
            Position p = new Position(15, 24);
            var r = f.GetIndex(p);
            Assert.AreEqual(241, r);
        }

        [Test]
        public void ColumnOutOfBounds()
        {
            Position p = new Position(1, 100);
            Assert.Throws<IndexOutOfRangeException>(() => f.GetIndex(p));
        }

        [Test]
        public void LineOutOfBounds()
        {
            Position p = new Position(100, 0);
            Assert.Throws<IndexOutOfRangeException>(() => f.GetIndex(p));
        }
    }

    public class ApplyChangesTest
    {
        private PhysicalFile f;

        private const string cleanSource = "\r\n\r\nclass MyClass {\r\n\r\n\r\n var field: int; \r\n\t\r\n\t\r\n method  addOne(i: int) returns(r:int) {\r\n r := i + 1;\r\n       return r; \r\n    }\r\n method aMethod() modifies this { \r\n var aLocalVar := 2;\r\n field := aLocalVar;  \r\n aLocalVar := addOne(field);\r\n    }\r\n\r\n\r\n\r\n constructor() { }\r\n\r\n}\r\n\r\n";

        [SetUp]
        public void InitializeFile()
        {
            f = new PhysicalFile()
            {
                Sourcecode = cleanSource
            };
        }

        [Test]
        public void Empty_at_0_0()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 0, Character = 0 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource, cleanSource);
        }

        [Test]
        public void Empty_at_15_15() //this is at:    aLocalV|ar := addOne(field);
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 15 },
                    End = new Position { Line = 15, Character = 15 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource, f.Sourcecode);
        }

        //INSSERT
        [Test]
        public void InsertOneCharAt_0_0()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 0, Character = 0 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual("a" + cleanSource, f.Sourcecode);
        }

        [Test]
        public void InsertOneCharAt_1_0()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position { Line = 1, Character = 0 },
                    End = new Position { Line = 1, Character = 0 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Insert(2, "a"), f.Sourcecode);
        }

        [Test]
        public void InsertOneCharAt_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 15 },
                    End = new Position { Line = 15, Character = 15 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Insert(229, "a"), f.Sourcecode);
        }

        [Test]
        public void InsertTextAt_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "abc",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 15 },
                    End = new Position { Line = 15, Character = 15 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Insert(229, "abc"), f.Sourcecode);
        }

        //DELETE
        [Test]
        public void PressingDELETEat_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 15 },
                    End = new Position { Line = 15, Character = 16 }
                },
                RangeLength = 1
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(229, 1), f.Sourcecode);
        }

        [Test]
        public void PressingRETURNat_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 14 },
                    End = new Position { Line = 15, Character = 15 }
                },
                RangeLength = 1
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(228, 1), f.Sourcecode);
        }

        [Test]
        public void RemovingRangeAt_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 15 },
                    End = new Position { Line = 15, Character = 20 }
                },
                RangeLength = 5
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(229, 5), f.Sourcecode);
        }

        [Test]
        public void RemovingLine_15_WithoutNewLine()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 0 },
                    End = new Position { Line = 15, Character = 35 }
                },
                RangeLength = 35
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(214, 35), f.Sourcecode);
        }

        [Test]
        public void RemovingLine_15_IncludingNewLine()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 0 },
                    End = new Position { Line = 16, Character = 0 }
                },
                RangeLength = 37
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(214, 37), f.Sourcecode);
        }

        [Test]
        public void RemovingLine_1()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 1, Character = 0 }
                },
                RangeLength = 2
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(0, 2), f.Sourcecode);
        }

        //CopyPaste
        [Test]
        public void ReplaceLine_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "baba",
                Range = new Range
                {
                    Start = new Position { Line = 15, Character = 1 },
                    End = new Position { Line = 15, Character = 30 }
                },
                RangeLength = 29
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(215, 29).Insert(215, "baba"), f.Sourcecode);
        }
    }

    public class RequestExplicitlyMeasuredByDebugging_ExpectationExplicitlyTypedOut
    {
        private PhysicalFile f;

        private const string cleanSource = "class A {\r\n    var a:int;\r\n    constructor(){}\r\n}";

        [SetUp]
        public void InitializeFile()
        {
            f = new PhysicalFile()
            {
                Sourcecode = cleanSource
            };
        }

        [Test]
        public void HitReturn_2_8()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 2, Character = 7 },
                    End = new Position { Line = 2, Character = 8 }
                },
                RangeLength = 1
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var a:int;\r\n    contructor(){}\r\n}";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void HitDel_2_8()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 2, Character = 8 },
                    End = new Position { Line = 2, Character = 9 }
                },
                RangeLength = 1
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var a:int;\r\n    consructor(){}\r\n}";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void TypeAt_2_8()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position { Line = 2, Character = 8 },
                    End = new Position { Line = 2, Character = 8 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var a:int;\r\n    consatructor(){}\r\n}";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void InsertAt_2_8()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "abc",
                Range = new Range
                {
                    Start = new Position { Line = 2, Character = 8 },
                    End = new Position { Line = 2, Character = 8 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var a:int;\r\n    consabctructor(){}\r\n}";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void Replace()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "var b:string;",
                Range = new Range
                {
                    Start = new Position { Line = 1, Character = 4 },
                    End = new Position { Line = 1, Character = 14 }
                },
                RangeLength = 10
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var b:string;\r\n    constructor(){}\r\n}";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void Replace_All()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "//nix",
                Range = new Range
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 3, Character = 1 }
                },
                RangeLength = 49
            };
            f.Apply(change);

            const string expected = @"//nix";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void Cut_All()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position { Line = 0, Character = 0 },
                    End = new Position { Line = 3, Character = 1 }
                },
                RangeLength = 49
            };
            f.Apply(change);

            const string expected = @"";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void AppendNL()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "\r\n",
                Range = new Range
                {
                    Start = new Position { Line = 3, Character = 1 },
                    End = new Position { Line = 3, Character = 1 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var a:int;\r\n    constructor(){}\r\n}\r\n";

            Assert.AreEqual(expected, f.Sourcecode);
        }

        [Test]
        public void AppendText()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "//bla",
                Range = new Range
                {
                    Start = new Position { Line = 3, Character = 1 },
                    End = new Position { Line = 3, Character = 1 }
                },
                RangeLength = 0
            };
            f.Apply(change);

            const string expected = "class A {\r\n    var a:int;\r\n    constructor(){}\r\n}//bla";

            Assert.AreEqual(expected, f.Sourcecode);
        }
    }
}