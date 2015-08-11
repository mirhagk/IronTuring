var stuff : array 1 .. 10 of array 1 .. 10 of int
var stuff2 : string
proc foo (a : int, b : int)
    put a + b
end foo
foo (1, 2)
var num := 10
if num = 5 then
    foo (5 + 5, 6 + 6)
elsif num > 5 then
    foo (5 + 5, num)
else
    foo (19 * 1023 * 10, 10 + 30 * 10 - 102 div 10)
end if
