
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
      //invariant 0 < i <= n
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