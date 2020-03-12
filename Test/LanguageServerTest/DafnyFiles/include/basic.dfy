include "includee.dfy"
method m() {
    var c := new C();
    assert c.increase(4) == 6;
}
