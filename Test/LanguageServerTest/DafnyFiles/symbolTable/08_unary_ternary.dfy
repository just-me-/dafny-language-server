 method foo() returns (b:int) {
   b := 5;
   
   //unary expr
   b := -b;

   //ternary expr
   b := if b<0 then -b else b;
}