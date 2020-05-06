method minimum(s: set<int>) returns (out: int)
requires |s| >= 1
ensures forall t : int :: t in s ==> out <= t
{
  var y :| y in s;
  if (|s| > 1) {
    var m := minimum(s - {y});
    out := (if y < m then y else m);
    assert forall t : int :: t in (s - {y}) ==> out <= t;
    assert out <= y;
  } else {
    assert |s| == 1;
    assert y in s;
    assert |s - {y}| == 0;
    assert s - {y} == {};
    assert s == {y};
    return y;
  }
}