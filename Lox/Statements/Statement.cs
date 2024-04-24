namespace UcciLox.Statements;

internal abstract class Statement
{
    internal abstract R? Accept<R>(IStatementVisitor<R?> visitor);
}