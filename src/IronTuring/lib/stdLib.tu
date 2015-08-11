unit
module StdLib2
    export test
    var num := 0
    function test (i : int) : int
	num := num + 1
	if (num >= 5) then
	    result 5
	end if
	result 10
    end test
    put "creating module"
    %include "stdstuff"
end StdLib2

