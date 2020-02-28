method MultipleReturns(x: int, y: int) returns (more: int, less: int)
   requires 0 < y            //einkommentieren -> fehler postcondition geht weg
   ensures less < x < more
{
   more := x + y;
   less := x - y;
   //assert x == 1;              //auskommentieren -> assertion violation geht weg
   bruder := 1;
}


method Main() {
   var a := 1+2;
   print "a is ";
   print a;
}