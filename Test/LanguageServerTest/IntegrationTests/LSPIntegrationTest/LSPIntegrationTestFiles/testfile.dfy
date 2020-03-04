method a(inp1: int) returns (more: int)
   ensures inp1 < more
{
   more := inp1 * 2;
   assert 1 == 2;


   //segment for goto definition
   var b := 0;
   var c := b + 2;
}