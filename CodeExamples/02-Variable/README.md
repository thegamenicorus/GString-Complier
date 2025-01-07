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
---
### 📝 variables_float_double.gstr
In this compiler, float and double cannot be used directly as literals (e.g., 1.10). Instead, we must use the Parse method, as shown in the example.

```
Class: {Application}.
    Main.
        Declare pi as float, value: {
            (float, call method: {Parse, 
                arguments:{
                    "3.14159265"
                }
            })
        }.
        Declare electron_g_factor as double, value: {
            (double, call method: {Parse, 
                arguments:{
                    "-2.00231930436092"
                }
            })
        }.

        Display with line: "Pi ~ " { pi }.
        Display with line: "Pi/3 ~ " { pi/3 }.
        Display with line: "Pi/3.5 ~ " { 
            pi / 
                (float, call method: {Parse, 
                        arguments:{
                            "3.5"
                        }
                    }
                )
        }.
        Display with line: "Electron g-factor = " { electron_g_factor }.

        ** Array.
        Declare double_arr as array of double, size:{5}, value: {
            [
                (double, call method: {Parse, 
                    arguments:{
                        "1.11111"
                    }
                }),
                (double, call method: {Parse, 
                    arguments:{
                        "2.22222"
                    }
                }),
                (double, call method: {Parse, 
                    arguments:{
                        "3.33333"
                    }
                }),
                (double, call method: {Parse, 
                    arguments:{
                        "4.44444"
                    }
                }),
                (double, call method: {Parse, 
                    arguments:{
                        "5.55555"
                    }
                }),
            ]
        }.

        Declare double_from_array_0, value: { double_arr, position:{0} }.

        Display with line: "double_arr[1] = " { double_arr, position:{1} }.
        Display with line: "double_arr[3] = " { double_arr, position:{3} }.
        Display with line: "double_arr[4]/2 = " { double_arr, position:{4} / 2 }.
        Display with line: "double_from_array_0 = " { double_from_array_0 }.

    End method.
End class.
```
#### Result:
```
Pi ~ 3.141593
Pi/3 ~ 1.047198
Pi/3.5 ~ 0.8975979
Electron g-factor = -2.00231930436092
double_arr[1] = 2.22222
double_arr[3] = 4.44444
double_arr[4]/2 = 2.777775
double_from_array_0 = 1.11111
```
