trait Base1 {
    var baseVar1 : int;
    method baseMethod1()
    modifies this {
        baseVar1 := 2;
    }
}

trait Base2 {
    var baseVar2 : int;
    method baseMethod2()
    modifies this {
        baseVar2 := 2;
    }
}

class Sub extends Base1, Base2 {
    var subVar : int;
    method subMethod()
    modifies this {
        baseVar1 := 1;
        baseVar2 := 2;
        subVar := 3;

        baseMethod1();
        this.baseMethod1();
        
        baseMethod2();
        this.baseMethod2();
    }
}

class Sub2 extends Base2 {
    method foo() {
        baseVar2 := 7;  
        baseMethod2();
    }
}
 
method globalMethod() { 
    var baseVar1 := 4;
    var baseVar2 := 4;
    var subVar := 6;
}