using UcciLox.Tokens;

namespace UcciLox.Expressions;

internal class Assign(Token name, Expression value) : Expression
{
    public Token Token = name;
    public Expression value = value;

    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitAssignExpression(this);
}