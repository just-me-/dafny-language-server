method Demo(x: int, y: int) returns (more: int, less: int)
   requires y > 0
   ensures less < x < more
{
   more := x + y;
   less := x - y;
}

