method a(inp1: int) returns (more: int)
   ensures inp1 < more
{
   more := inp1 * 2;
   assert 1 == 2;
   int b = 0;
   int c = b + 2;
}