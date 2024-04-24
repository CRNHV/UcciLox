using System.Collections.Generic;
using UcciLox.Statements;

namespace UcciLox.Functions;

internal sealed class Callable(Function declaration) : ICallable
{
    private readonly Function declaration = declaration;

    int ICallable.Arity => declaration.Params.Count;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        BlockEnvironment environment = new(interpreter.Global);
        for (int i = 0; i < declaration.Params.Count; i++)
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (Exceptions.Return ex)
        {
            return ex.value;
        }
        return null;
    }
}