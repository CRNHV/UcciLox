namespace UcciLox.Expressions;

internal abstract class Expression
{
    internal abstract R? Accept<R>(IExpressionVisitor<R?> visitor);
}