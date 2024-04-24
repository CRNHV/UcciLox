using System.Collections.Generic;
using UcciLox.Exceptions;
using UcciLox.Tokens;

namespace UcciLox;

internal sealed class BlockEnvironment(BlockEnvironment? enclosing)
{
    private readonly Dictionary<string, object> variables = [];
    private readonly BlockEnvironment? enclosing = enclosing;
    private static readonly object unassigned = new();

    public void Define(string name, object value) => variables[name] = value;

    public void Define(string name) => variables[name] = unassigned;

    public object Get(Token name)
    {
        if (variables.TryGetValue(name.Lexeme, out object? variable))
        {
            object? variableValue = variable;
            if (variableValue == unassigned)
            {
                throw new RuntimeError(name,
                    "Attempted to use unassigned variable '" + name.Lexeme + "'.");
            }

            return variableValue;
        }

        if (enclosing != null)
        {
            return enclosing.Get(name);
        }

        throw new RuntimeError(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Assign(Token name, object value)
    {
        if (variables.ContainsKey(name.Lexeme))
        {
            variables[name.Lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name,
            "Undefined variable '" + name.Lexeme + "'.");
    }

    public bool Contains(string lexeme)
    {
        if (variables.ContainsKey(lexeme))
        {
            return true;
        }

        if (enclosing != null && enclosing.Contains(lexeme))
        {
            return true;
        }

        return false;
    }
}