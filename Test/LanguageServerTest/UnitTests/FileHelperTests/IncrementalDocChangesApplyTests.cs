using System;
using DafnyLanguageServer.FileManager;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

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
        public void pos_0_0()
        {
            Position p = new Position(0, 0);
            var r = f.GetIndex(p);
            Assert.AreEqual(0, r);
        }

        [Test]
        public void pos_2_0()
        {
            Position p = new Position(2, 0);
            var r = f.GetIndex(p);
            Assert.AreEqual(4, r);
        }

        [Test]
        public void pos_2_1()
        {
            Position p = new Position(2, 1);
            var r = f.GetIndex(p);
            Assert.AreEqual(5, r);
        }

        [Test]
        public void pos_2_2()
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
            Assert.AreEqual(268, r);
        }

        [Test]
        public void Illegal_col_out_of_bounds()
        {
            Position p = new Position(1, 100);
            Assert.Throws<IndexOutOfRangeException>(() => f.GetIndex(p));

        }

        [Test]
        public void Illegal_line_out_of_bounds()
        {
            Position p = new Position(100, 0);
            Assert.Throws<IndexOutOfRangeException>(() => f.GetIndex(p));

        }

    }

    public class ApplyChangesTest
    {

        private PhysicalFile f;

        private const string cleanSource = @"

class MyClass {


    var field: int; 
	
	
    method  addOne(i: int) returns (r:int) {
       r := i + 1;
       return r; 
    }
    method aMethod() modifies this { 
        var aLocalVar := 2;
        field := aLocalVar;  
        aLocalVar := addOne(field);
    }



    constructor () { }

}

";

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
                    Start = new Position {Line = 0, Character = 0},
                    End = new Position {Line = 0, Character = 0}
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
                    Start = new Position {Line = 15, Character = 15},
                    End = new Position {Line = 15, Character = 15}
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource, f.Sourcecode);

        }



        //INSSERT
        [Test]
        public void Insert_one_char_at_0_0()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position {Line = 0, Character = 0},
                    End = new Position {Line = 0, Character = 0}
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual("a" + cleanSource, f.Sourcecode);
        }

        [Test]
        public void Insert_one_char_at_1_0()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position {Line = 1, Character = 0},
                    End = new Position {Line = 1, Character = 0}
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Insert(2, "a"), f.Sourcecode);
        }

        [Test]
        public void Insert_one_char_at_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "a",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 15},
                    End = new Position {Line = 15, Character = 15}
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Insert(259, "a"), f.Sourcecode);
        }

        [Test]
        public void Insert_text_at_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "abc",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 15},
                    End = new Position {Line = 15, Character = 15}
                },
                RangeLength = 0
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Insert(259, "abc"), f.Sourcecode);
        }


        //DELETE
        [Test]
        public void PressingDEL_at_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 15},
                    End = new Position {Line = 15, Character = 16}
                },
                RangeLength = 1
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(259, 1), f.Sourcecode);
        }

        [Test]
        public void PressingRETURN_at_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 14},
                    End = new Position {Line = 15, Character = 15}
                },
                RangeLength = 1
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(258, 1), f.Sourcecode);
        }


        [Test]
        public void Removing_Range_at_15_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 15},
                    End = new Position {Line = 15, Character = 20}
                },
                RangeLength = 5
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(259, 5), f.Sourcecode);
        }


        [Test]
        public void Removing_Line_15_But_Keep_Line_Alive()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 0},
                    End = new Position {Line = 15, Character = 35}
                },
                RangeLength = 35
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(244, 35), f.Sourcecode);
        }

        [Test]
        public void Removing_Line_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 0},
                    End = new Position {Line = 16, Character = 0}
                },
                RangeLength = 37
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(242, 37), f.Sourcecode);
        }

        [Test]
        public void Removing_Line_1()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 0, Character = 0},
                    End = new Position {Line = 1, Character = 0}
                },
                RangeLength = 2
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(0, 2), f.Sourcecode);
        }

        //CopyPaste
        [Test]
        public void Replace_Line_15()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "baba",
                Range = new Range
                {
                    Start = new Position {Line = 15, Character = 8},
                    End = new Position {Line = 15, Character = 35}
                },
                RangeLength = 27
            };
            f.Apply(change);

            Assert.AreEqual(cleanSource.Remove(252, 27).Insert(252, "baba"), f.Sourcecode);
        }



    }

    //these are the meaningful tests
    public class ApplyChanges_RequestExplicitlyMeasuredByDebugging_ExpectationExplicitlyTypedOut
    {

        private PhysicalFile f;

        private const string cleanSource = @"class A {
    var a:int;
    constructor(){}
}";

        [SetUp]
        public void InitializeFile()
        {
            f = new PhysicalFile()
            {
                Sourcecode = cleanSource
            };
        }


        [Test]
        public void Hit_Return_2_8()
        {
            TextDocumentContentChangeEvent change = new TextDocumentContentChangeEvent
            {
                Text = "",
                Range = new Range
                {
                    Start = new Position {Line = 2, Character = 7},
                    End = new Position {Line = 2, Character = 8}
                },
                RangeLength = 1
            };
            f.Apply(change);

            string expected = @"class A {
    var a:int;
    contructor(){}
}";

            Assert.AreEqual(expected, f.Sourcecode);

        }

        [Test]
        public void Hit_Del_2_8()
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

            string expected = @"class A {
    var a:int;
    consructor(){}
}";

            Assert.AreEqual(expected, f.Sourcecode);

        }

        [Test]
        public void Type_at_2_8()
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

            string expected = @"class A {
    var a:int;
    consatructor(){}
}";

            Assert.AreEqual(expected, f.Sourcecode);

        }

        [Test]
        public void Insert_at_2_8()
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

            string expected = @"class A {
    var a:int;
    consabctructor(){}
}";

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
                    Start = new Position { Line = 1, Character = 4},
                    End = new Position { Line = 1, Character = 14 }
                },
                RangeLength = 10
            };
            f.Apply(change);

            string expected = @"class A {
    var b:string;
    constructor(){}
}";

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

            string expected = @"//nix";

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

            string expected = @"";

            Assert.AreEqual(expected, f.Sourcecode);

        }

        [Test]
        public void Append_NL()
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

            string expected = @"class A {
    var a:int;
    constructor(){}
}
";

            Assert.AreEqual(expected, f.Sourcecode);

        }

        [Test]
        public void Append_Text()
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

            string expected = @"class A {
    var a:int;
    constructor(){}
}//bla";

            Assert.AreEqual(expected, f.Sourcecode);

        }
    }
}