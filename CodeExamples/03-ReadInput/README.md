## Read user's input!

### 📝 read_input.gstr

```
Class: {Application}.
    Main.
        ** Declare.
        Declare string_from_input as string.

        ** Read input.
        Assign to string_from_input, value:{Read input, "Input your string: "}.

        ** Display.
        Display with line: "Your string is -- " {string_from_input}.
    End method.
End class.
```

#### Result:
```
Input your string: Hello from Sydney!
Your string is -- Hello from Sydney!
```
---
### 📝 read_input_inline.gstr
```
Class: {Application}.
    Main.
        ** Read input in-line and display.
        Display with line: "My name is " {Read input, "Your name: "} ", I am " {Read input, "Age: "} " years old.".
    End method.
End class.
```

#### Result:
```
Your name: Bob
Age: 35
My name is Bob, I am 35 years old.
```
