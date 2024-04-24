using System.Collections.Generic;

namespace UcciLox.Statements;

internal sealed class Block(List<Statement> statements) : Statement
{
    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitBlockStatement(this);

    public List<Statement> statements = statements;
}