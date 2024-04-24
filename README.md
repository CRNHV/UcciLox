# UcciLox

This is my implementation of the Lox interpreter from [craftinginterpreters.com](https://craftinginterpreters.com).
There's no support for classes or closures as I wasn't feeling like adding them.

I tried to add support for calling WinAPI functions via [`GetModuleHandle`](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/GetModHandle.cs) and [`GetProcAddress`](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/GetProcAddr.cs) [but unfortunately it didn't work out the way I wanted to.](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/WinAPIFunc.cs)

As previously said, the goal was to be able to call any WinAPI function but C#'s type safety kind of prevents doing that in a dynamic way. 
I wanted to do something like:
```
var user32lib = GetModuleHandle("User32.dll");
var msgBoxProc = GetProcAddress(user32lib, "MessageBoxA");
WinAPIFunc(msgBoxProc, "Text", "Caption", 0);
```
but in C# land we have to declare the function as a delegate and can't really build that dynamically:
```csharp
delegate void NativeFunction(IntPtr handle, string text, string caption, int type);
```

So currently The `WinAPIFunc` class is kind of hardcoded for `MessageBoxA`

```csharp
public object? Call(Interpreter interpreter, List<object> arguments)
{
    // The goal was to be able to call WinAPI functions without knowing what the
    // Function signature looks like in advance. Turns out that's more difficult
    // Than I thought, sad stuff. 

    nint nativeFunc = (nint)arguments[0];
    object[] args = arguments.Skip(1).ToArray();

    NativeFunction myFunction = (NativeFunction)Marshal.GetDelegateForFunctionPointer(nativeFunc, typeof(NativeFunction));
    myFunction(nint.Zero, (string)args[0], (string)args[1], 0);
    return null;
}
```
