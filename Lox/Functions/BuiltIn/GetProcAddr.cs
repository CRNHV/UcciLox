using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UcciLox.Functions.BuiltIn;
internal sealed class GetProcAddr : ICallable
{
    [DllImport("kernel32.dll")]
    static extern nint GetProcAddress(nint hModule, string functionName);

    int ICallable.Arity => 2;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        nint hModule = (nint)arguments[0];
        string lpProcName = (string)arguments[1];
        nint proc = GetProcAddress(hModule, lpProcName);
        return proc;
    }
}