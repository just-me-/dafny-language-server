//Module Definitions
include "14_includee.dfy"

method Main() {
    var myCounter := new M.Counter(100);
    myCounter.write();
    myCounter.increase();
    myCounter.increase();
    myCounter.write(); 
    myCounter.reset();
    myCounter.write();
}