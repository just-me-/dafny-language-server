//Module Definitions

module M1 {
    class M1_Class {
        constructor(){}
        method M1_Class_Method() {}
    }

    method M1_Method() {}
}

module M2 {
        class M2_Class {
        constructor(){}
        method M2_Class_Method() {}
    }

    method M2_Method() {}
}

//Global Module Definitions

class GlobalClass {
    constructor(){}
    method Method() {}
}
method Global_Method() {}

//Tests

module ImportWithIdentifier {
    import Mod = M1
    method test1() {
        var m1 := new Mod.M1_Class();
        m1.M1_Class_Method();
        Mod.M1_Method();
    }
}
module ImportWithName {
    import M1
    method test2() {
        var m1 := new M1.M1_Class();
        m1.M1_Class_Method();
        M1.M1_Method();
    }

}

module ImportOpened{
    import opened M2
    method test3() {
        var m2 := new M2_Class();
        m2.M2_Class_Method();
        M2_Method();
    }
}

module ImportMultiple{
    import M1
    import M2
    method test3() {
        var m1 := new M1.M1_Class();
        var m2 := new M2.M2_Class();
        m2.M2_Class_Method();
        M1.M1_Method();
    }
}

module UsingGlobals{
    method test3() {
        //var g := new GlobalClass();
        //g.Method();
        //Global_Method();   //not supported by language
    }
}