namespace UcciLox.Statements;

internal sealed class Break : Statement
{
    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitBreakStatement(this);
}