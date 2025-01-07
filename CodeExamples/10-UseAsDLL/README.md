## Use as DLL!

### 📝 area_calcularor.gstr

This example demonstrates how to compile and use a GString DLL file with other languages on the .NET Framework. The provided GString code includes various methods for shape calculations. We will compile it into a .dll file and use it as a library in C#.

```
Namespace: {GString.Calculator}.
    Class: {AreaCalculator}, modifiers:{Public}.
        
        Method: {calculateSquareArea}, modifiers:{Public}, return type:{ double }, parameters:{length as double}.
                Return: { length * length }.
        End method.

        Method: {calculateRectangleArea}, modifiers:{Public}, return type:{ double }, parameters:{length as double, width as double}.
                Return: { length * width }.
        End method.

        Method: {calculateCircleArea}, modifiers:{Public}, return type:{ double }, parameters:{radius as double}.
            Declare pi as double, value: { double, call method:{Parse, arguments:{"3.1415926535897931"}}  }.
            Return: { pi * radius * radius }.
        End method.

        Method: {calculateSectorArea}, modifiers:{Public}, return type:{ double }, parameters:{radius as double, angle as double}.
            Declare pi as double, value: { double, call method:{Parse, arguments:{"3.1415926535897931"}}  }.
            Declare const_angle as double, value: { double, call method:{Parse, arguments:{"360"}}  }.
            Return: { angle / const_angle *  pi * radius * radius }.
        End method.

        Method: {calculateEllipseArea}, modifiers:{Public}, return type:{ double }, parameters:{semi_major_axes as double, semi_minor_axes as double}.
            Declare pi as double, value: { double, call method:{Parse, arguments:{"3.1415926535897931"}}  }.
            Return: { pi * semi_major_axes * semi_minor_axes }.
        End method.

        Method: {calculateTrapezoidArea}, modifiers:{Public}, return type:{ double }, parameters:{base_1 as double, base_2 as double, height as double}.
            Declare const_base_avg as double, value: { double, call method:{Parse, arguments:{"2"}}  }.
            Return: { (base_1 + base_2) / const_base_avg * height }.
        End method.

        Method: {calculateTriangleArea}, modifiers:{Public}, return type:{ double }, parameters:{base as double, height as double}.
            Declare const_formula as double, value: { double, call method:{Parse, arguments:{"0.5"}}  }.
            Return: { const_formula * base * height }.
        End method.

    End class.
End namespace.
```

### Compile to area_calculator.dll
```
> gstrc .\area_calculator.gstr -dll

Compile result - Success
Compile time - 94 ms
Store location - C:\Users\yolo\Desktop\GString-Complier\CodeExamples\09-Namespace_Class_Method_Modifiers\modifiers.exe

```

