function isEven(i: int):bool {
    i%2==0
}

predicate isPos(i: int) {
    i>=0
}

function method isOdd(i: int) : bool {
    i%2 == 1
}  

method foo(i: int) returns (r: int) 
requires i >= 0
requires isOdd(i) 
ensures isEven(r) 
{
    r:= i + 1;
    assert r > 0;
}

class A{
    method foo()
    modifies this { 
        var i:= 0;
        
        while ( i < 10)
        invariant isPos(i)
        invariant i >= 0
        decreases 10-i
        {
            i := i+1; 
        }

        while (false) {} 
    } 
}