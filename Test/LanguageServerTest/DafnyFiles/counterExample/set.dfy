method floaty(in1: set<int>) returns (out1: set<int>)
   ensures out1 < in1
{
   out1 := in1 + {2};
}