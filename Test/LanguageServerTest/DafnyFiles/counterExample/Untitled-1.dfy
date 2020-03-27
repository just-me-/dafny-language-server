method Abs(x: int)  returns (y: int)
ensures y >= 0 && (y == x || y == -x) {
    if (x < 0) {return -x;}
    else {return x;}
}


method Abs_weirdo(x: int) returns (y: int)
requires x < 0
ensures y > 0 
{ return -x; }



method Abs_weirdorama(x: int) returns (res: int) 
    requires x > -2
    ensures res > 0
{
    return x+2;
}

method MultipleReturns(x: int, y: int) returns (more: int, less: int)
   requires y > 0
   ensures less < x < more
{
   more := x + y;
   less := x - y;
}


method Max(a: int, b:int) returns (c: int)
ensures a > b ==> c == a
ensures b > a ==> c == b
{
    if (a>b) {return a;}
    else {return b;}
}

method Testing() {
    var x : int  :=  Max(5,3);
    assert x == 5;
}




//functions haben genau ein statement

function max(x: int, y: int): int
{
   if x > y then x else y
}


//function sind limitierter, können aber in assert und in sich selbst direkt genutzt werden
method Testing2()
{
    assert max(2, 9) == 9; //ohne var
}




function fib(n: nat): nat
decreases n //hilft dafny die termination zu prooven
{
   if n == 0 then 0 else
   if n == 1 then 1 else
                  fib(n - 1) + fib(n - 2)
}

method ComputeFib(n: nat) returns (b: nat)
   ensures b == fib(n)
{
 if n == 0 { return 0; }
   var i := 1;
   var a := 0;
       b := 1;
   while i < n
      invariant i !=n+1
      invariant a == fib(i - 1)
      invariant b == fib(i)
      decreases n-i //distanz i<->n wird immer kleiner
   {
      a, b := b, a + b;
      i := i + 1;
   }
   return b;

}


method Find(a: array<int>, key: int) returns (index: int)
   ensures 0 <= index ==> index < a.Length && a[index] == key
   ensures index < 0 ==> forall k :: 0 <= k < a.Length ==> a[k] != key
{
   index := 0;
   while index < a.Length
      invariant 0 <= index <= a.Length
      invariant forall k :: 0 <= k < index ==> a[k] != key
          //für alle k zwischen 0 bis index gilt...
      decreases a.Length - index
   {
      if a[index] == key { return; }
      index := index + 1;
   }
   index := -1;
}