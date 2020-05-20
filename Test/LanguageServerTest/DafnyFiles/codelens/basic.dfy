method MultipleReturns(inp1: int, inp2: int) returns (more: int, less: int)
{
   more := inp1 + inp2;
   less := inp1 - inp2;
}
class ClassA {
   constructor () { }
   method myMethod() { /* do something */ }
}

class ClassB {
   constructor () { }
   method myMethod() { /* do something */ }
}

method Main() {
   var myNumber := 1+2;
   var classA := new ClassA();
   classA.myMethod();
   var classB1 := new ClassB();
   var classB2 := new ClassB();
   classB1.myMethod();
   classB2.myMethod();
   classB2.myMethod();

   var more;
   var less;
   more, less := MultipleReturns(1,2);
   
   var abc := new ClassC();
   abc.ABC := 1;
}

class ClassC {
   var ABC: int;
   constructor () { }
   method myMethod() { /* do something */ }
}