## Operations!

### 📝 operations.gstr

```
Class: {Application}.
    Main.
        Declare num1 as int, value:{Read input, "Input first number: "}.
        Declare num2 as int, value:{Read input, "Input second number: "}.

        Declare add as int, value: {num1 + num2}.
        Declare subtract as int, value: {num1 - num2}.
        Declare muliply as int, value: {num1 * num2}.
        Declare division as int, value: {num1 / num2}.
        Declare mix_ops as int, value: {num1 + 10 * num2 / 3 - 15 + muliply * division}.

        Display with line: "num1 + num2 = " {add}.
        Display with line: "num1 - num2 = " {subtract}.
        Display with line: "num1 * num2 = " {muliply}.
        Display with line: "num1 / num2 = " {division}.
        Display with line: "num1 % num2 = " {num1 % num2}. ** Perform mod operation and display.
        Display with line: "Mix operations = " {mix_ops}.
    End method.
End class.
```

#### Result:
```
Input first number: 10
Input second number: 3
num1 + num2 = 13
num1 - num2 = 7
num1 * num2 = 30
num1 / num2 = 3
num1 % num2 = 1
Mix operations = 95
```
