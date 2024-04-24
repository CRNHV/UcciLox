using UcciLox.Tokens;

namespace UcciLox.Expressions;

internal class Unary(Token op, Expression right) : Expression
{
    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitUnaryExpression(this);

    public Token Operator = op;
    public Expression Right = right;
}