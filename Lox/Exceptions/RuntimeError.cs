using System;
using UcciLox.Tokens;

namespace UcciLox.Exceptions;

internal class RuntimeError(Token token, string message) : Exception(message)
{
    public readonly Token token = token;
}