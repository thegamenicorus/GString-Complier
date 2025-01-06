## Condition!

### 📝 operations.gstr

```
Class: {Application}.
    Main.
        Declare num as int, value:{Read input, "Input number: "}.

        If (num > 0):
            Display: "Positive and ".

            If (num % 2 == 0):
                Display: "even".
            Else:
                Display: "odd".
            End condition.

        Else if (num < 0):
            Display: "Negative and ".
            
            If (num % 2 == 0):
                Display: "even".
            Else:
                Display: "odd".
            End condition.

        Else:
            Display: "It's zero!".
        End condition. 
    End method.
End class.
```

#### Result:
```
Input number: 0
It's zero!
```

```
Input number: -91
Negative and odd
```

```
Input number: 22
Positive and even
```
