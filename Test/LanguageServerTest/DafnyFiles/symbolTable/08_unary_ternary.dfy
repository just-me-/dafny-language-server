 method foo() returns (b:int) {
   b := 5;
    
   //negation expr
   b := -b; 

   //unary expr? wtf. how?
   b := b;

   //ternary expr
   b := if b<0 then -b else b;
}

