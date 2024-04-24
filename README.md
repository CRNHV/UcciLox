# UcciLox

This is my implementation of the Lox interpreter from [craftinginterpreters.com](https://craftinginterpreters.com).
There's no support for classes or closures as I wasn't feeling like adding them. Also, the script is currently hardcoded to `/LoxScripts/LoadLib.lox`

I tried to add support for calling WinAPI functions via [`GetModuleHandle`](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/GetModHandle.cs) and [`GetProcAddress`](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/GetProcAddr.cs) [but unfortunately it didn't work out the way I wanted to.](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/WinAPIFunc.cs)

So in the LoadLib.lox script I wanted to have something like this and be able to call the MessageBoxA function:
```
var user32lib = GetModuleHandle("User32.dll");
var msgBoxProc = GetProcAddress(user32lib, "MessageBoxA");
WinAPIFunc(msgBoxProc, "Text", "Caption", 0);
```
but this ideally would've been used for any function.

In C# land we have to declare the function as a delegate and can't really build that dynamically:
```csharp
delegate void NativeFunction(IntPtr handle, string text, string caption, int type);
```
So currently The `WinAPIFunc` class is kind of hardcoded for `MessageBoxA`
```csharp
public object? Call(Interpreter interpreter, List<object> arguments)
{
    nint nativeFunc = (nint)arguments[0];
    object[] args = arguments.Skip(1).ToArray();

    NativeFunction myFunction = (NativeFunction)Marshal.GetDelegateForFunctionPointer(nativeFunc, typeof(NativeFunction));
    myFunction(nint.Zero, (string)args[0], (string)args[1], 0);
    return null;
}
```
