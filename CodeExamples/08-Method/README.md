## Method!

### 📝 method.gstr

```
Class: {Application}.
    Main.
        ** Invoke method SumAndPrint(string, string).
        Call method: {SumAndPrint, arguments:{
                (Read input,"input num1: "),
                (Read input,"input num2: ")
            }
        }.
    End method.
    
    ** Define SumAndPrint method.
    ** In C# will be: public static void SumAndPrint(string num1, string num2).
    Method: {SumAndPrint}, modifiers:{Public,Static}, return type:{void}, parameters:{ num1 as string, num2 as string }.
        ** Parse both num1 and num2 to integer and invoke SumAndPrint(int num1, int num2).
        Call method: {SumAndPrint, arguments:{
            (int, call method:{Parse,arguments:{num1}}),
            (int, call method:{Parse,arguments:{num2}})
            }
        }.
    End method.

    ** Overload SumAndPrint method.
    ** In C# will be: public static void SumAndPrint(int num1, int num2).
    Method: {SumAndPrint}, modifiers:{Public,Static}, return type:{void}, parameters:{ num1 as int, num2 as int }.
        ** Display num1 + num2.
        Display with line: "Sum of " {num1} " and " {num2} " is "
        {   
            num1 + num2
        }.
    End method.
End class.
```

#### Result:
```
input num1: 9119
input num2: 1991
Sum of 9119 and 1991 is 11110
```
