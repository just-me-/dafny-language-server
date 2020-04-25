//this file crashes the language server... why??
//iwo stack overflow - evtl visitor loop bei den statments unten - ma schauen.

method IAmTestNo7() {
   var a: bool := true;
   var b: nat := 2;

   //if
   if (!true || (a && b==2) ) {
      var a := 12;
   } else if (false) {
      print a;
   } else {
      print b;
   }

   //while
   while  (b < 10)
   decreases 10-b
   {
     b := b+1;
   }


}