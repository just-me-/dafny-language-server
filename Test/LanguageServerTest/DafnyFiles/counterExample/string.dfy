method floaty(in1: string) returns (out1: bool)
   ensures out1 == false
{
   out1 := "ab" == in1;
}