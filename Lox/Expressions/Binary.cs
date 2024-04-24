using UcciLox.Tokens;

namespace UcciLox.Expressions;

internal class Binary(Expression left, Token op, Expression right) : Expression
{
    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitBinaryExpression(this);

    public Expression Left = left;
    public Token Operator = op;
    public Expression Right = right;
}