using System;

namespace UcciLox.Exceptions;

internal class Return(object? value) : Exception
{
    public readonly object? value = value;
}