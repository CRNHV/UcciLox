namespace UcciLox.Statements;

internal sealed class ExpressionStatement(Expressions.Expression expression) : Statement
{
    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitExpressionStatement(this);

    public Expressions.Expression expression = expression;
}