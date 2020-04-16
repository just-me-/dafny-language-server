include "include_includee.dfy"

module MainModule {
    import H = Helpers;
    method Main() {
        var bigNumber := H.addOne(2);
        assert bigNumber == 3;
        var b:int;
    }
}