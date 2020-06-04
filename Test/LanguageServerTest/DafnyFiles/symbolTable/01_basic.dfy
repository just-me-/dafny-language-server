class MyClass {

    var field: int; 
	
    method  add(i: int, j: int) returns (r:int) {
       r := i + j;
       return r; 
    }
	
    method aMethod() modifies this { 
        var aLocalVar := 2;
        field := aLocalVar;  
        aLocalVar := add(field, 3);
    }

    constructor () { }
}