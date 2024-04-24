using System.Collections.Generic;
using UcciLox.Tokens;

namespace UcciLox.Statements;

internal sealed class Function(Token name, List<Token> param, List<Statement> body) : Statement
{
    public Token Name { get; init; } = name;
    public List<Token> Params { get; init; } = param;
    public List<Statement> Body { get; init; } = body;

    internal override R? Accept<R>(IStatementVisitor<R?> visitor) where R : default => visitor.VisitFunctionStatement(this);
}