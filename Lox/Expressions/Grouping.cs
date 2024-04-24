namespace UcciLox.Expressions;

internal class Grouping(Expression expression) : Expression
{
    internal override R? Accept<R>(IExpressionVisitor<R?> visitor) where R : default => visitor.VisitGroupingExpression(this);

    public Expression Expression = expression;
}