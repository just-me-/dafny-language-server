method a(inp1: int) returns (more: int)
   ensures inp1 < more
{
   more := inp1 * 2;
}

method b(xxxxxxxxxxxxxxx: int) returns (yyyyyyyyyyyyyyyyyy: int)
   ensures xxxxxxxxxxxxxxx < yyyyyyyyyyyyyyyyyy
{
   yyyyyyyyyyyyyyyyyy := xxxxxxxxxxxxxxx * 2;
   var asfdas := a(1);
   var xy := 12;
   var aa := xy;
   assert 1==2;
}