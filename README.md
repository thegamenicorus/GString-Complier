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
- `int`, `string`, and `boolean` types.
- Partial support for `float` and `double` types.

#### - Reflection.Emit API
This project relies heavily on the **Reflection.Emit** API, which works fine on .NET Framework 4.8. However, many significant changes have been made to the Reflection.Emit API in .NET Core and subsequent versions. Therefore, this project has not been upgraded to the latest .NET version.


## Usage
```
>> gstrc [-dll] <file.gstr>
```

Example:
```
>> gstrc .\bubble_sort.gstr

Compile result - Success
Compile time - 125 ms
Store location - C:\Users\yolo\Desktop\GStringExamples\bubble_sort.exe
```

## Contributing
This project is discontinued and not open for contributions at the moment. However, feel free to fork the repository and explore on your own.

---

Thank you for checking out GString Compiler! I hope it provides value to those learning about compilers on the .NET framework. 🙏



