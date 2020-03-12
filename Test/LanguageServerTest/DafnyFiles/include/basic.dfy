include "includee.dfy"
method m() {
    var c := new C();
    assert c.increase(5) == 6;
}
