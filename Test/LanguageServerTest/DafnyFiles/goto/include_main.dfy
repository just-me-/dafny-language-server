include "include_includee.dfy"
method Main() {
    var c := new C();
    assert c.increase(5) == 6;
}
