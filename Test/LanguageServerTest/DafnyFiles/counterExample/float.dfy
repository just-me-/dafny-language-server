method floaty(inp1: real) returns (out1: real) //this example is only true if input is between -0.5 und 0.5
   ensures -1.0 <= out1 <= 1.0
{
   out1 := 2.0 * inp1;
} 