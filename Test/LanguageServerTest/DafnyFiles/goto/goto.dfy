method MultipleReturns(inp1: int, inp2: int) returns (more: int, less: int)
   requires inp2 > 0
   ensures less < inp1 < more
{
   more := inp1 + inp2;
   less := inp1 - inp2;
   assert more == 0;
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
   var classB := new ClassB();
   classB.myMethod();

   var more;
   var less;
   more, less := MultipleReturns(1,2);
}