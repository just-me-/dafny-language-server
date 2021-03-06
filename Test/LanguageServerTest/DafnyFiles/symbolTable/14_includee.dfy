module M {
    function method addOne(i: nat) : nat {
        i + 1
    }

    class Counter {
        var count : nat;

        constructor(initial: nat)
        {
            count := initial;
        }
        method reset()
        modifies this
        {
            count := 0;
        }
        method increase()
        modifies this
        {
            count := addOne(count);
        }

        method get() returns (r: nat) {
            return count;
        }

        method write() {
            var c := get();
            print c, '\n';
        }
    }
}