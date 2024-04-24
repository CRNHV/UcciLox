using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UcciLox.Functions.BuiltIn;
internal sealed class GetModHandle : ICallable
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint GetModuleHandle(string lpModuleName);

    int ICallable.Arity => 1;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        string libraryPath = arguments[0] as string;
        nint library = GetModuleHandle(libraryPath);

        return library;
    }
}