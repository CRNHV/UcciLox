namespace UcciLox.Statements;

internal sealed class If(Expressions.Expression condition, Statement thenBranch, Statement? elseBranch) : Statement
{
    public Expressions.Expression condition = condition;
    public Statement thenBranch = thenBranch;
    public Statement? elseBranch = elseBranch;

    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitIfStatement(this);
}