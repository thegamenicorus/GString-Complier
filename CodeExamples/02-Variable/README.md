## Declare and assign variables!

### 📝 variables.gstr

```
Class: {Application}.
    Main.
        ** Declare.
        Declare var1 as string.
        Declare var2 as bool.
        Declare xx as double.

        ** Assign.
        Assign to var1, value: {"Hello"}.
        Assign to var2, value: {true}.

        ** Declare and assign.
        Declare var3 as int, value:{1234}.


        ** Display.
        Display with line: "var1 is " {var1}.
        Display with line: "var2 is " {var2}.
        Display with line: "var3 is " {var3}.
    End method.
End class.
```

#### Result:
```
var1 is Hello
var2 is True
var3 is 1234
```
---
### 📝 variables_array.gstr

```
Class: {Application}.
    Main.
        Declare int_arr as array of int, size:{5}, value:{[ 10, 20, 30, 40, 50 ]}.
        Declare str_arr as array of string, size:{5}, value:{[ "Alpha", "Bravo", "Charlie" , "Delta", "Echo" ]}.

        Display with line: "int_arr[2] = " { int_arr, position:{2} }.
        Display with line: "int_arr[4] = " { int_arr, position:{4} }.
        Display with line: "str_arr[1] = " { str_arr, position:{1} }.
        Display with line: "str_arr[3] = " { str_arr, position:{3} }.
    End method.
End class.
```

#### Result:
```
int_arr[2] = 30
int_arr[4] = 50
str_arr[1] = Bravo
str_arr[3] = Delta
```
