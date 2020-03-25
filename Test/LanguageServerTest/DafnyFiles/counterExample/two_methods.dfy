method m1(in1: int) returns (out1: int)
   ensures out1 < in1      //not true when in1 positive
{
   out1 := in1 * 2;
}

method m2(in2: int) returns (out2: int)
   ensures out2 > in2          //not true when in2 negative
{
   out2 := in2 * 2;
}