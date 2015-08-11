var num := 5 + 5 - 5 * 5
put num, " ", 6
/*var name : string
var price : real := 5.01
var things : array 0 .. 9 of array 0 .. 9 of int
put price, " " ..
price := price * 2.0
put price*/

function fact (num : int) : int
    if (num <= 1) then
		result 1
    end if
    result num * fact (num - 1)
end fact
function square (num : int, unused : real) : int
    result num * num
end square

function doStuff : int
    put "stuff"
    result 0
end doStuff

/*put square (5, 5.001)*/
put fact (6)
