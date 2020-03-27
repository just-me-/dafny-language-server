function TwoToThe( i : int ) : int
decreases i 
requires i >= 0
{
    if i==0 then 1 else 2*TwoToThe( i-1 )
}

method interestingBVD(x : int, y : int)
    requires y > 0 
    requires x >= 0
{
    var q := 1 ;
    var qy := y ; // Tracks q*y
    ghost var i := 0 ;
    // Double q until q*y exceeds x
    while( qy < x )  // Off by one error.
        invariant qy == q*y
        invariant q == TwoToThe(i)
        decreases 2*x-qy ;
    {
        q, qy, i := 2*q, 2*qy, i + 1 ;
    }
    // In the BVD we get actual numbers!!
    assert q*y == qy > x && q == TwoToThe(i) ; 
}