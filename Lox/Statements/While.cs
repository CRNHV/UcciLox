namespace UcciLox.Statements;

internal sealed class While(Expressions.Expression condition, Statement body) : Statement
{
    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitWhileStatement(this);

    public Expressions.Expression condition = condition;
    public Statement body = body;
}