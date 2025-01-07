# GString Compiler

## 🚧 🚨 Disclaimer 🚨 🚧
This project was created as a learning exercise and is NOT intended for production use. It is full of limitations and is not actively maintained or developed further. 

I have uploaded this project for educational purposes, so it may be helpful for anyone interested in studying compiler construction on the .NET framework. Please use it accordingly and understand its limitations.

## Introduction
Welcome to **GString Compiler**! This project is a personal hobby project aimed at exploring and understanding how to build a compiler on the .NET framework. 

The project is heavily inspired by the [GoodForNothing Compiler](https://learn.microsoft.com/en-us/archive/msdn-magazine/2008/february/create-a-language-compiler-for-the-net-framework-using-csharp) by Joel Pobar. 

**GString Compiler**'s name is derived from "*Game's Strings*", where *Game* is my nickname. Thus, the full concept behind this name represents "The set of Game's characters."

## Limitations
#### - Primitive types: 
GString Compiler currently supports:
- `int`, `short`, `long`, `string`, and `boolean` types.
- [Partial support](https://github.com/thegamenicorus/GString-Complier/blob/main/CodeExamples/02-Variable/README.md#-variables_float_doublegstr) for `float` and `double` types.
- Not support multi-dimensional array.

#### - Reflection.Emit API
This project relies heavily on the **Reflection.Emit** API, which works fine on .NET Framework 4.8. However, many significant changes have been made to the Reflection.Emit API in .NET Core and subsequent versions. Therefore, this project has not been upgraded to the latest .NET version.

#### - Incomplete functionality  😳
As mentioned earlier, this project is just a hobby and not intended for production use. It may have limited features, incomplete functionality, and many undiscovered bugs 🐞. Use it for learning, fun, or experiments, but avoid relying on it for critical or professional purposes! See the [Code examples](https://github.com/thegamenicorus/GString-Complier/tree/main?tab=readme-ov-file#code-examples) section for further exploration. 

## Usage
```
>> gstrc [-dll] <file.gstr>
```

Example:
```
>> gstrc .\bubble_sort.gstr

Compile result - Success
Compile time - 125 ms
Store location - C:\Users\yolo\Desktop\GString-Complier\CodeExamples\07-BubbleSort\bubble_sort.dll
```

Or compile to .dll
```
>> gstrc .\area_calculator.gstr -dll

Compile result - Success
Compile time - 94 ms
Store location - C:\Users\yolo\Desktop\GString-Complier\CodeExamples\10-UseAsDLL\area_calculator.dll
```

## Code examples
- [Hello world](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/01-HelloWord) - Display text output to the screen.
- [Variables](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/02-Variable) - Define and assign values to variables.
- [Read input](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/03-ReadInput) - Receive user input from the console.
- [Operations](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/04-Operations) - Perform basic arithmetic operations (addition, subtraction, multiplication, division, modulus).
- [Condition](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/05-Condition) - Implement logic using If, Else if, and Else statements.
- [Loop](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/06-Loop) - Execute repetitive actions using `Repeat since` and `Repeat while` loops.
- [Bubble sort](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/07-BubbleSort) - Use previous concepts to create a basic sorting algorithm.
- [Method](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/08-Method) - Define and invoke methods.
- [Namespace Class and Modifiers](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/09-Namespace_Class_Method_Modifiers) - Learn about namespaces, classes, and access modifiers.
- [Use as DLL](https://github.com/thegamenicorus/GString-Complier/tree/main/CodeExamples/10-UseAsDLL) - Compile GString code as a DLL and integrate it with other .NET-based languages.

## Contributing
This project is discontinued and not open for contributions at the moment. However, feel free to fork the repository and explore on your own.

---

Thank you for checking out GString Compiler! I hope it provides value to those learning about compilers on the .NET framework. 🙏



