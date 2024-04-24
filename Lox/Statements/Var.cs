using UcciLox.Tokens;

namespace UcciLox.Statements;

internal sealed class Var(Token name, Expressions.Expression initializer) : Statement
{
    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitVarStatement(this);

    public Token name = name;
    public Expressions.Expression initializer = initializer;
}