using System.Collections.Generic;

namespace UcciLox.Functions;

internal interface ICallable
{
    object? Call(Interpreter interpreter, List<object> arguments);

    int Arity { get; }
}