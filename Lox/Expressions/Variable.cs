using UcciLox.Tokens;

namespace UcciLox.Expressions;

internal class Variable(Token name) : Expression
{
    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitVariableExpression(this);

    public Token Name = name;
}