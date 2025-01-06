## Loop!

### 📝 loop.gstr

```
Class: {Application}.
    Main.
        Declare array_size as int, value:{Read input,"Input round: "}.
        Declare string_array as array of string, size:{array_size}.
        Declare loop_count as int, value:{1}.


        ** For loop, use Repeat since <variable> to assign value to string_array.
        Repeat since index:{0 to before array_size},increase by:{loop_count}:
            Assign to string_array, position:{index}, value:{Read input, "Input value of string_array[" {index} "]: "}.
        End loop.

        ** While loop, use Repeat while to read value from string_array.
        Repeat while loop_count <= array_size:
            Display with line: "Value of string_array[" {loop_count - 1} "]: "{string_array, position:{loop_count - 1}}.
            Assign to loop_count, value:{loop_count + 1}.
        End loop.
    End method.
End class.
```

#### Result:
```
Input round: 5
Input value of string_array[0]: This
Input value of string_array[1]: is
Input value of string_array[2]: the
Input value of string_array[3]: GString
Input value of string_array[4]: loop!
Value of string_array[0]: This
Value of string_array[1]: is
Value of string_array[2]: the
Value of string_array[3]: GString
Value of string_array[4]: loop!
```
