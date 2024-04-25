# UcciLox

This is my implementation of the Lox interpreter from [craftinginterpreters.com](https://craftinginterpreters.com).
There's no support for classes or closures as I wasn't feeling like adding them. Also, the script is currently hardcoded to `/LoxScripts/LoadLib.lox`

One cool feature which doesn't exist in the standard Lox is the ability to call **some** WinAPI functions. It's probably not very stable and you're limited to functions which do not have an ```_out_``` parameter 
For example, this function won't work:
```C
BOOL GetModuleInformation(
  [in]  HANDLE       hProcess,
  [in]  HMODULE      hModule,
  [out] LPMODULEINFO lpmodinfo,
  [in]  DWORD        cb
);
```

But as you can see in [loadlib.lox](/Lox/LoxScripts/LoadLib.lox)
You are able to call functions like ```GetCurrentProcess``` or  ```MessageBoxA```.

Example:
```
var k32lib = GetModuleHandle("kernel32.dll");

var getCurrentProcessProc = GetProcAddress(k32lib, "GetCurrentProcess");
var currentProcessHandle = WinAPIFunc(getCurrentProcessProc, "IntPtr");

var user32lib = GetModuleHandle("User32.dll");
var messageboxaproc = GetProcAddress(user32lib, "MessageBoxA");

WinAPIFunc(messageboxaproc, "int", "IntPtr",0 , "string", "Text", "string", "Caption", "uint", 0);
```
