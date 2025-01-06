## Bubble sort!

### 📝 bubble_sort.gstr

```
Class: {Application}.
    Main.
        Declare arr_size as int, value:{Read input,"Input array size: "}.
        Declare int_arr as array of int, size:{arr_size}.

        Repeat since i:{0 to before arr_size}, increase by:{1}:
        Assign to int_arr, position:{i}, value:{Read input,"Number " {i+1} ": "}.
        End loop.

        Declare int_temp as int.

        Repeat since i:{1 to {arr_size-1}}, increase by:{1}:
            Repeat since j:{0 to {arr_size-2}}, increase by:{1}:
                If int_arr, position:{j} > int_arr, position:{j+1}:
                    Assign to int_temp, value:{int_arr,position:{j}}.
                    Assign to int_arr, position:{j} , value:{int_arr, position:{j+1}}.
                    Assign to int_arr, position:{j+1}, value:{int_temp}.
                End condition.
            End loop.
        End loop.

        Repeat since i:{0 to before arr_size}, increase by:{1}:
            Display: {int_arr, position:{i}} " ".
        End loop.
    End method.
End class.
```

#### Result:
```
Input array size: 8
Number 1: 2
Number 2: 88
Number 3: 34
Number 4: -3
Number 5: 55
Number 6: -82
Number 7: 111
Number 8: 0
-82 -3 0 2 34 55 88 111
```
