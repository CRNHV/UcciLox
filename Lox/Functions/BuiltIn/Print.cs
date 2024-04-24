using System;
using System.Collections.Generic;

namespace UcciLox.Functions.BuiltIn;

internal sealed class Print : ICallable
{
    int ICallable.Arity => 1;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var argsAsString = arguments[0] as string;
        Console.WriteLine(argsAsString.Replace("\"", ""));
        return null;
    }
}