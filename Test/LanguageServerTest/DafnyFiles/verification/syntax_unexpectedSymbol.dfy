method m1(in1: int) returns (out1: int)
   ensures in1 > out1
{
   var a := new XXX();
}

method m2(in2: int) returns (out2: int)
   ensures in2 < out2
{
   out2 := in2 * 2;
}