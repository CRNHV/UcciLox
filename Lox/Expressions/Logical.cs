using UcciLox.Tokens;

namespace UcciLox.Expressions;

internal class Logical(Expression left, Token op, Expression right) : Expression
{
    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitLogicalExpression(this);

    public Expression left = left;
    public Token op = op;
    public Expression right = right;
}