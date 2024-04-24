# UcciLox

This is my implementation of the Lox interpreter from [craftinginterpreters.com](https://craftinginterpreters.com).
There's no support for classes or closures as I wasn't feeling like adding them.

I tried to add support for calling WinAPI functions via [`GetModuleHandle`](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/GetModHandle.cs) and [`GetProcAddress`](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/GetProcAddr.cs) [but unfortunately it didn't work out the way I wanted to.](https://github.com/CRNHV/UcciLox/blob/master/Lox/Functions/BuiltIn/WinAPIFunc.cs)
