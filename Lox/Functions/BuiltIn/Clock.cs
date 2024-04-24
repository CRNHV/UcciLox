using System.Collections.Generic;

namespace UcciLox.Functions.BuiltIn;

internal sealed class Clock : ICallable
{
    int ICallable.Arity => 0;

    public object Call(Interpreter interpreter, List<object> arguments) => (double)System.Environment.TickCount;
}