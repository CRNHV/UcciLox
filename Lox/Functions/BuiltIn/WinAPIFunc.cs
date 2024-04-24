using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace UcciLox.Functions.BuiltIn;
internal sealed class WinAPIFunc : ICallable
{
    int ICallable.Arity => -1;

    delegate void NativeFunction(IntPtr handle, string text, string caption, int type);

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
}