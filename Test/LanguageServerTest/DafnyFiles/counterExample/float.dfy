method floaty(inp1: real) returns (out1: real)
   ensures -1.0 < out1 < 1.0
{
   out1 := 2.0 * inp1;
}