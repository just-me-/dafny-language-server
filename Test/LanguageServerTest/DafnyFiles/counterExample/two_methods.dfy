method a(inp1: int) returns (more: int)
   ensures inp1 > more
{
   more := inp1 * 2;
}

method b(xx: int) returns (yy: int)
   ensures xx < yy
{
   yy := xx * 2;
   var asfdas := a(1);
   var xy := 12;
   var aa := xy;
}