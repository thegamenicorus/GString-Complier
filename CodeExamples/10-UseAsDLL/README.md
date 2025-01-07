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
Store location - C:\Users\yolo\Desktop\GString-Complier\CodeExamples\10-UseAsDLL\area_calculator.dll

```

### Using area_calculator.dll with C#.NET

#### 👉 Attach area_calculator.dll to C# project.

<img width="790" alt="Screenshot 2025-01-07 at 1 35 32 PM" src="https://github.com/user-attachments/assets/f6dff6da-b157-42a0-9c0a-0594f6a1311b" />

#### 👉 Intellisense works!! ✌️

<img width="1019" alt="Screenshot 2025-01-07 at 1 47 23 PM" src="https://github.com/user-attachments/assets/6c052fa5-1c88-41b6-af2b-b0690401d99d" />


<img width="889" alt="Screenshot 2025-01-07 at 1 47 51 PM" src="https://github.com/user-attachments/assets/267cd7f1-805f-4403-909b-3dc01b514013" />

#### 👉 Let's compile and run!

```C#
var areaCalculator = new GString.Calculator.AreaCalculator();

var squareArea = areaCalculator.calculateSquareArea(30);
var rectangleArea = areaCalculator.calculateRectangleArea(30, 40);
var circleArea = areaCalculator.calculateCircleArea(30);
var sector = areaCalculator.calculateSectorArea(30, 90);
var ellipse = areaCalculator.calculateEllipseArea(30, 20);
var trapezoid = areaCalculator.calculateTrapezoidArea(30, 45, 20);
var triangle = areaCalculator.calculateTriangleArea(30, 45);

Console.WriteLine($"Square Area: {squareArea}");
Console.WriteLine($"Rectangle Area: {rectangleArea}");
Console.WriteLine($"Circle Area: {circleArea}");
Console.WriteLine($"Sector Area: {sector}");
Console.WriteLine($"Ellipse Area: {ellipse}");
Console.WriteLine($"Trapezoid Area: {trapezoid}");
Console.WriteLine($"Triangle Area: {triangle}");
```

#### Result
```
Square Area: 900
Rectangle Area: 1200
Circle Area: 2827.4333882308138
Sector Area: 706.8583470577034
Ellipse Area: 1884.9555921538758
Trapezoid Area: 750
Triangle Area: 675
```



