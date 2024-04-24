namespace UcciLox.Expressions;

internal class Literal(object? value) : Expression
{
    public object? Value = value;

    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default
        => visitor.VisitLiteralExpression(this);
}