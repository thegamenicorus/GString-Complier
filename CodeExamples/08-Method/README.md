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
---
### 📝 method_return.gstr

```
Class: {Application}.
    Main.
        Display with line: "Return from GetInt: " {Call method:{GetInt}}.
        Display with line: "Return from GetString: " {Call method:{GetString}}.
        
        ** Read input -> int.Parse() -> invoke AddByGetInt().
        Display with line: "Return from AddByGetInt: " {
            Call method:{AddByGetInt, 
                arguments:{
                    (int, 
                        call method:{ Parse,
                            arguments:{
                                (Read input, "Your number: ")
                            }
                        }
                    )
                }
            }
        }.

        ** Read input -> invoke Greeting().
        Display with line: "Return from Greeting: " {
            Call method:{Greeting, 
                arguments:{ (Read input, "Your message: ") }
            }
        }.
    End method.
    
    ** Define GetInt method.
    Method: {GetInt}, modifiers:{Public,Static}, return type:{int}.
        Return: {50}.
    End method.

    ** Define GetString method.
    Method: {GetString}, modifiers:{Public,Static}, return type:{string}.
        Return: {"Hello there."}.
    End method.

    ** Define AddByGetInt method.
    Method: {AddByGetInt}, modifiers:{Public,Static}, return type:{int}, parameters:{num as int}.
        Return: {num + (Call method:{GetInt})}.
    End method.

    ** Define Greeting method.
    Method: {Greeting}, modifiers:{Public,Static}, return type:{string}, parameters:{message as string}.
        Declare greeting_message as string, value: {Call method:{GetString} + " " + message}.
        Return: { greeting_message }.
    End method.
End class.
```

#### Result:
```
Return from GetInt: 50
Return from GetString: Hello there.
Your number: 99
Return from AddByGetInt: 149
Your message: I am Jimmy!
Return from Greeting: Hello there. I am Jimmy!
```
---
### 📝 method_pass_by_ref.gstr
```
Class: {Application}.
    Main.
        Declare num as int, value: {
            (int, 
                call method:{ Parse,
                    arguments:{
                        (Read input, "Number to x2: ")
                    }
                }
            )
        }.

        Declare text as string, value: {Read input, "Text to cut space: "}.
        
        Display with line: "".
        Display with line: "- Invoke DoubleNum...".
        Call method: {DoubleNum, 
            arguments: {
                (pass by reference)num,
            }
        }.
        Display with line: "Current num = " {num}.
        Display with line: "Current text = " {text}.

        Display with line: "".
        Display with line: "- Invoke MakeItCute...".
        Call method: {MakeItCute, 
            arguments: {
                (pass by reference)text,
            }
        }.
        Display with line: "Current num = " {num}.
        Display with line: "Current text = " {text}.
    End method.
    
    ** Define DoubleNum, make the number twice.
    Method: {DoubleNum}, 
        modifiers:{Public,Static},
        return type:{void}, 
        parameters:{ 
            (pass by reference)num as int
        }.
        Assign to num, value: {num * 2}.
    End method.

    ** Define MakeItCute, Concat text with a cat face and sound fx.
    Method: {MakeItCute}, 
        modifiers:{Public,Static},
        return type:{void}, 
        parameters:{ 
            (pass by reference)text as string
        }.
        Assign to text, value: {text + " (^._.^) Meow ~~  "}.
    End method.
End class.
```

#### Result:
```
Number to x2: 82
Text to cut space: OH Noooooooo!

- Invoke DoubleNum...
Current num = 164
Current text = OH Noooooooo!

- Invoke MakeItCute...
Current num = 164
Current text = OH Noooooooo! (^._.^) Meow ~~
```
