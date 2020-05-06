function square(n: int): int
{
    n * n
}

method sqrt(n : nat) returns (r: int)
  // square less than or equal to n
  ensures r * r <= n
  // largest number
  ensures forall i :: 0 <= i < r ==> square(i) < r * r
{
    var i := 0; // increasing number
    r := 0;
    while i * i <= n
      invariant r * r <= n
      invariant forall k :: 0 <= k < r ==> square(k) < r * r 
      decreases n - i
    {
      r := i;
      i := i + 1;
    }

    return r;
}