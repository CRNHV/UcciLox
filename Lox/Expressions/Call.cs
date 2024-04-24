using System.Collections.Generic;
using UcciLox.Tokens;

namespace UcciLox.Expressions;

internal class Call(Expression callee, Token paren, List<Expression> arguments) : Expression
{
    public Expression Callee = callee;
    public Token Paren = paren;
    public List<Expression> Arguments = arguments;

    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitCallExpression(this);
}